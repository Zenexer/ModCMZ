using ModCMZ.Core.Injectors;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Mods.Basic.Injectors.DNA.CastleMinerZ.UI
{
	[Injector("CastleMinerZ", "DNA.CastleMinerZ.UI.InGameHUD")]
	[Serializable]
	public sealed class InGameHUDInjector : Injector
	{
		[MethodInjector("OnUpdate")]
		public void InjectOnUpdate()
		{
			Replace(
				i =>
					i.OpCode == OpCodes.Callvirt
					&& i.Operand is MethodReference method
					&& method.Name == "UpdateHostSession"
					&& method.DeclaringType.FullName == "DNA.Net.GamerServices.NetworkSession",
				GetModCall("UpdateHostSession_GameHasBegun")
			);
		}
	}
}
