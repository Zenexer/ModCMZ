using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ModCMZ.Bootstrap;
using ModCMZ.Core.Game;
using ModCMZ.Core.Mods;
using diag = System.Diagnostics;
using ModCMZ.Core.Mods.Core;
using System.Threading;
using System.Globalization;


namespace ModCMZ.Core
{
	public class App : MarshalByRefObject
	{
		private static readonly string[] s_TargetAssemblies = new[]
		{
			"DNA.Common.dll",
			"CastleMinerZ.exe",
		};

		private static string[] s_ReferenceAssemblies = null;

		private static string[] s_Dependencies = new[]
		{
			"Facebook.dll",
			"Services.Client.dll",
			"DNA.Steam.dll",
		};

		public static App Current { get; private set; }

		public DirectoryInfo MirrorDirectory { get; set; }
		public DirectoryInfo TargetDirectory { get; set; }
		public FileInfo[] TargetAssemblies { get; set; }
		public string[] ProgramArgs { get; set; }
		internal Injection Injection { get; private set; }
		public AppDomain Domain { get; private set; }
		public Dictionary<string, Assembly> Assemblies { get; private set; }
		public DirectoryInfo AppDirectory { get; private set; }
		public DirectoryInfo XnaDirectory { get; private set; }
		public DirectoryInfo ModDirectory { get; private set; }
		public GameApp Game { get; private set; }
		public IMod[] Mods { get; private set; }

		private CoreMod m_CoreMod = null;
		public CoreMod CoreMod
		{
			get
			{
				if (m_CoreMod == null)
				{
					m_CoreMod = GetMod<CoreMod>();
				}

				return m_CoreMod;
			}
		}

		public string[] SearchPath
		{
			get
			{
				return new[]
				{
					TargetDirectory.FullName,
					AppDirectory.FullName,
					XnaDirectory.FullName,
				};
			}
		}

		public string Id
		{
			get
			{
				return AppDomain.CurrentDomain.FriendlyName;
			}
		}

		public App()
		{
#if !DEBUG
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
#endif
			Application.EnableVisualStyles();

			var xnaDir = Path.GetDirectoryName(typeof(Microsoft.Xna.Framework.Game).Assembly.Location);
			diag::Debug.Assert(xnaDir != null, "XNA framework directory must be located");
			XnaDirectory = new DirectoryInfo(xnaDir);

			var assembly = Assembly.GetExecutingAssembly();
			Assemblies = new Dictionary<string, Assembly>
			{
				{ assembly.GetName().Name, assembly },
			};

#if DEBUG
			diag::Debug.Listeners.Add(new ConsoleTraceListener(true));
#elif TRACE
			Trace.Listeners.Add(new ConsoleTraceListener(true));
#endif
		}

		public static void SetReferencedAssemblies(string[] referencedAssemblies)
		{
			s_ReferenceAssemblies = referencedAssemblies;
		}

		public static App Create()
		{
			var app = new App();
			app.Initialize();
			return Current = app;
		}

		public static App CreateInternal()
		{
			var app = Create();
			app.Domain = AppDomain.CurrentDomain;
			app.Game = new GameApp();
			app.Mods = app.LoadMods();
			return app;
		}

		public T GetMod<T>() where T : class, IMod
		{
			foreach (var mod in Mods.Select(m => m as T))
			{
				if (mod != null)
				{
					return mod;
				}
			}

			return null;
		}

		public IEnumerable<Assembly> GetModAssemblies() => Mods.Select(m => m.GetType().Assembly).Distinct();

		public IEnumerable<Type> GetModTypes() => GetModAssemblies().SelectMany(a => a.GetTypes());

		public Type GetModType(string fullName) => GetModTypes().Where(x => x.FullName == fullName).FirstOrDefault();

		public IEnumerable<Type> GetModTypes<T>() => GetModTypes().Where(t => t.IsVisible && !t.IsAbstract && typeof(T).IsAssignableFrom(t));

		public IEnumerable<T> InstantiateModTypes<T>() where T : class
        {
			foreach (var type in GetModTypes<T>())
            {
				var ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, Type.EmptyTypes, null);
				yield return (T)ctor.Invoke(new object[0]);
            }
        }

		[Conditional("DEBUG")]
		public void Debug(string format, params object[] args)
		{
			diag::Debug.WriteLine(string.Format("[{0}] {1}", Id, format), args);
		}

		[Conditional("TRACE")]
		public void Info(string format, params object[] args)
		{
			Trace.TraceInformation(string.Format("[{0}] {1}", Id, format), args);
		}

		public void Warning(string format, params object[] args)
		{
			Trace.TraceWarning(string.Format("[{0}] {1}", Id, format), args);
		}

		public void Error(string format, params object[] args)
		{
			var message = string.Format(string.Format("[{0}] {1}", Id, format), args);
			MessageBox.Show(message, "ModCMZ: Error", MessageBoxButtons.OK);
			Trace.TraceWarning(message);
		}

		public void Fatal(string format, params object[] args)
		{
			var message = string.Format(string.Format("[{0}] {1}", Id, format), args);
			MessageBox.Show(message, "ModCMZ: Fatal Error", MessageBoxButtons.OK);
			Trace.TraceError(message);
			Environment.Exit(1000);
		}

		public void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			const string FORMAT = "Unhandled exception:\r\n{0}";

			var message = "";

			for (var ex = e.ExceptionObject as Exception; ex != null; ex = ex.InnerException)
            {
				if (message != "")
                {
					message += "\r\n\r\ncaused by:\r\n";
                }

				message += ex;
            }

			File.WriteAllText(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "crash.txt"), message);

			if (e.IsTerminating)
			{
				Fatal(FORMAT, message);
			}
			else
			{
				Error(FORMAT, message);
			}
		}

		private void OnCouldNotFindApp()
		{
			Fatal("Could not find CastleMiner Z.  Is it installed?");
		}

		private void Initialize()
		{
			AppDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			MirrorDirectory = new DirectoryInfo(Path.Combine(AppDirectory.FullName, "Mirror"));
			ModDirectory = new DirectoryInfo(Path.Combine(AppDirectory.FullName, "Mod"));

			if (!MirrorDirectory.Exists)
			{
				MirrorDirectory.Create();
			}
			if (!ModDirectory.Exists)
			{
				ModDirectory.Create();
			}

			TargetDirectory = null;
			FindSteamFolder();
			FindClickOnceFolder();
			if (TargetDirectory == null)
			{
				OnCouldNotFindApp();
				return;
			}

			Environment.CurrentDirectory = TargetDirectory.FullName;

			var assemblies = new List<FileInfo>
			{
				new FileInfo(typeof(App).Assembly.Location),
			};

			foreach (var file in s_TargetAssemblies.Select(assembly => TargetDirectory.GetFiles(assembly).FirstOrDefault()))
			{
				if (file == null)
				{
					OnCouldNotFindApp();
					return;
				}

				assemblies.Add(file);
			}

			TargetAssemblies = assemblies.ToArray();
		}

		[Conditional("STEAM")]
		private void FindSteamFolder()
		{
			if (TargetDirectory != null)
			{
				return;
			}

			foreach (var dir in new[]
			{
				Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Vanilla"),
				@"C:\Program Files (x86)\Steam\steamapps\common",
				@"F:\Program Files (x86)\Steam\steamapps\common",
				@"C:\Program Files\Steam\steamapps\common",
				@"D:\SteamLibrary\SteamApps\common",
				@"D:\Games\Steam\steamapps\common",
			})
			{
				if (!Directory.Exists(dir))
				{
					continue;
				}

				var cmzDir = Path.Combine(dir, "CastleMiner Z");

				if (Directory.Exists(cmzDir))
				{
					TargetDirectory = new DirectoryInfo(cmzDir);
					return;
				}
			}
		}

		[Conditional("CLICKONCE")]
		private void FindClickOnceFolder()
		{
			if (TargetDirectory != null)
			{
				return;
			}

			var dir = new DirectoryInfo(Environment.ExpandEnvironmentVariables(Path.Combine("%LocalAppData%", "Apps", "2.0")));
			if (!dir.Exists)
			{
				return;
			}

			for (var i = 0; i < 2; i++)
			{
				dir = dir.EnumerateDirectories().FirstOrDefault(d => d.Name.Contains("."));
				if (dir == null)
				{
					return;
				}
			}

			dir = dir.EnumerateDirectories("cast..tion_*").LastOrDefault();
			if (dir == null)
			{
				return;
			}

			TargetDirectory = dir;
		}

		/// <summary>
		/// Called from another application domain.
		/// </summary>
		private void InitializeExternal()
		{
			Current = this;
			Domain = AppDomain.CurrentDomain;
		}

		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
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
					Path.Combine(TargetDirectory.FullName, n + ".dll"),
					Path.Combine(TargetDirectory.FullName, n + ".exe"),
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
		}

		private IMod[] LoadMods()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			var modTypes = LoadModsFromAssembly(Assembly.GetExecutingAssembly());
			var myDir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

			foreach (var file in myDir.EnumerateFiles("ModCMZ.Mods.*.dll").Concat(ModDirectory.EnumerateFiles("*.dll")))
			{
				try
				{
					modTypes = modTypes.Concat(LoadModsFromAssembly(Assembly.LoadFrom(file.FullName)));
				}
				catch (Exception ex)
				{
					Debug("Unable to load mods from {0}:\r\n{1}", file.FullName, ex);
				}
			}

			return LoadModsFromTypes(modTypes);
		}

		private IMod[] LoadModsFromTypes(IEnumerable<Type> modTypes)
		{
			var mods = new List<IMod>();
			foreach (var type in modTypes)
			{
				try
				{
					var ctor = type.GetConstructor(Type.EmptyTypes);
					if (ctor == null)
					{
						continue;
					}

					var mod = ctor.Invoke(new object[0]) as IMod;
					if (mod == null)
					{
						continue;
					}

					Info("Loaded mod {0} version {1} by {2}", mod.Name, mod.Version, mod.Author);
					mods.Add(mod);
				}
				catch (Exception ex)
				{
					Debug("Unable to instantiate mod {0}:\r\n{1}", type.FullName, ex);
				}
			}

			return mods.ToArray();
		}

		private IEnumerable<Type> LoadModsFromAssembly(Assembly assembly)
		{
			var mods = Enumerable.Empty<Type>();

			var attribute = assembly.GetCustomAttribute<ModContainerAttribute>();
			if (attribute == null)
			{
				foreach (var module in assembly.Modules)
				{
					attribute = module.GetCustomAttribute<ModContainerAttribute>();
					if (attribute == null)
					{
						continue;
					}

					mods = mods.Concat(LoadModsFromModule(attribute, module));
				}
			}
			else
			{
				foreach (var module in assembly.Modules)
				{
					mods = mods.Concat(LoadModsFromModule(attribute, module));
				}
			}

			return mods;
		}

		private IEnumerable<Type> LoadModsFromModule(ModContainerAttribute attribute, Module module)
		{
			return
				from t in module.GetTypes()
				where t.GetCustomAttribute<ModAttribute>() != null
				select t;
		}

		public void SyncFiles(string sourceDir, string targetDir, IEnumerable<string> files)
		{
			foreach (var name in files)
			{
				var source = new FileInfo(Path.Combine(sourceDir, name));
				var target = new FileInfo(Path.Combine(targetDir, name));

				if (!source.Exists)
				{
					throw new Exception(string.Format("Missing file: {0}", source.FullName));
				}

				if (target.Exists && target.Length == source.Length && target.LastWriteTimeUtc >= source.LastWriteTimeUtc)
				{
					continue;
				}

				source.CopyTo(target.FullName, true);
			}
		}

		public void Run()
		{
			Info("Copying dependencies to mirror directory");
			SyncFiles(TargetDirectory.FullName, MirrorDirectory.FullName, s_Dependencies);

			Info("Preparing external application domain");
			//Environment.SetEnvironmentVariable("PATH", string.Join(";", Environment.GetEnvironmentVariable("PATH"), MirrorDirectory.FullName, TargetDirectory.FullName));
			Environment.CurrentDirectory = TargetDirectory.FullName;
			var domainSetup = new AppDomainSetup
			{
				ApplicationName = "CastleMinerZ.exe",
				ApplicationBase = AppDirectory.FullName,
				PrivateBinPath = "Mirror",
				//PrivateBinPathProbe = "",
			};

			Domain = AppDomain.CreateDomain("CastleMinerZ.exe", null, domainSetup);

			try
			{
				Info("Loading mods");
				Mods = LoadMods();

				var modAssemblies = (from m in Mods select m.GetType().Assembly).Distinct().ToArray();
				var types = GetModTypes();
				s_ReferenceAssemblies = s_ReferenceAssemblies.Concat(from a in modAssemblies select a.Location).Distinct().ToArray();

				Injection = new Injection(SearchPath);
				foreach (var assembly in Mods.Select(m => m.GetType().Assembly).Distinct())
				{
					Injection.AddInjectors(assembly);
					Injection.AddReplacers(assembly);
				}

				Info("Initializing bootstrapper in external application domain");
				var deref = (Deref)Domain.CreateInstanceFromAndUnwrap(typeof(Deref).Assembly.Location, typeof(Deref).FullName, false, BindingFlags.Default, null, new object[0], null, null);

				Info("Injecting assemblies");
				foreach (var assemblyFile in s_TargetAssemblies)
				{
					var file = Injection.Inject(assemblyFile);
					deref.LoadAssembly(file.FullName);
				}

				Info("Loading referenced assemblies");
				foreach (var file in s_ReferenceAssemblies)
				{
					deref.LoadAssembly(file);
				}

				deref.ListAssemblies();

				Info("Launching CastleMiner Z");
				deref.Run();
			}
			finally
			{
				// Prevents some issues where the vhost.exe debug process doesn't release injected DLL files.
				AppDomain.Unload(Domain);
			}
		}

		[Obsolete]
		public void RunExternal()
		{
			Info("Initializing GameApp");
			Game = new GameApp();

			Info("Launching CastleMiner Z");
			try
			{
				OnLaunching();

				Debug("Assembly location: {0}", typeof(DNA.CastleMinerZ.CastleMinerZGame).Assembly.Location);
				typeof(AppDomain).GetMethodEx("nExecuteAssembly").Invoke(Domain, new object[] { typeof(DNA.CastleMinerZ.CastleMinerZGame).Assembly, ProgramArgs });
				// Without reflection, but less reliable:
				//AppDomain.CurrentDomain.ExecuteAssemblyByName(typeof(DNA.CastleMinerZ.CastleMinerZGame).Assembly.FullName, ProgramArgs);
			}
			catch (Exception ex)
			{
				Fatal("CastleMiner Z crashed:\r\n\r\n{0}", ex);
			}
		}

		private void OnModEventException(IMod mod, string evt, Exception ex)
		{
			Error("Mod {0} failed to handle event {1}:\r\n{2}", mod.Name, evt, ex);
		}

		internal void OnGameReady(GameApp game)
		{
			foreach (var mod in Mods)
			{
				try
				{
					mod.OnGameReady(game);
				}
				catch (Exception ex)
				{
					OnModEventException(mod, "GameReady", ex);
				}
			}

			foreach (var mod in Mods)
			{
				try
				{
					mod.OnComponentsReady();
				}
				catch (Exception ex)
				{
					OnModEventException(mod, "ComponentsReady", ex);
				}
			}
		}

		internal void OnLaunching()
		{
			foreach (var mod in Mods)
			{
				try
				{
					mod.OnLaunching();
				}
				catch (Exception ex)
				{
					OnModEventException(mod, "Launching", ex);
				}
			}
		}

		internal void OnLaunched()
		{
			foreach (var mod in Mods)
			{
				try
				{
					mod.OnLaunched();
				}
				catch (Exception ex)
				{
					OnModEventException(mod, "Launched", ex);
				}
			}
		}

		internal void OnDomainReady()
		{
			foreach (var mod in Mods)
			{
				try
				{
					mod.OnDomainReady();
				}
				catch (Exception ex)
				{
					OnModEventException(mod, "DomainReady", ex);
				}
			}
		}
	}
}
