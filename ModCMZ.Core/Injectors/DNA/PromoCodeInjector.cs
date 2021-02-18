using System;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil.Cil;

namespace ModCMZ.Core.Injectors.DNA
{
	// He's dead, Jim!
	/*
	[Injector("DNA.Common", "DNA.PromoCode")]
	[Serializable]
	public class PromoCodeInjector : Injector
	{
		[MethodInjector(".ctor")]
		//[Conditional("DEBUG")]
		public void InjectCtor()
		{
			Append(Create(OpCodes.Ldarg_0), Create(OpCodes.Ldc_I4_1), IL.Create(OpCodes.Stfld, Type.Fields.First(f => f.Name == "_redeemed")));
		}
	}
	*/
}
