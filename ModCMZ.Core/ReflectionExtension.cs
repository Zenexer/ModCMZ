using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModCMZ.Core
{
	public static class ReflectionExtension
	{
		private static bool IsInherited<T>() where T : Attribute
		{
			var usage = typeof(T).GetCustomAttributes(typeof(AttributeUsageAttribute), true).FirstOrDefault() as AttributeUsageAttribute;
			return usage == null || usage.Inherited;
		}

		public static T GetCustomAttribute<T>(this Type target) where T : Attribute
		{
			return target.GetCustomAttributes(typeof(T), IsInherited<T>()).FirstOrDefault() as T;
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this Type target) where T : Attribute
		{
			return target.GetCustomAttributes(typeof(T), IsInherited<T>()).Cast<T>();
		}

		public static T GetCustomAttribute<T>(this MethodInfo target) where T : Attribute
		{
			return target.GetCustomAttributes(typeof(T), IsInherited<T>()).FirstOrDefault() as T;
		}

		public static IEnumerable<T> GetCustomAttributes<T>(this MethodInfo target) where T : Attribute
		{
			return target.GetCustomAttributes(typeof(T), IsInherited<T>()).Cast<T>();
		}

		public static MethodInfo GetMethodEx(this Type type, string methodName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
		{
			return type.GetMethods(bindingFlags).FirstOrDefault(m => m.Name == methodName);
		}
	}
}
