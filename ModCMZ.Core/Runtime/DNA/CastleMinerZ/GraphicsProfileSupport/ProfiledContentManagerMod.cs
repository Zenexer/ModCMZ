//#define USE_ABSOLUTE_ROOTS

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DNA.CastleMinerZ.GraphicsProfileSupport;
using Microsoft.Xna.Framework.Content;

namespace ModCMZ.Core.Runtime.DNA.CastleMinerZ.GraphicsProfileSupport
{
	public static class ProfiledContentManagerMod
	{
		private static string s_RootDirectory;

		public static void Ctor(ProfiledContentManager instance)
		{

			var root = Path.Combine(App.Current.TargetDirectory.FullName, "Content");
			instance.RootDirectory = root;
			s_RootDirectory = root;

			// It appends the directory names to RootDirectory for now, but in case that ever changes, we have this:
#if USE_ABSOLUTE_ROOTS
			var reachRoot = typeof(ProfiledContentManager).GetField("_reachRoot", BindingFlags.NonPublic | BindingFlags.Instance);
			var hiDefRoot = typeof(ProfiledContentManager).GetField("_hiDefRoot", BindingFlags.NonPublic | BindingFlags.Instance);
			reachRoot.SetValue(instance, Path.Combine(root, "ReachContent"));
			reachRoot.SetValue(instance, Path.Combine(root, "HiDefContent"));
#endif
		}

		public static T Load<T>(ProfiledContentManager instance, string assetName)
		{
			if (instance.RootDirectory != s_RootDirectory)
			{
				instance.RootDirectory = s_RootDirectory;
			}

			var tokens = assetName.Split('/', '\\');
			var assetFile = tokens.Last();
			var pathTokens = new string[tokens.Length - 1];
			Array.Copy(tokens, pathTokens, pathTokens.Length);
			var assetPath = pathTokens.Any() ? Path.Combine(pathTokens) : "";

			var defaultPath = Path.Combine("Content", assetPath);
			var isDefault = Directory.Exists(defaultPath) && Directory.EnumerateFiles(defaultPath, assetFile + ".*").Any();

			var hidefPath = Path.Combine("Content", "HiDefContent", assetPath);
			var isHiDef = GraphicsProfileManager.Instance.IsHiDef && Directory.Exists(hidefPath) && Directory.EnumerateFiles(hidefPath, assetFile + ".*").Any();

			var reachPath = Path.Combine("Content", "ReachContent", assetPath);
			var isReach = Directory.Exists(reachPath) && Directory.EnumerateFiles(reachPath, assetFile + ".*").Any();

			if (isDefault)
			{
				try
				{
					return instance.BaseLoad<T>(assetName);
				}
				catch (ContentLoadException)
				{
					Debug.WriteLine("Asset type mismatch.  Requested type: {0} from {1}", typeof(T).FullName, typeof(T).Assembly.Location);
					throw;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(@"Asset {0} [{3}, {4}] exists as Content{1}{0}.*, but loading failed: {2}", assetName, Path.DirectorySeparatorChar, ex.Message, typeof(T).FullName, typeof(T).Assembly.GetName().Name);

					if (!isHiDef && !isReach)
					{
						throw;
					}
				}
			}

			if (isHiDef)
			{
				// We might need to change this to use \ instead of system path separator for Linux compatibility, oddly enough.
				return instance.BaseLoad<T>(Path.Combine("HiDefContent", assetName));
			}

			if (isReach)
			{
				// idem
				return instance.BaseLoad<T>(Path.Combine("ReachContent", assetName));
			}

			throw new FileNotFoundException(string.Format("Could not location asset: {0}", assetName));
		}

		public static T BaseLoad<T>(this ProfiledContentManager instance, string assetName)
		{
			return ((dynamic)instance).BaseLoad<T>(assetName);
		}
	}
}
