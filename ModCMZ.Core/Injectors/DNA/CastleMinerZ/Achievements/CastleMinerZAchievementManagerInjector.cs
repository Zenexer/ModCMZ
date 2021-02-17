using ModCMZ.Core.Injectors;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Core.Injectors.DNA.CastleMinerZ.Achievements
{
	[Injector("CastleMinerZ", "DNA.CastleMinerZ.Achievements.CastleMinerZAchievementManager")]
	public sealed class CastleMinerZAchievementManagerInjector : Injector
	{
		// Prevents achievements from being reported to Steam during debugging
#if DEBUG
		[MethodInjector("OnAchieved")]
		public void InjectOnAchieved()
		{
			for (int i = 0; i < Instructions.Count; i++)
			{
				if (Instructions[i].OpCode == OpCodes.Callvirt && ((MethodReference)Instructions[i].Operand).Name == "get_SteamAPI")
				{
					for (int i2 = i - 2; i2 >= 0; i2--)
					{
						if (Instructions[i2].OpCode == OpCodes.Ldloc_0 && Instructions[i2 + 1].OpCode == OpCodes.Brfalse_S)
						{
							IL.Replace(Instructions[i2], Create(OpCodes.Ldnull));
							return;
						}
					}
				}
			}

			throw new Exception("Couldn't find instruction pattern");
		}
#endif
	}
}
