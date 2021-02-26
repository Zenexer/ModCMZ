//#define SANDBOX

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Content;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace ModCMZ.Core
{
	internal static class DebugSandbox
	{
		[Conditional("DEBUG")]
		public static void Run()
		{
#if SANDBOX
			Application.EnableVisualStyles();

			if (!Debugger.IsAttached)
			{
				MessageBox.Show("Warning: This is a debug build, but there is no debugger attached.", "ModCMZ");
				return;
			}

			using (var sandbox = new Sandcastle())
			{
				sandbox.Build();
				sandbox.Play();
				sandbox.KnockDown();
			}

			Environment.Exit(1001);
#endif
		}

		private class Sandcastle : IDisposable
		{
			public void Build()
			{
			}

			public void Play()
			{
				var app = App.Create();
				//var domain = AppDomain.CurrentDomain;
				//var injection = new Injection(domain, app);

				var assemblyDefinition = AssemblyDefinition.ReadAssembly(Path.Combine(app.TargetDirectory.FullName, "CastleMinerZ.exe"));
				var moduleDefinition = assemblyDefinition.MainModule;
				var targetType = moduleDefinition.GetType("DNA.CastleMinerZ.GraphicsProfileSupport.ProfiledContentManager");

				{
					var method = targetType.Methods.First(m => m.Name == "Load");
					var body = method.Body;
					var instructions = body.Instructions;
					var il = body.GetILProcessor();

					body.ExceptionHandlers.Clear();
					instructions.Clear();
					body.Variables.Clear();

					var contentManager = moduleDefinition.Import(typeof(Microsoft.Xna.Framework.Content.ContentManager)).Resolve();
					var generic = moduleDefinition.Import(contentManager.Methods.First(m => m.Name == "Load")).Resolve();
					var genericInstance = new GenericInstanceMethod(generic);
					genericInstance.GenericArguments.Add(method.GenericParameters[0]);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldarg_1);
					il.Emit(OpCodes.Call, genericInstance);
					il.Emit(OpCodes.Ret);
				}

				using (var stream = new MemoryStream())
				{
					assemblyDefinition.Write(stream);
				}
			}

			public void KnockDown()
			{
				Debugger.Break();
			}

			public void Dispose()
			{
			}
		}
	}
}
