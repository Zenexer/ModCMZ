using DNA.CastleMinerZ;
using DNA.CastleMinerZ.AI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModCMZ.Core;
using ModCMZ.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Commands
{
    public class ExtractCommand : Command
    {
        public override string Name => "extract";

        public override string Description => "(WIP) Attempts to extract XNB files";

        public override string Usage => "";

        public override string Help => "";

        public override void Run(CommandArguments args)
		{

			var folder = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "XNB"));
			if (!folder.Exists)
			{
				folder.Create();
			}

			var content = Game.Game.Content;

			foreach (var textureName in EnemyType._textureNames)
			{
				Console.WriteLine($"Attempting to extract resource: {textureName}");
				try
				{
					var texture = content.Load<Texture2D>(textureName);
					var format = texture.Format;
					Console.WriteLine("Format: {0}", format);

					/*
					int num = *(int*)__unnamed000;
					_D3DFORMAT d3DFORMAT = (_D3DFORMAT)num;
					int num2 = (d3DFORMAT == (_D3DFORMAT)827611204 || d3DFORMAT == (_D3DFORMAT)844388420 || d3DFORMAT == (_D3DFORMAT)861165636 || d3DFORMAT == (_D3DFORMAT)877942852 || d3DFORMAT == (_D3DFORMAT)894720068) ? 1 : 0;
					if ((byte)num2 != 0)
					{
						dwLockWidth = dwLockWidth + 3 >> 2;
						dwLockHeight = dwLockHeight + 3 >> 2;
						dwFormatSize = ((num == 827611204) ? 8u : 16u);
					}

					uint num3 = dwLockWidth * dwLockHeight * dwFormatSize;
					if (dwElementSize * elementCount != num3)
					{
						throw new ArgumentException(FrameworkResources.InvalidTotalSize);
					}
					*/

					int elementSize;
					string ext;

					switch (format)
                    {
						case SurfaceFormat.Dxt3:
							elementSize = 4;
							ext = "dxt3";
							break;

						case SurfaceFormat.Dxt5:
							elementSize = 1;
							ext = "dxt5";
							break;

						default:
							elementSize = 4;
							ext = format.ToString().ToLowerInvariant();
							break;
					}

					var data = new Color[texture.Width * texture.Height];
					texture.GetData(data);

					var file = new FileInfo(Path.Combine(folder.FullName, textureName + ".bmp"));

					if (!file.Directory.Exists)
					{
						file.Directory.Create();
					}

					using (var tx = file.OpenWrite())
					{
						//tx.Write(data, 0, data.Length);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to extract resource {textureName}: {ex.Message}");
				}
			}
		}
    }
}
