﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DandyDoc.Overlays.DisplayName
{

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// <para>
	/// Note: the resulting display name can not be resolved back into the
	/// generating declaration or reference as it may be missing critical information.
	/// Use <see cref="DandyDoc.Overlays.Cref.CrefOverlay"/> if a unique and reversible
	/// identifying name is required.
	/// </para>
	/// </remarks>
	public class DisplayNameOverlay
	{

		private static readonly Dictionary<string, string> OperatorSymbols = new Dictionary<string, string>{
			{"op_Addition","+"},
			{"op_Subtraction","-"},
			{"op_Multiply","*"},
			{"op_Division","/"},
			{"op_Modulus","%"},
			{"op_ExclusiveOr","^"},
			{"op_BitwiseAnd","&"},
			{"op_BitwiseOr","|"},
			{"op_LogicalAnd","&&"},
			{"op_LogicalOr","||"},
			{"op_Assign","="},
			{"op_LeftShift","<<"},
			{"op_RightShift",">>"},
			{"op_Equality","=="},
			{"op_GreaterThan",">"},
			{"op_LessThan","<"},
			{"op_Inequality","!="},
			{"op_GreaterThanOrEqual",">="},
			{"op_LessThanOrEqual","<="},
			{"op_MultiplicationAssignment","*="},
			{"op_SubtractionAssignment","-="},
			{"op_ExclusiveOrAssignment","^="},
			{"op_LeftShiftAssignment","<<="},
			{"op_ModulusAssignment","%="},
			{"op_AdditionAssignment","+="},
			{"op_BitwiseAndAssignment","&="},
			{"op_BitwiseOrAssignment","|="},
			{"op_Comma",","},
			{"op_DivisionAssignment","/="},
			{"op_Decrement","--"},
			{"op_Increment","++"},
			{"op_UnaryNegation","-"},
			{"op_UnaryPlus","+"},
			{"op_OnesComplement","~"}
		};

		public static bool TryGetOperatorSymbol(string operatorName, out string symbol) {
			if(null == operatorName) throw new ArgumentNullException("operatorName");
			Contract.EndContractBlock();
			return OperatorSymbols.TryGetValue(operatorName, out symbol);
		}

		private static readonly DisplayNameOverlay DefaultParamDisplayNameOverlay = new DisplayNameOverlay();

		public DisplayNameOverlay(){
			IncludeNamespaceForTypes = false;
			ShowGenericParametersOnDefinition = true;
			ShowTypeNameForMembers = false;
			ListSeperator = ", ";
			ParameterTypeDisplayNameOverlay = null;
		}

		public bool ShowGenericParametersOnDefinition { get; set; }

		public bool IncludeNamespaceForTypes { get; set; }

		public bool ShowTypeNameForMembers { get; set; }

		public string ListSeperator { get; set; }

		public DisplayNameOverlay ParameterTypeDisplayNameOverlay { get; set; }

		private string GetTypeDisplayName(TypeReference reference){
			Contract.Requires(null != reference);
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			var result = reference.Name;
			if (ShowGenericParametersOnDefinition){
				if (reference is TypeDefinition){
					var definition = (TypeDefinition) reference;
					if (definition.HasGenericParameters){
						var tickIndex = result.LastIndexOf('`');
						if (tickIndex >= 0)
							result = result.Substring(0, tickIndex);

						IList<GenericParameter> genericParameters = definition.GenericParameters;
						if (definition.IsNested && definition.DeclaringType.HasGenericParameters){
							var parentGenericParams = definition.DeclaringType.GenericParameters;
							genericParameters = genericParameters.Where(p => parentGenericParams.All(t => t.Name != p.Name)).ToList();
						}

						if (genericParameters.Count > 0){
							result = String.Concat(
								result,
								'<',
								String.Join(ListSeperator, genericParameters.Select(GetDisplayName)),
								'>');
						}
					}
				}
				else if (reference.IsGenericInstance){
					var genericInstanceType = reference as GenericInstanceType;
					Contract.Assume(null != genericInstanceType);
					var tickIndex = result.LastIndexOf('`');
					if (tickIndex >= 0)
						result = result.Substring(0, tickIndex);
					result = String.Concat(
						result,
						'<',
						String.Join(ListSeperator, genericInstanceType.GenericArguments.Select(GetDisplayName)),
						'>');
				}
			}
			return result;
		}

		private string GetNestedTypeDisplayName(ref TypeReference reference){
			Contract.Requires(null != reference);
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			var typeParts = new List<string>();
			while (null != reference) {
				typeParts.Insert(0, GetTypeDisplayName(reference));
				if (!reference.IsNested)
					break;

				Contract.Assume(null != reference.DeclaringType);
				reference = reference.DeclaringType;
			}
			return typeParts.Count == 1
				? typeParts[0]
				: String.Join(".", typeParts);
		}

		public string GetDisplayName(TypeReference reference){
			if (null == reference) throw new ArgumentNullException("reference");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

			if (reference.IsGenericParameter)
				return reference.Name;

			var fullTypeName = ShowTypeNameForMembers
				? GetNestedTypeDisplayName(ref reference)
				: GetTypeDisplayName(reference);

			if (IncludeNamespaceForTypes && !String.IsNullOrEmpty(reference.Namespace))
				fullTypeName = String.Concat(reference.Namespace, '.', fullTypeName);

			return fullTypeName;
		}

		public string GetDisplayName(MethodDefinition definition){
			if(null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			string name;
			if (definition.IsConstructor){
				var typeName = definition.DeclaringType.Name;
				if (definition.DeclaringType.HasGenericParameters){
					var tickIndex = typeName.LastIndexOf('`');
					if (tickIndex >= 0)
						typeName = typeName.Substring(0, tickIndex);
				}
				name = typeName;
			}
			else if (definition.IsOperatorOverload()){
				Contract.Assume(!String.IsNullOrEmpty(definition.Name));
				if (TryGetOperatorSymbol(definition.Name, out name)){
					name = String.Concat("operator ", name);
				}
				else{
					name = definition.Name;
					if (name.StartsWith("op_"))
						name = name.Substring(3);
				}
			}
			else{
				name = definition.Name;
				if (definition.HasGenericParameters){
					var tickIndex = name.LastIndexOf('`');
					if (tickIndex >= 0)
						name = name.Substring(0, tickIndex);
					name = String.Concat(
						name,
						'<',
						String.Join(ListSeperator, definition.GenericParameters.Select(GetDisplayName)),
						'>');
				}
			}

			var paramNameGenerator = ParameterTypeDisplayNameOverlay ?? DefaultParamDisplayNameOverlay;
			name = String.Concat(
				name,
				'(',
				String.Join(ListSeperator, definition.Parameters.Select(p => paramNameGenerator.GetDisplayName(p.ParameterType))),
				')');

			if (ShowTypeNameForMembers){
				Contract.Assume(null != definition.DeclaringType);
				name = String.Concat(GetDisplayName(definition.DeclaringType), '.', name);
			}

			return name;
		}

		public string GetDisplayName(PropertyDefinition definition){
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			var name = definition.Name;
			if (definition.HasParameters){
				var paramNameGenerator = ParameterTypeDisplayNameOverlay ?? DefaultParamDisplayNameOverlay;
				char openParen, closeParen;
				if ("Item".Equals(name)){
					openParen = '[';
					closeParen = ']';
				}
				else{
					openParen = '(';
					closeParen = ')';
				}

				name = String.Concat(
					name,
					openParen,
					String.Join(ListSeperator, definition.Parameters.Select(p => paramNameGenerator.GetDisplayName(p.ParameterType))),
					closeParen);
			}
			if (ShowTypeNameForMembers){
				Contract.Assume(null != definition.DeclaringType);
				name = String.Concat(GetDisplayName(definition.DeclaringType), '.', name);
			}
			return name;
		}

		private string GetGenericDisplayName(IMemberDefinition definition){
			Contract.Requires(null != definition);
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			var name = definition.Name;
			if (ShowTypeNameForMembers) {
				Contract.Assume(null != definition.DeclaringType);
				name = String.Concat(GetDisplayName(definition.DeclaringType), '.', name);
			}
			return name;
		}

		public string GetDisplayName(MemberReference reference){
			if (null == reference) throw new ArgumentNullException("reference");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			if (reference is TypeReference)
				return GetDisplayName((TypeReference) reference);
			if (reference is MethodDefinition)
				return GetDisplayName((MethodDefinition) reference);
			if (reference is PropertyDefinition)
				return GetDisplayName((PropertyDefinition) reference);
			if (reference is IMemberDefinition)
				return GetGenericDisplayName((IMemberDefinition) reference);
			throw new NotSupportedException();
		}

		public string GetDisplayName(IMemberDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			Contract.Assume(null != (definition as MemberReference));
			return GetDisplayName(definition as MemberReference);
		}

		public string GetDisplayName(TypeDefinition definition) {
			if (null == definition) throw new ArgumentNullException("definition");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
			return GetDisplayName((TypeReference)definition);
		}

	}
}