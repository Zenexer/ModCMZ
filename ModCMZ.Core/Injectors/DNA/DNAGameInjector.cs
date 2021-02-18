using System;
using ModCMZ.Core.Game;
using Mono.Cecil.Cil;
using xna = Microsoft.Xna.Framework;


namespace ModCMZ.Core.Injectors.DNA
{
	[Injector("DNA.Common", "DNA.DNAGame")]
	[Serializable]
	public sealed class DNAGameInjector : Injector
	{
		[MethodInjector("Initialize")]
		public void InjectInitialize()
		{
			AppendModCall();
		}

		[MethodInjector("Update")]
		public void InjectUpdate()
		{
			PrependModCall();
		}

		[MethodInjector("Draw")]
		public void InjectDraw()
		{
			AppendModCall();
		}
	}
}
