using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;

namespace ModCMZ.Core.Injectors.DNA.Input
{
	[Injector("DNA.Common", "DNA.Input.InputManager")]
	[Serializable]
	public sealed class InputManagerInjector : Injector
	{
		[MethodInjector("Update")]
		public void InjectUpdate()
		{
			Prepend(
				Create(OpCodes.Ldarg_0),
				GetModCall(),
				Create(OpCodes.Brtrue_S, Instructions[0]),
				Create(OpCodes.Ret));
		}
	}
}
