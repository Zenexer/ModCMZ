using System;
using System.Windows.Forms;
using DNA.Text;
using DNA.Reflection;
using DNA.CastleMinerZ;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using ModCMZ.Bootstrap;
using DNA.Net.GamerServices;
using DNA.Diagnostics.IssueReporting;
using ModCMZ.Core.Runtime.DNA.CastleMinerZ.GraphicsProfileSupport;
using ModCMZ.Core.Net;
using DNA.Net.GamerServices.LidgrenProvider;
using DNA.Net.MatchMaking;
using DNA.Collections;
#if STEAM
using DNA.Distribution;
using DNA.Distribution.Steam;
using DNA.CastleMinerZ.Net.Steam;
#endif

namespace ModCMZ.Core.Runtime.DNA.CastleMinerZ
{
    public static class ProgramMod
	{
		// Taken from the last paramater from this line in Program.Main:
		//   NetworkSession.NetworkSessionServices = new SteamNetworkSessionServices(onlineServices.SteamAPI, productID, 3);
		public const int NetworkProtocolVersion = 3;
		public const uint SteamAppId = 253430;
		public const string ProductId = "FAE62948-F4E6-4F18-9D73-ED507466057F";

		public static void Main()
		{
			//AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			CommandLineArgs.ProcessArguments();

			var app = App.CreateInternal();
			app.Info("Entered injected code successfully!");
			app.OnDomainReady();
			app.OnLaunched();

			var productId = Guid.Parse(ProductId);
			var version = typeof(CastleMinerZGame).Assembly.GetName().Version;

			// This next line is normally called via CommonAssembly.Initialize(), but that will load ModCMZ instead of CastleMinerZ if we call it here.
			// Instead, pass the assemblies that would normally be passed if CommonAssembly.Initialize() were invoked directly from CastleMinerZ.
			ReflectionTools.RegisterAssembly(typeof(CastleMinerZGame).Assembly, typeof(CommonAssembly).Assembly);

			UpdatePaths();

#if STEAM
			SteamOnlineServices onlineServices = null;
			try
			{
				var cwd = Environment.CurrentDirectory;
				Environment.CurrentDirectory = Path.GetDirectoryName(typeof(ProgramMod).Assembly.Location);
				try
				{
					var steamIdFile = Path.Combine(Environment.CurrentDirectory, "steam_appid.txt");
					if (!File.Exists(steamIdFile))
					{
						File.WriteAllText(steamIdFile, SteamAppId.ToString());
					}

					var steamId = uint.Parse(File.ReadAllText(steamIdFile).Trim());
					onlineServices = new SteamOnlineServices(productId, steamId);
				}
				finally
				{
					Environment.CurrentDirectory = cwd;
				}

				if (!onlineServices.OperationWasSuccessful)
				{
					SteamErrorCode errorCode = onlineServices.ErrorCode;
					app.Fatal("Steam error: {0}", Enum.GetName(typeof(SteamErrorCode), errorCode));
					return;
				}

				CastleMinerZGame.GlobalSettings.Load();

				NetworkSession.StaticProvider = new SteamNetworkSessionStaticProvider(onlineServices.SteamAPI);
				NetworkSession.NetworkSessionServices = new SteamNetworkSessionServices(onlineServices.SteamAPI, productId, 3);
				IssueReporter issueReporter = new IssueReporter();

				CastleMinerZGame.TrialMode = false;

				using (CastleMinerZGame game = new CastleMinerZGame())
				{
					UpdatePaths();
					game.Content = new ModContentManager(game.Services, "ReachContent", "HiDefContent", CastleMinerZGame.GlobalSettings.TextureQualityLevel);
					game.Content.RootDirectory = "Content";
					game.LicenseServices = onlineServices;
					var y = ReflectionTools.GetAssemblies();
					var x = ReflectionTools.GetTypes(new Filter<Type>((Type type) => type.IsSubclassOf(typeof(global::DNA.Net.Message)) && !type.IsAbstract));
					game.Run();
				}
			}
			finally
            {
				onlineServices?.Dispose();
            }
#else
			using (ModOnlineServices onlineServices = new ModOnlineServices(productId))
			{
				CheckAssemblies();
				CastleMinerZGame.GlobalSettings.Load();

				NetworkSession.StaticProvider = new LidgrenNetworkSessionStaticProvider();
				NetworkSession.NetworkSessionServices = new OnlineNetworkSessionServices(Environment.UserName, "waffles", productId, NetworkProtocolVersion);

				CastleMinerZGame.TrialMode = false;

				using (CastleMinerZGame game = new CastleMinerZGame())
				{
					UpdatePaths();
					game.Content = new ModContentManager(game.Services, "ReachContent", "HiDefContent", CastleMinerZGame.GlobalSettings.TextureQualityLevel);
					game.Content.RootDirectory = "Content";
					game.LicenseServices = onlineServices;
					game.Run();
				}
			}
#endif
		}

		private static void UpdatePaths()
		{
			string titleLocation = App.Current.TargetDirectory.FullName;
			var titleLocationType = typeof(TitleContainer).Assembly.GetType("Microsoft.Xna.Framework.TitleLocation");
			var titleLocationField = titleLocationType.GetField("_titleLocation", BindingFlags.Static | BindingFlags.NonPublic);
			titleLocationField.SetValue(null, titleLocation);
		}

		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
		{
			AssemblyName name = new AssemblyName(e.Name);

			string appDir = App.Current.AppDirectory.FullName;
			string mirrorDir = App.Current.MirrorDirectory.FullName;
			string targetDir = App.Current.TargetDirectory.FullName;

			List<string> paths = new List<string>();
			List<string> names = new List<string>(3) { name.Name };

			if (name.Name.EndsWith(".resources"))
			{
				names.Add(Path.Combine(name.CultureName, name.Name));
				names.Add(name.Name.Remove(name.Name.Length - ".resources".Length));
			}

			foreach (string n in names)
			{
				paths.AddRange(new[] {
					Path.Combine(mirrorDir, n + ".dll"),
					Path.Combine(mirrorDir, n + ".exe"),
					Path.Combine(appDir, n + ".dll"),
					Path.Combine(appDir, n + ".exe"),
					Path.Combine(targetDir, n + ".dll"),
					Path.Combine(targetDir, n + ".exe"),
				});
			}

			foreach (string path in paths)
			{
				if (File.Exists(path))
				{
					App.Current.Info("Loading assembly {0} from {1}", name, path);
					return Assembly.LoadFrom(path);
				}
			}

			return null;
			//throw new FileNotFoundException("Unable to load type because unable to find any of the following files:\n" + string.Join("\n", paths), paths[0]);
		}
	}
}
