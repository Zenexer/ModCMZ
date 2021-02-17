using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ModCMZ.Core.Injectors
{
	[Obsolete("This doesn't work.")]
	public class TypeReplacer
	{
		public ModuleDefinition Module { get; private set; }

		public TypeReplacer(ModuleDefinition module)
		{
			Module = module;
		}

		public void Replace(ICollection<TypeDefinition> collection, TypeReference oldRef, TypeReference newRef)
		{
			foreach (var obj in collection)
			{
				if (obj.HasFields)
				{
					Replace(obj.Fields, oldRef, newRef);
				}

				if (obj.HasProperties)
				{
					Replace(obj.Properties, oldRef, newRef);
				}

				if (obj.HasGenericParameters)
				{
					Replace(obj.GenericParameters, oldRef, newRef);
				}

				if (obj.HasNestedTypes)
				{
					Replace(obj.NestedTypes, oldRef, newRef);
				}

				if (obj.HasInterfaces)
				{
					Replace(obj.Interfaces, oldRef, newRef);
				}

				if (obj.HasEvents)
				{
					Replace(obj.Events, oldRef, newRef);
				}

				if (obj.HasMethods)
				{
					Replace(obj.Methods, oldRef, newRef);
				}
			}
		}

		public void Replace(ICollection<GenericParameter> collection, TypeReference oldRef, TypeReference newRef)
		{
			foreach (var obj in collection)
			{
				if (obj.HasGenericParameters)
				{
					Replace(obj.GenericParameters, oldRef, newRef);
				}

				if (obj.HasConstraints)
				{
					Replace(obj.Constraints, oldRef, newRef);
				}

				if (obj.HasCustomAttributes)
				{
					Replace(obj.CustomAttributes, oldRef, newRef);
				}
			}
		}

		public void Replace(ICollection<CustomAttribute> collection, TypeReference oldRef, TypeReference newRef)
		{
			foreach (var obj in collection.Where(o => o.AttributeType == oldRef))
			{
				// AttributeType property is read-only.
				// Will need to find a way around this if we want to replace attributes.  Leave code here just in case.
				throw new NotSupportedException("Attributes cannot be replaced.");
			}
		}

		public void Replace(ICollection<TypeReference> collection, TypeReference oldRef, TypeReference newRef)
		{
			if (collection.Contains(oldRef))
			{
				collection.Remove(oldRef);
				collection.Add(newRef);
			}
		}

		public void Replace(ICollection<PropertyDefinition> collection, TypeReference oldRef, TypeReference newRef)
		{
			foreach (var obj in collection)
			{
				if (obj.HasCustomAttributes)
				{
					Replace(obj.CustomAttributes, oldRef, newRef);
				}

				if (obj.PropertyType == oldRef)
				{
					obj.PropertyType = newRef;
				}
			}
		}

		public void Replace(ICollection<FieldDefinition> collection, TypeReference oldRef, TypeReference newRef)
		{
			foreach (var obj in collection)
			{
				if (obj.HasCustomAttributes)
				{
					Replace(obj.CustomAttributes, oldRef, newRef);
				}

				if (obj.FieldType == oldRef)
				{
					obj.FieldType = newRef;
				}
			}
		}

		public void Replace(ICollection<EventDefinition> collection, TypeReference oldRef, TypeReference newRef)
		{
			foreach (var obj in collection)
			{
				if (obj.HasCustomAttributes)
				{
					Replace(obj.CustomAttributes, oldRef, newRef);
				}

				if (obj.EventType == oldRef)
				{
					obj.EventType = newRef;
				}
			}
		}

		public void Replace(ICollection<MethodDefinition> collection, TypeReference oldRef, TypeReference newRef)
		{
			foreach (var obj in collection)
			{
				if (obj.HasBody)
				{
					Replace(obj.Body, oldRef, newRef);
				}

				if (obj.HasCustomAttributes)
				{
					Replace(obj.CustomAttributes, oldRef, newRef);
				}

				if (obj.HasGenericParameters)
				{
					Replace(obj.GenericParameters, oldRef, newRef);
				}

				if (obj.HasParameters)
				{
					Replace(obj.Parameters, oldRef, newRef);
				}
			}
		}

		private void Replace(ICollection<ParameterDefinition> collection, TypeReference oldRef, TypeReference newRef)
		{
			foreach (var obj in collection)
			{
				if (obj.HasCustomAttributes)
				{
					Replace(obj.CustomAttributes, oldRef, newRef);
				}

				if (obj.ParameterType == oldRef)
				{
					obj.ParameterType = newRef;
				}
			}
		}

		private void Replace(MethodBody body, TypeReference oldRef, TypeReference newRef)
		{
			if (body.HasExceptionHandlers)
			{
				Replace(body.ExceptionHandlers, oldRef, newRef);
			}

			if (body.HasVariables)
			{
				Replace(body.Variables, oldRef, newRef);
			}

			var il = body.GetILProcessor();

			foreach (var i in body.Instructions)
			{
				if (i.Operand == oldRef)
				{
					il.Replace(i, il.Create(i.OpCode, newRef));
					continue;
				}

				var member = i.Operand as MemberReference;
				if (member != null && (member = Replace(member, oldRef, newRef)) != null)
				{
					i.Operand = member;
				}
			}
		}

		private MemberReference Replace(MemberReference member, TypeReference oldRef, TypeReference newRef)
		{
			if (member.DeclaringType == null)
			{
				return null;
			}

			if (member.DeclaringType == oldRef)
			{
				member.DeclaringType = newRef;
				return Import(member);
			}

			return null;
		}

		// Note: This breaks when generic parameters are used.
		private MemberReference Import(MemberReference member)
		{
			var field = member as FieldReference;
			if (field != null)
			{
				return Module.Import(field);
			}

			var method = member as MethodReference;
			if (method != null)
			{
				return Module.Import(method);
			}

			var type = member as TypeReference;
			if (type != null)
			{
				return Module.Import(type);
			}

			throw new NotSupportedException("Tried to improt an unsupported member reference type.");
		}

		private void Replace(ICollection<VariableDefinition> collection, TypeReference oldRef, TypeReference newRef)
		{
			foreach (var obj in collection.Where(o => o.VariableType == oldRef))
			{
				obj.VariableType = newRef;
			}
		}

		private void Replace(ICollection<ExceptionHandler> collection, TypeReference oldRef, TypeReference newRef)
		{
			foreach (var obj in collection)
			{
				if (obj.CatchType == oldRef)
				{
					obj.CatchType = newRef;
				}
			}
		}
	}
}
