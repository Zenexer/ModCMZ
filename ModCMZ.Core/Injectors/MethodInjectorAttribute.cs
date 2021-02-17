using System;


namespace ModCMZ.Core.Injectors
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class MethodInjectorAttribute : Attribute
	{
		public string Method;
		public int? ParamCount;

		public MethodInjectorAttribute(string method)
		{
			Method = method;
		}
	}
}
