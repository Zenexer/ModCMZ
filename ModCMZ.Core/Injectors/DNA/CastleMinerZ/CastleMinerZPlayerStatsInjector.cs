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
	[Injector("CastleMinerZ", "DNA.CastleMinerZ.CastleMinerZPlayerStats")]
	[Serializable]
	public sealed class CastleMinerZPlayerStatsInjector : Injector
	{
		// Would force UndeadDragonKills to always return 5
		/*
		[MethodInjector("get_UndeadDragonKills")]
		public void InjectGetUndeadDragonKills()
		{
			ClearMethod();
			Append(
				Create(OpCodes.Ldc_I4_5),
				Create(OpCodes.Ret)
			);
		}
		*/

		// ???
		[MethodInjector(".ctor")]
		public void InjectCtor()
		{
			Replace(
				i =>
					i.OpCode == OpCodes.Ldsfld
					&& ((FieldReference)i.Operand).FullName == "DNA.CastleMinerZ.CastleMinerZGame DNA.CastleMinerZ.CastleMinerZGame::Instance"
					&& i.Next.OpCode == OpCodes.Brfalse_S,
				OpCodes.Ldnull
			);
		}
	}
}
