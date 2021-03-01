using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using cecil = Mono.Cecil;
using cil = Mono.Cecil.Cil;
using monogeneric = Mono.Collections.Generic;


namespace ModCMZ.Core.Injectors
{
	[Serializable]
	public abstract class Injector : IInjector
	{
		private const bool USE_SHORT_FORM = false;

		private static readonly OpCode[] LdargOpCodes = new[]
		{
			OpCodes.Ldarg_0,
			OpCodes.Ldarg_1,
			OpCodes.Ldarg_2,
			OpCodes.Ldarg_3,
		};

		public delegate void MethodInject();

		public event Action Injecting;
		public event Action Injected;

		public Injection Injection { get; protected set; }

		public bool UseShortForm => USE_SHORT_FORM;

		[NonSerialized]
		private TypeDefinition _type;
		public TypeDefinition Type => _type;

		[NonSerialized]
		private MethodDefinition _method;
		public MethodDefinition Method => _method;

		[NonSerialized]
		private TypeDefinition _modType;
		public virtual TypeDefinition ModType
		{
			get
			{
				if (_modType == null)
				{
					var typeName = (string.IsNullOrEmpty(ModTypeNamespace) ? "" : ModTypeNamespace + ".") + ModTypeName;
					var type = App.Current.GetModType(typeName);

					if (type == null)
					{
#if DEBUG
						// If the exception below is thrown, these may be useful to examine with a debugger.
						var types = App.Current.GetModTypes().Select(x => x.FullName).ToArray();
						var assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.FullName).ToArray();
#endif
						throw new Exception($"Expected {typeName} to exist, but it can't be found");
                    }

					_modType = Module.Import(type).Resolve();
				}

				return _modType;
			}
		}

		[NonSerialized]
		private string _modTypeName;
		public virtual string ModTypeName => _modTypeName ?? (_modTypeName = GetType().GetCustomAttribute<InjectorAttribute>().Type.Split('.').Last() + "Mod");

		[NonSerialized]
		private string _modTypeNamespace;
		public virtual string ModTypeNamespace
		{
			get
			{
				if (_modTypeNamespace == null)
				{
					var ns = GetType().Namespace;
					if (ns == null)
					{
						ns = "Runtime";
					}
					else if (ns.EndsWith(".Injectors"))
					{
						ns = ns.Remove(ns.LastIndexOf(".Injectors", StringComparison.Ordinal));
						ns += ".Runtime";
					}
					else if (ns.IndexOf(".Injectors.", StringComparison.Ordinal) >= 0)
					{
						ns = ns.Replace(".Injectors.", ".Runtime.");
					}
					else
					{
						ns = ns.Split('.')[0] + ".Runtime";
					}

					_modTypeNamespace = ns;
				}

				return _modTypeNamespace;
			}
		}

		[NonSerialized]
		private cil::MethodBody _body;
        protected cil::MethodBody Body => _body;

        [NonSerialized]
		private monogeneric::Collection<Instruction> _instructions;
        protected monogeneric::Collection<Instruction> Instructions => _instructions;

        [NonSerialized]
		private ILProcessor _IL;
        protected ILProcessor IL => _IL;

        protected ModuleDefinition Module => Type.Module;

        protected virtual void OnInjecting() => Injecting?.Invoke();

		protected virtual void OnInjected() => Injected?.Invoke();

		#region Implementation of IInjector

		public virtual void Inject(Injection injection, TypeDefinition type)
		{
			Injection = injection;
			_type = type;

			var injectMethods =
				(from m in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
				let d = Delegate.CreateDelegate(typeof(MethodInject), this, m, false) as MethodInject
				let a = m.GetCustomAttribute<MethodInjectorAttribute>()
				where d != null && a != null
				select new
				{
					Info = m,
					Inject = d,
					Attribute = a,
				}).ToArray();
			Debug.Assert(injectMethods.Any(), "Require at least one method injection");

			OnInjecting();

			foreach (var inject in injectMethods)
			{
				Debug.WriteLine("Running method injection {0}.{1} on {2}.{3}", GetType().FullName, inject.Info.Name, type.FullName, inject.Attribute.Method);
				var paramCount = inject.Attribute.ParamCount;
				var method = type.Methods.FirstOrDefault(m => m.Name == inject.Attribute.Method && (paramCount == null || m.Parameters.Count == paramCount));
				if (method == null)
				{
					App.Current.Error("Method {0} missing from type {1}.", inject.Attribute.Method, Type.FullName);
					continue;
				}

				Prepare(method);
				inject.Inject();
			}

			Clear();

			OnInjected();
		}

		#endregion

		private void Clear()
		{
			_method = null;
			_body = null;
			_instructions = null;
			_IL = null;
		}

		private void Prepare(MethodDefinition method)
		{
			_method = method;
			_body = Method.Body;
			_instructions = Body.Instructions;
			_IL = Body.GetILProcessor();
		}

		protected MethodCreator CreateMethod(string name, cecil::MethodAttributes attributes)
		{
			var creator = new MethodCreator(this);
			creator.CreateMethod(name, attributes);
			return creator;
		}

		protected void ClearMethod()
		{
			Body.ExceptionHandlers.Clear();
			Instructions.Clear();
			Body.Variables.Clear();
		}


		protected void Prepend(params Instruction[][] instructions) => Prepend(instructions.SelectMany(x => x));

		protected void Prepend(IEnumerable<Instruction> instructions) => Prepend(instructions.ToArray());

		protected void Prepend(params Instruction[] instructions)
		{
			for (var i = 0; i < instructions.Length; i++)
			{
				if (i == 0)
				{
					if (Instructions.Any())
					{
						IL.InsertBefore(Instructions[0], instructions[i]);
					}
					else
					{
						IL.Append(instructions[i]);
					}
				}
				else
				{
					IL.InsertAfter(instructions[i - 1], instructions[i]);
				}
			}
		}

		protected void Append(IEnumerable<Instruction> instructions) => Append(instructions.ToArray());

		protected void Append(params Instruction[][] instructions) => Append(instructions.SelectMany(x => x));

		protected void Append(params Instruction[] instructions)
		{
			if (!instructions.Any())
			{
				return;
			}

			var oldInstructionCount = Instructions.Count;
			var wasEmpty = oldInstructionCount <= 0;
			var start = 0;
			var wasRetLast = false;
			Instruction lastIns = null;
			Instruction oldRet = null;

			if (!wasEmpty)
			{
				oldRet = Instructions.Last();
				if (wasRetLast = oldRet.OpCode == OpCodes.Ret)
				{
					start = 1;
					oldInstructionCount--;
				}
				else
				{
					oldRet = null;
				}
			}

			for (var i = start; i < instructions.Length; i++)
			{
				IL.Append(lastIns = instructions[i]);
			}

			var last = lastIns.OpCode;
			if (!(last == OpCodes.Ret || last == OpCodes.Rethrow || last == OpCodes.Throw || last == OpCodes.Jmp || last == OpCodes.Tail))
			{
				IL.Append(lastIns = IL.Create(OpCodes.Ret));
			}

			if (!wasEmpty)
			{
				if (wasRetLast)
				{
					IL.Replace(oldRet, lastIns = instructions[0]);
				}

				for (var i = 0; i < oldInstructionCount; i++)
				{
					if (Instructions[i].OpCode == OpCodes.Ret)
					{
						IL.Replace(Instructions[i], IL.Create(UseShortForm && instructions[0].Offset - i <= 0x7f ? OpCodes.Br_S : OpCodes.Br, instructions[0]));
					}

					if (wasRetLast && Instructions[i].Operand == oldRet)
					{
						Instructions[i].Operand = instructions[0];
					}
				}
			}
		}

		protected void InsertModCallAfter(Instruction target, string format = null, string prefix = null, string suffix = null) => InsertAfter(target, GetModCall(format, prefix, suffix));

		protected void InsertAfter(Instruction target, params Instruction[] instructions) => InsertAfter(target, (IEnumerable<Instruction>)instructions);

		protected void InsertAfter(Instruction target, params Instruction[][] instructions) => InsertAfter(target, instructions.SelectMany(x => x));

		protected void InsertAfter(Instruction target, IEnumerable<Instruction> instructions)
        {
			foreach (var instruction in instructions)
            {
				IL.InsertAfter(target, instruction);
				target = instruction;
            }
        }

		protected void AppendModCall() => Append(GetModCall());

		protected void PrependModCall() => Prepend(GetModCall());

		protected void PrependModCallInterceptable() => Prepend(
			GetModCall(),
			new[] {
				Create(OpCodes.Brtrue_S, Instructions[0]),
				Create(OpCodes.Ret),
			}
		);

		/// <summary>
		/// Gets a standard mod method.
		/// </summary>
		/// <param name="format">{0}: targetName; {1}: prefix; {2}: suffix</param>
		/// <param name="prefix">Prepended unless there is a format</param>
		/// <param name="suffix">Appended unless there is a format</param>
		/// <returns>An unresolved reference to the mod method.</returns>
		protected MethodReference GetModMethod(string format = null, string prefix = null, string suffix = null)
		{
			string targetName;
			switch (Method.Name)
			{
			case ".ctor":
				targetName = "Ctor";
				break;

			case ".cctor":
				targetName = "CCtor";
				break;

			default:
				targetName = Method.Name;
				break;
			}

			if (!string.IsNullOrEmpty(format))
			{
				targetName = string.Format(format, targetName, prefix, suffix);
			}
			else
			{
				if (!string.IsNullOrEmpty(prefix))
				{
					targetName = prefix + targetName;
				}

				if (!string.IsNullOrEmpty(suffix))
				{
					targetName += suffix;
				}
			}

			var modMethod = (MethodReference)ModType.Methods.First(m => m.Name == targetName);
			return Module.Import(modMethod);
		}

		protected Instruction[] GetModCall(string format = null, string prefix = null, string suffix = null)
		{
			var method = GetModMethod(format, prefix, suffix);
			var paramCount = method.Parameters.Count;
			var maxParamCount = Method.Parameters.Count + (Method.HasThis ? 1 : 0);

			if (paramCount > maxParamCount)
            {
				throw new Exception($"Expected no more than {maxParamCount} parameter(s) for {Method.FullName}, but {method.FullName} requested {paramCount} parameter(s)");
            }

			var instrs = new Instruction[paramCount + 1];

			for (var i = 0; i < paramCount && i < LdargOpCodes.Length; i++)
            {
				instrs[i] = IL.Create(LdargOpCodes[i]);
            }

			for (var i = LdargOpCodes.Length; i < paramCount; i++)
            {
				instrs[i] = IL.Create(OpCodes.Ldarg_S, Method.Parameters[i]);
            }

			instrs[paramCount] = IL.Create(OpCodes.Call, method);

			return instrs;
		}

		protected bool Replace(Func<Instruction, bool> predicate, Instruction instruction) => Replace(predicate, new[] { instruction });

		protected bool Replace(Func<Instruction, bool> predicate, IEnumerable<Instruction> instructions) => Replace(predicate, instructions.ToArray());

		protected bool Replace(Func<Instruction, bool> predicate, Instruction[] instructions)
		{
			var first = Instructions.FirstOrDefault(predicate);
			if (first == null)
			{
				Debugger.Break();
				throw new Exception("Must replace at least one instruction");
			}

			IL.Replace(first, instructions[0]);

			for (var i = 1; i < instructions.Length; i++)
            {
				IL.InsertAfter(instructions[i - 1], instructions[i]);
            }

			return true;
		}

		protected bool Replace(Func<Instruction, bool> predicate, OpCode opCode)
		{
			return Replace(predicate, Create(opCode));
		}

		protected bool Replace<T>(Func<Instruction, bool> predicate, OpCode opCode, T operand)
		{
			return Replace(predicate, Create(opCode, operand));
		}

		protected void ReplaceAll(Func<Instruction, bool> predicate, OpCode opCode)
		{
			foreach (var instruction in Instructions.Where(predicate).ToArray())
			{
				IL.Replace(instruction, Create(opCode));
			}
		}

		protected void ReplaceAll<T>(Func<Instruction, bool> predicate, OpCode opCode, T operand)
		{
			foreach (var instruction in Instructions.Where(predicate).ToArray())
			{
				IL.Replace(instruction, Create(opCode, operand));
			}
		}

		protected Instruction Create(OpCode opCode)
		{
			return IL.Create(opCode);
		}

		protected Instruction Create<T>(OpCode opCode, T operand)
		{
			var method =
				(
					from m in typeof(ILProcessor).GetMethods(BindingFlags.Public | BindingFlags.Instance)
					where m.Name == "Create"
					let p = m.GetParameters()
					where p.Length == 2 && p[1].ParameterType == typeof(T)
					select m
				).FirstOrDefault();

			if (method == null)
			{
				throw new InvalidOperationException(string.Format("Cannot create instruction with specified operand type: {0}", typeof(T).FullName));
			}

			return (Instruction)method.Invoke(IL, new object[] { opCode, operand });
		}

		protected MethodReference Import(MethodInfo methodInfo)
		{
			var param = methodInfo.GetParameters();
			var declaringType = Import(methodInfo.DeclaringType);
			var methods = declaringType.Methods.Where(m => m.Name == methodInfo.Name && m.Parameters.Count == param.Length);
			var methodsArray = methods.Where(m =>
				!param.Where((t, i) =>
					t.ParameterType.FullName != m.Parameters[i].ParameterType.FullName
					&& !(t.ParameterType.IsGenericParameter && m.Parameters[i].ParameterType.FullName == null)).Any()).ToArray();
			Debug.Assert(methodsArray.Length == 1, "Must be exactly one method.", "Methods found: {0}", methodsArray.Length);
			var method = methodsArray[0];

			var methodRef = Module.Import(method);
			Debug.Assert(methodRef != null, "Method must resolve");
			return methodRef;
		}

		protected TypeReference Import(TypeDefinition type)
		{
			return Module.Import(type);
		}

		protected TypeDefinition Import(Type type)
		{
			return Injection.Import(type);
		}

		protected TypeDefinition Import<T>()
		{
			return Injection.Import<T>();
		}

		protected void MakeAllVisible()
		{
			foreach (var method in Type.Methods)
			{
				method.IsPrivate = false;
				method.IsPublic = true;
			}

			foreach (var field in Type.Fields)
			{
				field.IsPrivate = false;
				field.IsPublic = true;
			}
		}

		public class MethodCreator : IDisposable
		{
			private readonly Injector m_Injector;

			protected internal MethodCreator(Injector injector)
			{
				m_Injector = injector;
			}

			protected internal virtual void CreateMethod(string name, cecil::MethodAttributes attributes)
			{
				var method = new MethodDefinition(name, attributes, m_Injector.Type);
				m_Injector.Type.Methods.Add(method);
				m_Injector.Prepare(method);
			}

			public void Dispose()
			{
				m_Injector.Clear();
			}
		}
	}
}
