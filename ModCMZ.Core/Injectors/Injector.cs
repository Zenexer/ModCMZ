using System;
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

		public delegate void MethodInject();

		public event EventHandler Injecting;
		public event EventHandler Injected;

		public Injection Injection { get; protected set; }

		public bool UseShortForm
		{
			get
			{
				return USE_SHORT_FORM;
			}
		}

		[NonSerialized]
		private TypeDefinition m_Type;
		public TypeDefinition Type
		{
			get
			{
				return m_Type;
			}
		}

		[NonSerialized]
		private MethodDefinition m_Method;
		public MethodDefinition Method
		{
			get
			{
				return m_Method;
			}
		}

		[NonSerialized]
		private TypeDefinition m_ModType;
		public virtual TypeDefinition ModType
		{
			get
			{
				if (m_ModType == null)
				{
					var typeName = (string.IsNullOrEmpty(ModTypeNamespace) ? "" : ModTypeNamespace + ".") + ModTypeName;
					var types = App.Current.GetModTypes().Select(x => x.FullName).ToArray();
					var type = App.Current.GetModType(typeName);
					var assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.FullName).ToArray();
					Debug.Assert(type != null);
					m_ModType = Module.Import(type).Resolve();
				}

				return m_ModType;
			}
		}

		[NonSerialized]
		private string m_ModTypeName;
		public virtual string ModTypeName
		{
			get
			{
				if (m_ModTypeName == null)
				{
					m_ModTypeName = GetType().GetCustomAttribute<InjectorAttribute>().Type.Split('.').Last() + "Mod";
				}

				return m_ModTypeName;
			}
		}

		[NonSerialized]
		private string m_ModTypeNamespace;
		public virtual string ModTypeNamespace
		{
			get
			{
				if (m_ModTypeNamespace == null)
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

					m_ModTypeNamespace = ns;
				}

				return m_ModTypeNamespace;
			}
		}

		[NonSerialized]
		private cil::MethodBody m_Body;
		protected cil::MethodBody Body
		{
			get
			{
				return m_Body;
			}
		}

		[NonSerialized]
		private monogeneric::Collection<Instruction> m_Instructions;
		protected monogeneric::Collection<Instruction> Instructions
		{
			get
			{
				return m_Instructions;
			}
		}

		[NonSerialized]
		private ILProcessor m_IL;
		protected ILProcessor IL
		{
			get
			{
				return m_IL;
			}
		}

		protected ModuleDefinition Module
		{
			get
			{
				return Type.Module;
			}
		}

		protected virtual void OnInjecting()
		{
			if (Injecting != null)
			{
				Injecting.Invoke(this, EventArgs.Empty);
			}
		}

		protected virtual void OnInjected()
		{
			if (Injected != null)
			{
				Injected.Invoke(this, EventArgs.Empty);
			}
		}

		#region Implementation of IInjector

		public virtual void Inject(Injection injection, TypeDefinition type)
		{
			Injection = injection;
			m_Type = type;

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
			m_Method = null;
			m_Body = null;
			m_Instructions = null;
			m_IL = null;
		}

		private void Prepare(MethodDefinition method)
		{
			m_Method = method;
			m_Body = Method.Body;
			m_Instructions = Body.Instructions;
			m_IL = Body.GetILProcessor();
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

		protected void AppendModCall()
		{
			if (Method.IsStatic)
			{
				Append(GetModCall());
			}
			else
			{
				Append(Create(OpCodes.Ldarg_0), GetModCall());
			}
		}

		protected void PrependModCall()
		{
			if (Method.IsStatic)
			{
				Prepend(GetModCall());
			}
			else
			{
				Prepend(Create(OpCodes.Ldarg_0), GetModCall());
			}
		}

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

		protected Instruction GetModCall(string format = null, string prefix = null, string suffix = null)
		{
			return IL.Create(OpCodes.Call, GetModMethod(format, prefix, suffix));
		}

		protected bool Replace(Func<Instruction, bool> predicate, Instruction instruction)
		{
			var first = Instructions.FirstOrDefault(predicate);
			if (first == null)
			{
				Debugger.Break();
				throw new Exception("Must replace at least one instruction");
			}

			IL.Replace(first, instruction);
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
