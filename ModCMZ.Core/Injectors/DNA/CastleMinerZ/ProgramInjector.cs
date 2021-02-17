using System;
using System.Reflection;
using Mono.Cecil.Cil;
using Mono.Cecil;


namespace ModCMZ.Core.Injectors.DNA.CastleMinerZ
{
	[Injector("CastleMinerZ", "DNA.CastleMinerZ.Program")]
	[Serializable]
	public class ProgramInjector : Injector
	{
		[MethodInjector("Main")]
		public void InjectMain()
		{
			ClearMethod();
			AppendModCall();
		}
	}
}
