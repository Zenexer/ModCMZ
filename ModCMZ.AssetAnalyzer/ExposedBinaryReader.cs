using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.AssetAnalyzer
{
	public class ExposedBinaryReader : BinaryReader
	{
		public ExposedBinaryReader(Stream input)
			: base(input)
		{
		}

		public int Read7BitEncodedIntEx()
		{
			return Read7BitEncodedInt();
		}
	}
}
