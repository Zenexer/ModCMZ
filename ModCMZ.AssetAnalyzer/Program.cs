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
				string testFile = Path.Combine("Test", "SmokeEffect.xnb");

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
			catch
			{
				Console.WriteLine("Error: File {0} is invalid.", file);
			}
			finally
			{
				Console.WriteLine();
			}
		}

		private static void AnalyzeXnbContent(Stream stream, ExposedBinaryReader bin)
		{
			var typeReaderCount = bin.Read7BitEncodedIntEx();
			Console.WriteLine("Type Reader Count: {0}", typeReaderCount);

			for (var i = 0; i < typeReaderCount; i++)
			{
				Console.WriteLine("  {0}:", i);

				var typeReaderName = bin.ReadString();
				Console.WriteLine("    Type Reader Name: {0}", typeReaderName);

				var readerVersionNumber = bin.ReadInt32();
				Console.WriteLine("    Reader Version Number: {0}", readerVersionNumber);
			}
		}
	}
}
