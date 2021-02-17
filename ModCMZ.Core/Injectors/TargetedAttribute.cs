using System;


namespace ModCMZ.Core.Injectors
{
	public abstract class TargetedAttribute : Attribute
	{
		public string Assembly;
		public string Type;

		public TargetedAttribute(string assembly, string type)
		{
			Assembly = assembly;
			Type = type;
		}
	}
}
