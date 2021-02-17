using ModCMZ.Core.Injectors;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Injectors.DNA.CastleMinerZ
{
	[Injector("CastleMinerZ", "DNA.CastleMinerZ.FrontEndScreen")]
	public sealed class FrontEndScreenInjector : Injector
	{
		[MethodInjector("SetupSaveDevice")]
		public void InjectSetupSaveDevice()
		{
			// Disables saving to Steam; can be convenient while debugging
			/*
			Replace(
				i =>
					i.OpCode == OpCodes.Ldsfld
					&& ((FieldReference)i.Operand).FullName == "DNA.CastleMinerZ.CastleMinerZGame DNA.CastleMinerZ.CastleMinerZGame::Instance"
					&& i.Next.OpCode == OpCodes.Callvirt
					&& ((MethodReference)i.Next.Operand).Name == "get_LicenseServices",
				OpCodes.Ldc_I8,
				127L);
			Replace(
				i =>
					i.OpCode == OpCodes.Callvirt
					&& ((MethodReference)i.Operand).Name == "get_LicenseServices",
				OpCodes.Nop);
			Replace(
				i =>
					i.OpCode == OpCodes.Callvirt
					&& ((MethodReference)i.Operand).Name == "get_SteamUserID",
				OpCodes.Nop);
			*/
		}
	}
}
