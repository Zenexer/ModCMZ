using DNA.CastleMinerZ;
using DNA.CastleMinerZ.GraphicsProfileSupport;
using DNA.Drawing;
using DNA.Drawing.Imaging;
using DNA.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ModCMZ.Core.Mods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModCMZ.Core.Runtime.DNA.CastleMinerZ.GraphicsProfileSupport
{
	public class ModContentManager : ProfiledContentManager
	{
		/// <summary>
		/// Gets a list of image file patterns for user-overridden textures
		/// </summary>
		public static ReadOnlyCollection<string> ImageFilePatterns { get; } = new[]
		{
			"*.dds",
			"*.tiff",
			"*.tif",
			"*.png",
			"*.jpg",
			"*.jpeg",
			"*.bmp",
		}.ToList().AsReadOnly();

		/// <summary>
		/// Gets or sets the path to the HiDef content folder
		/// </summary>
		public string HiDefRoot { get; set; }

		/// <summary>
		/// Gets or sets the path to the Reach content folder
		/// </summary>
		public string ReachRoot { get; set; }

		/// <summary>
		/// TODO: Unknown purpose
		/// </summary>
		public int TextureLevel { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to check for the existence of user-supplied textures before resorting to the default XNB textures
		/// </summary>
		public bool IsUserOverridingTextures { get; set; }

		public string ProfileRoot => GraphicsProfileManager.Instance.IsHiDef ? HiDefRoot : ReachRoot;

		/// <summary>
		/// Gets or sets the path to the user-supplied texture folder
		/// </summary>
		public string UserTextureFolder { get; set; }
		public Dictionary<string, string> AvailableUserTextures { get; set; } = new Dictionary<string, string>();
		public Dictionary<string, Texture2D> PreloadedUserTextures { get; set; } = new Dictionary<string, Texture2D>();

		private readonly ConditionalWeakTable<string, IMod> _modContentClaims = new ConditionalWeakTable<string, IMod>();
		private Stopwatch _loadingTimer;

		public string FullRootDirectory => _fullRootDirectory.Value;
		private Lazy<string> _fullRootDirectory => new Lazy<string>(() =>
			(string)typeof(ContentManager)
				.GetField("fullRootDirectory", BindingFlags.Instance | BindingFlags.NonPublic)
				.GetValue(this)
		);

		private readonly Dictionary<string, string> _resolvedAssetNames = new Dictionary<string, string>();

		public ModContentManager(IServiceProvider services, string reachRoot, string hiDefRoot, int textureLevel)
			//: base(services, reachRoot, hiDefRoot, textureLevel)
			: base(services, reachRoot, hiDefRoot, textureLevel)
		{
			var argsType = typeof(CastleMinerZGame).Assembly.GetType("DNA.CastleMinerZ.CastleMinerZArgs");
			var instanceField = argsType.GetField("Instance");
			var argsInstance = instanceField.GetValue(null);
			var textureFolderField = argsType.GetField("TextureFolder");
			var textureFolder = (string)textureFolderField.GetValue(argsInstance) ?? Path.Combine(Path.GetDirectoryName(typeof(ModContentManager).Assembly.Location), "CustomTextures");

			HiDefRoot = hiDefRoot;
			ReachRoot = reachRoot;
			TextureLevel = textureLevel;
			_loadingTimer = Stopwatch.StartNew();
			UserTextureFolder = textureFolder;

			if (textureFolder == null || !Directory.Exists(textureFolder))
			{
				return;
			}

			//
			// User is attempting to override textures
			//

			var textureFolderPrefixLength = textureFolder.Length + 1;

			IsUserOverridingTextures = false;
			foreach (string searchPattern in ImageFilePatterns)
			{
				var files = PathTools.RecursivelyGetFiles(textureFolder, searchPattern);
				if (files == null || files.Length == 0)
                {
					continue;
                }

				// At least one override
				IsUserOverridingTextures = true;

				foreach (var file in files)
				{
					var key = Path.ChangeExtension(file, null).Substring(textureFolderPrefixLength).ToLower();

					if (key.EndsWith("_m"))
					{
						key = key.Remove(key.Length - 2);
					}

					if (!AvailableUserTextures.ContainsKey(key))
					{
						AvailableUserTextures.Add(key, file);
					}
				}
			}
		}

		public void ClaimContent(IMod mod, string assetName)
        {
			if (_modContentClaims.TryGetValue(assetName, out var existingMod))
            {
				if (existingMod == mod)
                {
					return;
                }

				throw new Exception($"{mod.Id} tried to claim {assetName}, but it's already claimed by {existingMod.Id}");
            }

			_modContentClaims.Add(assetName, mod);
        }

		public Texture2D TryLoadFromFile(string assetName)
		{
			if (PreloadedUserTextures.TryGetValue(assetName, out var textured))
			{
				return textured;
			}

            var lowerAssetName = assetName.ToLower();

            if (
				!AvailableUserTextures.TryGetValue(lowerAssetName, out var userTextureFile) &&
				!AvailableUserTextures.TryGetValue(Path.Combine(ProfileRoot.ToLowerInvariant(), lowerAssetName), out userTextureFile)
			)
            {
                return null;
            }

            lowerAssetName = userTextureFile.ToLower();
            var makeMipmaps = Path.ChangeExtension(lowerAssetName, null).EndsWith("_m");
            var normalizeMipmaps = false;

            if (makeMipmaps)
            {
                normalizeMipmaps = lowerAssetName.Contains("_nrm_") || lowerAssetName.Contains("_n_");
            }

			// Why is this lock necessary?
			while (!GraphicsDeviceLocker.Instance.TryLockDeviceTimed(ref _loadingTimer))
            {
				Thread.Sleep(10);
			}

			try
			{
				textured = TextureLoader.LoadFromFile(CastleMinerZGame.Instance.GraphicsDevice, userTextureFile, makeMipmaps, normalizeMipmaps);
			}
			finally
			{
				GraphicsDeviceLocker.Instance.UnlockDevice();
			}

            PreloadedUserTextures.Add(assetName, textured);

            return textured;
		}

		public override T Load<T>(string assetName)
		{
			if (_resolvedAssetNames.TryGetValue(assetName, out var knownAssetName))
            {
				return base.Load<T>(knownAssetName);
            }

			var isTexture = typeof(T) == typeof(Texture) || typeof(T).IsSubclassOf(typeof(Texture));

			_loadingTimer.Restart();

			if (IsUserOverridingTextures && isTexture)
			{
				var texture = TryLoadFromFile(assetName);

				if (texture != null)
				{
					return (T)(object)texture;
				}
			}

			var prefix = ProfileRoot + @"\";

			var resolvedAssetNames = new List<string>();

			if (TextureLevel > 1 && isTexture)
			{
				var levelSuffix = "_L" + TextureLevel;
				resolvedAssetNames.Add(assetName + levelSuffix);
				resolvedAssetNames.Add(prefix + assetName + levelSuffix);
			}

			resolvedAssetNames.Add(assetName);
			resolvedAssetNames.Add(prefix + assetName);

			var resolvedAssets = resolvedAssetNames.Select(x => (resolvedAssetName: x, fileName: Path.Combine(FullRootDirectory, x + ".xnb"))).ToArray();

			foreach (var (resolvedAssetName, fileName) in resolvedAssets)
			{
				if (File.Exists(fileName))
                {
					var value = base.Load<T>(resolvedAssetName);
					_resolvedAssetNames.Add(assetName, resolvedAssetName);  // Ensure this runs after it loads so we don't store something that throws an exception
					return value;
				}
			}

			throw new FileNotFoundException($"Content not found: {assetName} -> {string.Join(", ", resolvedAssets.Select(x => x.fileName))}");
		}

        protected override Stream OpenStream(string assetName)
		{
			if (_modContentClaims.TryGetValue(assetName, out var mod))
            {
				return mod.OpenContentStream(assetName);
            }

			var delimPos = assetName.IndexOf('\\');
			var modName = delimPos >= 3 ? assetName.Remove(delimPos) : null;

			if (modName != null && App.Current.Mods.TryGetValue(modName, out mod))
            {
				return mod.OpenContentStream(assetName.Substring(delimPos - 1));
            }

			return base.OpenStream(assetName);
        }
    }
}
