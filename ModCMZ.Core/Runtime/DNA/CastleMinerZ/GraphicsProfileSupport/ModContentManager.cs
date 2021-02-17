using DNA.CastleMinerZ;
using DNA.CastleMinerZ.GraphicsProfileSupport;
using DNA.Drawing;
using DNA.Drawing.Imaging;
using DNA.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModCMZ.Core.Runtime.DNA.CastleMinerZ.GraphicsProfileSupport
{
	public class ModContentManager : ContentManager
	{
		public static string[] CImageExtensions { get; set; }

		public string HiDefRoot { get; set; }
		public string ReachRoot { get; set; }
		public int TextureLevel { get; set; }
		public bool CheckUserTextures { get; set; }
		public string UserTexturesDefault { get; set; }
		public Dictionary<string, string> AvailableTextures { get; set; }
		public Dictionary<string, Texture2D> PreLoadedTextures { get; set; }

		private Stopwatch m_LoadingTimer;
		public Stopwatch LoadingTimer
		{
			get { return m_LoadingTimer; }
			set { m_LoadingTimer = value; }
		}

		static ModContentManager()
		{
			CImageExtensions = new[] { "*.dds", "*.tiff", "*.png", "*.jpg", "*.jpeg", "*.bmp" };
		}

		public ModContentManager(IServiceProvider services, string reachRoot, string hiDefRoot, int textureLevel)
			//: base(services, reachRoot, hiDefRoot, textureLevel)
			: base(services)
		{
			HiDefRoot = hiDefRoot;
			ReachRoot = reachRoot;
			TextureLevel = textureLevel;
			LoadingTimer = Stopwatch.StartNew();

			Type argsType = typeof(CastleMinerZGame).Assembly.GetType("DNA.CastleMinerZ.CastleMinerZArgs");
			Debug.Assert(argsType != null, "argsType != null");
			FieldInfo instanceField = argsType.GetField("Instance");
			Debug.Assert(instanceField != null, "instanceField != null");
			object argsInstance = instanceField.GetValue(null);
			Debug.Assert(argsInstance != null, "argsInstance != null");
			FieldInfo textureFolderField = argsType.GetField("TextureFolder");
			Debug.Assert(textureFolderField != null, "textureFolderField != null");
			string textureFolder = (string)textureFolderField.GetValue(argsInstance);

			if (textureFolder != null)
			{
				CheckUserTextures = false;
				int startIndex = textureFolder.Length + 1;

				foreach (string str in CImageExtensions)
				{
					string[] strArray = PathTools.RecursivelyGetFiles(textureFolder, str);
					if (strArray != null && strArray.Length > 0)
					{
						if (!CheckUserTextures)
						{
							UserTexturesDefault = textureFolder;
							CheckUserTextures = true;
							AvailableTextures = new Dictionary<string, string>();
							PreLoadedTextures = new Dictionary<string, Texture2D>();
						}
						foreach (string str2 in strArray)
						{
							string key = Path.ChangeExtension(str2, null).Substring(startIndex).ToLower();
							if (key.EndsWith("_m"))
							{
								key = key.Substring(0, key.Length - 2);
							}
							if (!AvailableTextures.ContainsKey(key))
							{
								AvailableTextures.Add(key, str2);
							}
						}
					}
				}
			}
			else
			{
				CheckUserTextures = false;
			}
		}

		public Texture2D TryLoadFromFile(string assetName)
		{
			Texture2D textured;

			if (!PreLoadedTextures.TryGetValue(assetName, out textured))
			{
				string str2;
				string key = assetName.ToLower();
				bool flag = false;
				flag = AvailableTextures.TryGetValue(key, out str2);

				if (!flag)
				{
					if (GraphicsProfileManager.Instance.IsHiDef)
					{
						flag = AvailableTextures.TryGetValue(Path.Combine(HiDefRoot.ToLowerInvariant(), key), out str2);
					}
					else
					{
						flag = AvailableTextures.TryGetValue(Path.Combine(ReachRoot.ToLowerInvariant(), key), out str2);
					}
				}

				if (!flag)
				{
					return null;
				}

				bool flag2 = false;
				key = str2.ToLower();
				bool makeMipmaps = Path.ChangeExtension(key, null).EndsWith("_m");
				bool normalizeMipmaps = false;

				if (makeMipmaps)
				{
					normalizeMipmaps = key.Contains("_nrm_") || key.Contains("_n_");
				}

				do
				{
					if (GraphicsDeviceLocker.Instance.TryLockDeviceTimed(ref m_LoadingTimer))
					{
						flag2 = true;
						try
						{
							textured = TextureLoader.LoadFromFile(CastleMinerZGame.Instance.GraphicsDevice, str2, makeMipmaps, normalizeMipmaps);
						}
						finally
						{
							GraphicsDeviceLocker.Instance.UnlockDevice();
						}
					}
					if (!flag2)
					{
						Thread.Sleep(10);
					}
				}
				while (!flag2);

				PreLoadedTextures.Add(assetName, textured);
			}

			return textured;
		}

		public override T Load<T>(string assetName)
		{
			Type type = typeof(T);
			bool flag = type.IsSubclassOf(typeof(Texture)) || (type == typeof(Texture));
			T local = default(T);
			LoadingTimer.Restart();
			if (this.CheckUserTextures && flag)
			{
				object obj = TryLoadFromFile(assetName);
				//local = Cast<T>(obj);
				local = obj == null ? default(T) : (T)obj;
				if (local != null)
				{
					return local;
				}
			}
			List<string> list = new List<string>();
			if ((this.TextureLevel > 1) && flag)
			{
				string str = "_L" + this.TextureLevel.ToString();
				list.Add(assetName + str);
				if (GraphicsProfileManager.Instance.IsHiDef)
				{
					list.Add(this.HiDefRoot + @"\" + assetName + str);
				}
				else
				{
					list.Add(this.ReachRoot + @"\" + assetName + str);
				}
			}
			list.Add(assetName);
			if (GraphicsProfileManager.Instance.IsHiDef)
			{
				list.Add(this.HiDefRoot + @"\" + assetName);
			}
			else
			{
				list.Add(this.ReachRoot + @"\" + assetName);
			}

#if DEBUG
			List<Exception> exceptions = new List<Exception>(list.Count);
#endif

			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					return base.Load<T>(list[i]);
				}
				catch (Exception ex)
				{
#if DEBUG
					exceptions.Add(ex);
#endif

					if (Debugger.IsAttached && File.Exists(Path.Combine(App.Current.TargetDirectory.FullName, RootDirectory, list[i] + ".xnb")))
					{
						Type a = Type.GetType("DNA.Drawing.Particles.ParticleEffect, DNA.Common");
						object b = Activator.CreateInstance(a);
						bool c = b is T;

						using (Stream stream = OpenStream(list[i]))
						{
							using (ContentReader reader = (ContentReader)typeof(ContentReader).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { this, stream, list[i], new Action<IDisposable>(d => { }) }))
							{
								ContentTypeReader typeReader;
								var method = typeof(ContentTypeReaderManager).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).First(m => m.Name == "GetTypeReader" && m.GetParameters().Length >= 3);
								typeReader = (ContentTypeReader)method.Invoke(null, new object[] { "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[DNA.Drawing.Particles.ParticleEffect, DNA.Common]]", reader, new List<ContentTypeReader>(0) });
								//InstantiateTypeReader("Microsoft.Xna.Framework.Content.ReflectiveReader`1[[DNA.Drawing.Particles.ParticleEffect, DNA.Common]]", reader, out typeReader);
								b = Activator.CreateInstance(typeReader.TargetType);
								c = b is T;
								var d = b.GetType().Assembly.Location;
								d = typeReader.TargetType.Assembly.Location;
							}
						}


						throw;
					}
				}
			}

			throw new Exception("Asset not found " + assetName);
		}

		delegate bool InstantiateTypeReaderD(string readerTypeName, ContentReader contentReader, out ContentTypeReader reader);
		private static bool InstantiateTypeReader(string readerTypeName, ContentReader contentReader, out ContentTypeReader reader)
		{
			var method = (InstantiateTypeReaderD)Delegate.CreateDelegate(typeof(InstantiateTypeReaderD), typeof(ContentTypeReaderManager).GetMethod("InstantiateTypeReader", BindingFlags.Static | BindingFlags.NonPublic));
			return method(readerTypeName, contentReader, out reader);
		}
	}
}
