using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModCMZ.Bootstrap
{
	public class Deref : MarshalByRefObject
	{
		public IDictionary<string, Assembly> Assemblies { get; private set; }
		public string[] ProgramArgs { get; set; }

		public Deref()
		{
			Assemblies = new Dictionary<string, Assembly>();
		}

		public void LoadAssembly(string file)
		{
			Assembly assembly = Assembly.LoadFrom(file);
			Assemblies.Add(assembly.GetName().Name, assembly);
			Debug.WriteLine("Loadded assembly {0} from {1}", assembly.GetName().Name, file);
		}

		[Conditional("DEBUG")]
		public void ListAssemblies()
		{
			Console.WriteLine("Assemblies in domain:");

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Console.WriteLine("   - {0} from {1}", assembly.GetName().Name, assembly.Location);
			}
		}

		public void Run()
		{
			Debug.Assert(Assemblies.ContainsKey("CastleMinerZ"));
			Debug.Assert(Assemblies.ContainsKey("ModCMZ.Core"));
			
#if CLICKONCE
			// Using reflection to invoke the native method skips an unreliable step, but we'll leave this here in case it breaks.
			//AppDomain.CurrentDomain.ExecuteAssembly(Assemblies["CastleMinerZ"].Location);
			try
			{
				var nExecuteAssembly = typeof(AppDomain).GetMethod("nExecuteAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
				nExecuteAssembly.Invoke(AppDomain.CurrentDomain, new object[] { Assemblies["CastleMinerZ"], ProgramArgs });
			}
			catch
			{
				var onlineServices = Assemblies["DNA.Common"].GetType("DNA.Distribution.OnlineServices");
				var instanceProperty = onlineServices.GetProperty("Instance");
				var instance = instanceProperty.GetValue(null);
			}
#elif STEAM
			try
			{
				// Using reflection to invoke the native method skips an unreliable step, but we'll leave this here in case it breaks.
				//AppDomain.CurrentDomain.ExecuteAssembly(Assemblies["CastleMinerZ"].Location);

				var nExecuteAssembly = typeof(AppDomain).GetMethod("nExecuteAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
				nExecuteAssembly.Invoke(AppDomain.CurrentDomain, new object[] { Assemblies["CastleMinerZ"], ProgramArgs });
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null)
				{
					throw ex.InnerException;
				}
				else
				{
					throw;
				}
			}
#endif
		}
	}
}
