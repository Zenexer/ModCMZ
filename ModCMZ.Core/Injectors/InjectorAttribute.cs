using System;


namespace ModCMZ.Core.Injectors
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class InjectorAttribute : TargetedAttribute
	{
		public InjectorAttribute(string assembly, string type)
			: base(assembly, type)
		{
		}
	}
}
