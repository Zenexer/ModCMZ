using System;


namespace ModCMZ.Core.Injectors
{
	[AttributeUsage(
		AttributeTargets.Class |
		AttributeTargets.Struct |
		AttributeTargets.Interface |
		AttributeTargets.Enum |
		AttributeTargets.Delegate,
		AllowMultiple = false,
		Inherited = false)]
	public class ReplacesAttribute : TargetedAttribute
	{
		public ReplacesAttribute(string assembly, string type)
			: base(assembly, type)
		{
		}
	}
}
