using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.AssetAnalyzer
{
	public class Program
	{
		public static void Main(string[] args)
		{
			try
			{
				var testFile = @"D:\Games\Steam\steamapps\common\CastleMiner Z\Content\Enemies\Zombies\Diffuse01_0.xnb";

				if (Debugger.IsAttached && File.Exists(testFile))
				{
					AnalyzeXnb(testFile);
				}
				else
				{
					if (!args.Any())
					{
						Console.WriteLine("Usage: {0} asset1.xnb [asset2.xnb ...]", new FileInfo(Assembly.GetExecutingAssembly().Location).Name);
						return;
					}

					foreach (var file in args)
					{
						if (!File.Exists(file))
						{
							Console.WriteLine("Error: File {0} does not exist.", file);
							continue;
						}

						AnalyzeXnb(file);
					}
				}
			}
			finally
			{
				Console.WriteLine("Press any key to exit . . .");
				Console.ReadKey(true);
			}
		}

		public static void AnalyzeXnb(string file)
		{
			try
			{
				using (var stream = File.OpenRead(file))
				using (var bin = new ExposedBinaryReader(stream))
				{
					if (bin.ReadByte() != (byte)'X' || bin.ReadByte() != (byte)'N' || bin.ReadByte() != (byte)'B')
					{
						Console.WriteLine("Error: {0} is not an XNB file.", file);
					}

					Console.WriteLine("====== {0}", file);

					string targetPlatform;
					switch (bin.ReadByte())
					{
					case (byte)'w':
						targetPlatform = "Microsoft Windows";
						break;

					case (byte)'m':
						targetPlatform = "Windows Phone 7";
						break;

					case (byte)'x':
						targetPlatform = "Xbox 360";
						break;

					default:
						targetPlatform = "Unknown";
						break;
					}
					Console.WriteLine("Target Platform: {0}", targetPlatform);

					var xnbVersion = bin.ReadByte();
					var xnbVersionString = "";
					switch (xnbVersion)
					{
					case (byte)5:
						xnbVersionString = " (XNA Game Studio 4.0)";
						break;
					}
					Console.WriteLine("XNB Format Version: {0}{1}", (int)xnbVersion, xnbVersionString);

					var flags = (XnbFlags)stream.ReadByte();
					var isCompressed = flags.HasFlag(XnbFlags.Compressed) && xnbVersion >= 5;
					Console.WriteLine("Flags: {0}", flags);

					var compressedSize = bin.ReadUInt32();
					Console.WriteLine("Compressed Size: {0} bytes", compressedSize);

					if (isCompressed)
					{
						var decompressedSize = bin.ReadUInt32();
						Console.WriteLine("Decompressed Size: {0} bytes", decompressedSize);


						var decompressStreamType = typeof(Microsoft.Xna.Framework.Point).Assembly.GetType("Microsoft.Xna.Framework.Content.DecompressStream");
						var decompressStreamCtor = decompressStreamType.GetConstructor(new[] { typeof(Stream), typeof(int), typeof(int) });

						using (var decompressStream = (Stream)decompressStreamCtor.Invoke(new object[] { stream, (int)compressedSize - 14, (int)decompressedSize }))
						using (var decompressBin = new ExposedBinaryReader(decompressStream))
						{
							AnalyzeXnbContent(decompressStream, decompressBin);
						}
					}
					else
					{
						AnalyzeXnbContent(stream, bin);
					}
				}
			}
#if !DEBUG
			catch (Exception ex)
			{
				Console.WriteLine("Error: File {0} is invalid: {1}", file, ex.Message);
			}
#endif
			finally
			{
				Console.WriteLine();
			}
		}

		private static void AnalyzeXnbContent(Stream stream, ExposedBinaryReader bin)
		{
			var typeReaderCount = bin.Read7BitEncodedIntEx();

			if (typeReaderCount > 1024)
            {
				Console.WriteLine("Too many type readers; aborting");
				return;
			}

			var typeReaders = new string[typeReaderCount];

			for (var i = 0; i < typeReaderCount; i++)
			{
				Console.WriteLine("  {0}:", i);

				var typeReaderName = bin.ReadString();
				Console.WriteLine("    Type Reader Name: {0}", typeReaderName);

				var readerVersionNumber = bin.ReadInt32();
				Console.WriteLine("    Reader Version Number: {0}", readerVersionNumber);

				typeReaders[i] = typeReaderName;
			}

			var sharedResourceCount = bin.Read7BitEncodedIntEx();
			Console.WriteLine("Shared Resource Count: {0}", sharedResourceCount);

			// There's always one primary object that's separate from the shared resources, so use <=
			for (var resourceIdx = 0; resourceIdx <= sharedResourceCount; resourceIdx++)
            {
				Console.WriteLine("--- Resource {0} ---", resourceIdx);
				var typeId = bin.Read7BitEncodedIntEx();
				Console.WriteLine("typeId: {0}", typeId);

				if (typeId > 0 && typeId > typeReaders.Length)
                {
					Console.WriteLine("typeId {0} doesn't have a corresponding type reader declared in the file; aborting", typeId);
					return;
                }

				var typeReader = typeId > 0 ? typeReaders[typeId - 1] : "null";

				Console.WriteLine("Type reader: {0}", typeReader);


				var typeReaderClass = typeReader.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0];

				switch (typeReaderClass)
                {
					case "Microsoft.Xna.Framework.Content.Texture2DReader":
						var surfaceFormat = (SurfaceFormat)bin.ReadInt32();
						var width = (int)bin.ReadUInt32();
						var height = (int)bin.ReadUInt32();
						var mipCount = (int)bin.ReadUInt32();

						Console.WriteLine("Surface format: {0}", surfaceFormat);
						Console.WriteLine("Width: {0}", width);
						Console.WriteLine("Height: {0}", height);
						Console.WriteLine("Mip count: {0}", mipCount);

						var decoder = new DxtDecoder(width, height, surfaceFormat);
						
						for (var mipIdx = 0; mipIdx < mipCount; mipIdx++)
                        {
							var dataSize = bin.ReadUInt32();
							Console.WriteLine("Data size: {0}", dataSize);

							var imageData = bin.ReadBytes(checked((int)dataSize));

							Console.WriteLine("Data preview: {0}", BitConverter.ToString(imageData, 0, Math.Min(16, imageData.Length)));

							var colorData = decoder.Decode(imageData);
                        }
						break;
				}
            }
		}
	}
}
