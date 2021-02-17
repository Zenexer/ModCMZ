using Mono.Cecil;


namespace ModCMZ.Core.Injectors
{
	internal interface IInjector
	{
		void Inject(Injection injection, TypeDefinition type);
	}
}
