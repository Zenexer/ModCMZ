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
			Append(Create(OpCodes.Ldarg_0), GetModCall());
		}

		[MethodInjector("Update")]
		public void InjectUpdate()
		{
			Prepend(Create(OpCodes.Ldarg_0), Create(OpCodes.Ldarg_1), GetModCall());
		}

		[MethodInjector("Draw")]
		public void InjectDraw()
		{
			Append(Create(OpCodes.Ldarg_0), Create(OpCodes.Ldarg_1), GetModCall());
		}
	}
}
