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
			PrependModCallInterceptable();
		}
	}
}
