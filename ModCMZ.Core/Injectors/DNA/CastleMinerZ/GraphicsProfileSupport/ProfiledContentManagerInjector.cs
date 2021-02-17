using System;
using Microsoft.Xna.Framework.Content;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace ModCMZ.Core.Injectors.DNA.CastleMinerZ.GraphicsProfileSupport
{
#if false
	[Injector("CastleMinerZ", "DNA.CastleMinerZ.GraphicsProfileSupport.ProfiledContentManager")]
	[Serializable]
	public class ProfiledContentManagerInjector : Injector
	{
		public ProfiledContentManagerInjector()
		{
			Injected += ProfiledContentManagerInjector_Injected;
		}

		private void ProfiledContentManagerInjector_Injected(object sender, EventArgs e)
		{
			using (var creator = CreateMethod("BaseLoad", MethodAttributes.Public))
			{
				Method.Parameters.Add(new ParameterDefinition(Import<string>()));
				Method.GenericParameters.Add(new GenericParameter(0, GenericParameterType.Method, Module));

				var method = Import(typeof(ContentManager).GetMethod("Load"));
				var generic = new GenericInstanceMethod(method);
				generic.GenericArguments.Add(Method.GenericParameters[0]);
				Prepend(Create(OpCodes.Ldarg_0), Create(OpCodes.Ldarg_1), IL.Create(OpCodes.Call, generic), Create(OpCodes.Ret));
			}
		}

		[MethodInjector(".ctor")]
		public void InjectCtor()
		{
			Append(Create(OpCodes.Ldarg_0), GetModCall());
		}

		[MethodInjector("Load")]
		public void InjectLoad()
		{
			ClearMethod();

			var method = new GenericInstanceMethod(GetModMethod());
			method.GenericArguments.Add(Method.GenericParameters[0]);
			// Must use IL.Create for third opcode.
			Prepend(Create(OpCodes.Ldarg_0), Create(OpCodes.Ldarg_1), IL.Create(OpCodes.Call, method), Create(OpCodes.Ret));
		}
	}
#endif
}
