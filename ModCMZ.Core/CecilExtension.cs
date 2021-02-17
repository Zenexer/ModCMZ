using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;


namespace ModCMZ.Core
{
	public static class CecilExtension
	{
		public static MethodDefinition GetMethod(this TypeDefinition type, string name)
		{
			return type.Methods.FirstOrDefault(m => m.Name == name);
		}
	}
}
