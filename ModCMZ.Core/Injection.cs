using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using ModCMZ.Core.Injectors;
using Mono.Cecil;
using reflect = System.Reflection;
using System.Globalization;

namespace ModCMZ.Core
{
	public class Injection
	{
		public IEnumerable<Tuple<Type, ReplacesAttribute>> Replacers { get; private set; }
		public IEnumerable<Tuple<Type, InjectorAttribute>> Injectors { get; private set; }
		public ReaderParameters ReaderParams { get; private set; }
		public AssemblyDefinition Assembly { get; private set; }
		public ModuleDefinition Module { get; private set; }

		public Injection(string[] searchPath)
		{
			Injectors = Enumerable.Empty<Tuple<Type, InjectorAttribute>>();
			Replacers = Enumerable.Empty<Tuple<Type, ReplacesAttribute>>();

			var assemblyResolver = new DefaultAssemblyResolver();
			foreach (var path in searchPath.Where(Directory.Exists))
			{
				assemblyResolver.AddSearchDirectory(path);
			}

			ReaderParams = new ReaderParameters
			{
				AssemblyResolver = assemblyResolver,
				MetadataResolver = new MetadataResolver(assemblyResolver),
			};
		}

		public void AddInjectors(Assembly assembly = null)
		{
			if (assembly == null)
			{
				assembly = reflect::Assembly.GetExecutingAssembly();
			}

			// Enumerate immediately since we enumerate multiple times.
			var injectors =
				(from t in assembly.GetTypes()
				 where t.GetInterfaces().Contains(typeof(IInjector))
				 let a = t.GetCustomAttribute<InjectorAttribute>()
				 where a != null
				 select new Tuple<Type, InjectorAttribute>(t, a)).ToArray();

			Injectors = Injectors.Concat(injectors);
			App.Current.Debug("Added {0} injectors from {1}", injectors.Length, assembly.GetName().Name);
		}

		public void AddReplacers(Assembly assembly = null)
		{
			if (assembly == null)
			{
				assembly = reflect::Assembly.GetExecutingAssembly();
			}

			// Enumerate immediately since we enumerate multiple times.
			var replacers =
				(from t in assembly.GetTypes()
				 let a = t.GetCustomAttribute<ReplacesAttribute>()
				 where a != null
				 select new Tuple<Type, ReplacesAttribute>(t, a)).ToArray();

			Replacers = Replacers.Concat(replacers);
			App.Current.Debug("Added {0} injectors from {1}", replacers.Length, assembly.GetName().Name);
		}

		public FileInfo Inject(string assemblyFile)
		{
			return Run(new FileInfo(assemblyFile));
		}

		public TypeDefinition Import<T>()
		{
			return Import(typeof(T));
		}

		public TypeDefinition Import(Type type)
		{
			return Module.Import(type).Resolve();
		}

		public void LoadAssembly(FileInfo file)
		{
			LoadAssembly(file.FullName);
		}

		public void LoadAssembly(string file)
		{
			App.Current.Debug("Loading assembly from file: {0}", file);
			Assembly = AssemblyDefinition.ReadAssembly(file, ReaderParams);
			Module = Assembly.MainModule;
		}

		[Obsolete("This doesn't work.")]
		private void RunReplace()
		{
#if false
			App.Current.Debug("Replacing types in assembly: {0}", Assembly.Name.Name);
			var typeReplacer = new TypeReplacer(Module);

			foreach (var replacer in
				from i in Replacers
				where i.Item2.Assembly == Assembly.Name.Name
				select new
				{
					Type = i.Item1,
					Attribute = i.Item2,
				})
			{
				App.Current.Info("Replacing {0}", replacer.Attribute.Type);

				var oldRef = Module.GetType(replacer.Attribute.Type);
				if (oldRef == null)
				{
					App.Current.Warning("Missing type to replace: {0}", replacer.Attribute.Type);
					continue;
				}

				var newRef = Module.Import(replacer.Type);
				typeReplacer.Replace(Module.Types, oldRef, newRef);
				Module.Types.Remove(oldRef); // Remove AFTER, as Cecil will remove all references
			}
#endif
		}

		public void RunInject()
		{
			App.Current.Debug("Running injectors on assembly: {0}", Assembly.Name.Name);
			foreach (var injector in
				from i in Injectors
				where i.Item2.Assembly == Assembly.Name.Name
				select new
				{
					Type = i.Item1,
					Attribute = i.Item2,
				})
			{
				App.Current.Info("Running injector {0} on {1}", injector.Type.FullName, injector.Attribute.Type);

				IInjector instance;
				try
				{
					var constructor = injector.Type.GetConstructor(Type.EmptyTypes);
					if (constructor == null)
					{
						continue;
					}

					instance = (IInjector)constructor.Invoke(new object[0]);
				}
				catch (Exception ex)
				{
					throw new Exception($"Error instantiating injector: {injector.Attribute.Type}; responsible mod: {injector.GetType().Assembly.GetName().Name}", ex);
				}

				var type = Module.GetType(injector.Attribute.Type)
					?? throw new Exception($"Target type for injection doesn't exist: {injector.Attribute.Type}; responsible mod: {injector.GetType().Assembly.GetName().Name}");


				instance.Inject(this, type);
			}
		}


		public FileInfo Run(FileInfo assemblyFile)
		{
			LoadAssembly(assemblyFile);

			//RunReplace();
			RunInject();

			return Write(assemblyFile);
		}

		private void WriteResources(DirectoryInfo baseDir)
		{
			if (!Assembly.MainModule.HasResources)
			{
				return;
			}
			
			string name = Assembly.Name.Name + ".resources";
			Version version = Assembly.Name.Version;
			string culture = "en";

			DirectoryInfo resourceDir = new DirectoryInfo(Path.Combine(baseDir.FullName, culture));
			FileInfo outFile = new FileInfo(Path.Combine(resourceDir.FullName, name + ".dll"));

			AssemblyDefinition assembly = AssemblyDefinition.CreateAssembly(
				new AssemblyNameDefinition(name, version)
				{
					Culture = culture,
				},
				name + ".dll",
				ModuleKind.Dll
			);

			// Assembly attributes
			foreach (var attribute in Assembly.CustomAttributes)
			{
				assembly.CustomAttributes.Add(attribute);
			}

			//
			// Module
			//

			var module = assembly.MainModule;

			// References: assemblies
			module.AssemblyReferences.Add(Assembly.MainModule.AssemblyReferences.First(a => a.Name == "mscorlib"));

			// Resources
			foreach (EmbeddedResource orig in
				from r in assembly.MainModule.Resources
				let er = r as EmbeddedResource
				where er != null
				select er)
			{
				var resource = new EmbeddedResource(orig.Name, orig.Attributes, orig.GetResourceData())
				{
					IsPrivate = orig.IsPrivate,
					IsPublic = orig.IsPublic,
				};
				module.Resources.Add(resource);
			}

			//
			// Output
			//

			if (!resourceDir.Exists)
			{
				resourceDir.Create();
			}

			if (outFile.Exists)
			{
				outFile.Delete();
			}

			assembly.Write(outFile.FullName);

			outFile.Refresh();
			Debug.Assert(outFile.Exists);
		}

		private FileInfo Write(FileInfo assemblyFile)
		{
			var outFile = new FileInfo(Path.Combine(App.Current.MirrorDirectory.FullName, assemblyFile.Name));

			//WriteResources(outFile.Directory);

			if (outFile.Exists)
			{
				outFile.Delete();
			}

			Assembly.Write(outFile.FullName);

			outFile.Refresh();
			Debug.Assert(outFile.Exists);

			return outFile;
		}
	}
}
