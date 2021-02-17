using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.AssetAnalyzer
{
	[Flags]
	public enum XnbFlags : byte
	{
		None = 0,
		HiDef = 0x01,
		Compressed = 0x80,
	}
}
