﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;
using System.Collections.ObjectModel;

namespace DandyDoc.Core
{
	public static class CecilExtensions
	{

		public static bool IsDelegateType(this TypeDefinition typeDefinition){
			if (null == typeDefinition)
				return false;

			var baseType = typeDefinition.BaseType;
			if (null == baseType)
				return false;

			if (!baseType.FullName.Equals("System.MulticastDelegate"))
				return false;

			return typeDefinition.HasMethods && typeDefinition.Methods.Any(x => "Invoke".Equals(x.Name));
		}

		private static readonly ReadOnlyCollection<ParameterDefinition> EmptyParameterDefinitionCollection = Array.AsReadOnly(new ParameterDefinition[0]);

		public static IList<ParameterDefinition> GetDelegateTypeParameters(this TypeDefinition typeDefinition){
			Contract.Ensures(Contract.Result<IEnumerable<ParameterDefinition>>() != null);
			if (!IsDelegateType(typeDefinition))
				return EmptyParameterDefinitionCollection;

			var method = typeDefinition.Methods.FirstOrDefault(x => "Invoke".Equals(x.Name));
			return null == method || !method.HasParameters
				? (IList<ParameterDefinition>)EmptyParameterDefinitionCollection
				: method.Parameters;
		}

		private static readonly HashSet<string> OperatorMethodNames = new HashSet<string>{
			"op_Implicit",
			"op_explicit",
			"op_Addition",
			"op_Subtraction",
			"op_Multiply",
			"op_Division",
			"op_Modulus",
			"op_ExclusiveOr",
			"op_BitwiseAnd",
			"op_BitwiseOr",
			"op_LogicalAnd",
			"op_LogicalOr",
			"op_Assign",
			"op_LeftShift",
			"op_RightShift",
			"op_SignedRightShift",
			"op_UnsignedRightShift",
			"op_Equality",
			"op_GreaterThan",
			"op_LessThan",
			"op_Inequality",
			"op_GreaterThanOrEqual",
			"op_LessThanOrEqual",
			"op_MultiplicationAssignment",
			"op_SubtractionAssignment",
			"op_ExclusiveOrAssignment",
			"op_LeftShiftAssignment",
			"op_ModulusAssignment",
			"op_AdditionAssignment",
			"op_BitwiseAndAssignment",
			"op_BitwiseOrAssignment",
			"op_Comma",
			"op_DivisionAssignment",
			"op_Decrement",
			"op_Increment",
			"op_UnaryNegation",
			"op_UnaryPlus",
			"op_OnesComplement"
		};

		public static bool IsOperatorOverload(this MethodDefinition methodDefinition){
			if(null == methodDefinition) throw new ArgumentNullException("methodDefinition");
			Contract.EndContractBlock();
			if (!methodDefinition.IsStatic)
				return false;
			Contract.Assume(!String.IsNullOrEmpty(methodDefinition.Name));
			return OperatorMethodNames.Contains(methodDefinition.Name);
		}

		public static bool IsFinalizer(this MethodDefinition methodDefinition){
			if (null == methodDefinition) throw new ArgumentNullException("methodDefinition");
			Contract.EndContractBlock();
			return !methodDefinition.IsStatic
				&& !methodDefinition.HasParameters
				&& "Finalize".Equals(methodDefinition.Name);
		}

		public static bool IsStatic(this PropertyDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var method = definition.GetMethod ?? definition.SetMethod;
			return null != method && method.IsStatic;
		}

		public static bool IsStatic(this EventDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var method = definition.AddMethod ?? definition.InvokeMethod;
			return null != method && method.IsStatic;
		}

		public static bool IsExternallyExposed(this MethodDefinition definition) {
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			if (definition.IsPublic)
				return true;
			if (definition.IsFamily)
				return !definition.IsFamilyAndAssembly;
			return false;
		}

		public static bool IsExternallyExposed(this FieldDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			if (definition.IsPublic)
				return true;
			if (definition.IsFamily)
				return !definition.IsFamilyAndAssembly;
			return false;
		}

		public static bool IsExternallyExposed(this PropertyDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var getMethod = definition.GetMethod;
			if (null != getMethod) {
				if (getMethod.IsExternallyExposed())
					return true;
				var setMethod = definition.SetMethod;
				return null != setMethod && setMethod.IsExternallyExposed();
			}
			else {
				var setMethod = definition.SetMethod;
				return null != setMethod && setMethod.IsExternallyExposed();
			}
		}

		public static bool IsExternallyExposed(this EventDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			var method = definition.AddMethod ?? definition.InvokeMethod;
			return null != method && method.IsExternallyExposed();
		}

		public static bool IsExternallyExposed(this TypeDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.EndContractBlock();
			if (definition.IsNested) {
				Contract.Assume(null != definition.DeclaringType);
				return definition.DeclaringType.IsExternallyExposed() && (
					definition.IsPublic
					|| definition.IsNestedPublic
					|| (
						definition.IsNestedFamily
						&& !definition.IsNestedFamilyAndAssembly
					)
				);
			}
			return definition.IsPublic;
		}

	}
}
