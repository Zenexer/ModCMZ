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
		[MethodInjector("get_UndeadDragonKills")]
		public void InjectGetUndeadDragonKills()
		{
			// Example: Force UndeadDragonKills to always return 5
			/*
			ClearMethod();
			Append(
				Create(OpCodes.Ldc_I4_5),
				Create(OpCodes.Ret)
			);
			*/
		}
	}
}
