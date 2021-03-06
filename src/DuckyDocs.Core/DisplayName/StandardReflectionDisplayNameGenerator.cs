﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DuckyDocs.Reflection;
using DuckyDocs.Utility;

namespace DuckyDocs.DisplayName
{

    /// <summary>
    /// Generates display names for reflected members.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note: the resulting display name can not be resolved back into the
    /// generating declaration or reference as it may be missing critical information.
    /// Use <see cref="DuckyDocs.CRef.ReflectionCRefGenerator"/> if a unique and reversible
    /// identifying name is required.
    /// </para>
    /// </remarks>
    public class StandardReflectionDisplayNameGenerator
    {

        /// <summary>
        /// Creates a default display name generator.
        /// </summary>
        public StandardReflectionDisplayNameGenerator() {
            IncludeNamespaceForTypes = false;
            ShowGenericParametersOnDefinition = true;
            ShowTypeNameForMembers = false;
            ListSeparator = ", ";
        }

        /// <summary>
        /// Gets a value indicating if namespaces will be included.
        /// </summary>
        public bool IncludeNamespaceForTypes { get; set; }

        /// <summary>
        /// Gets a value indicating if generic parameters will be added to generic members.
        /// </summary>
        public bool ShowGenericParametersOnDefinition { get; set; }

        /// <summary>
        /// Gets a value indicating if declaring types will be added to members.
        /// </summary>
        public bool ShowTypeNameForMembers { get; set; }

        /// <summary>
        /// Gets the list separator.
        /// </summary>
        public string ListSeparator { get; set; }

        /// <summary>
        /// Generates a display name for the given member.
        /// </summary>
        /// <param name="memberInfo">The member to generate a display name for.</param>
        /// <returns>A display name.</returns>
        public string GetDisplayName(MemberInfo memberInfo) {
            if (null == memberInfo) throw new ArgumentNullException("memberInfo");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            if (memberInfo is Type)
                return GetDisplayName((Type)memberInfo, false);
            if (memberInfo is MethodBase)
                return GetDisplayName((MethodBase)memberInfo);
            if (memberInfo is PropertyInfo)
                return GetDisplayName((PropertyInfo)memberInfo);
            return GetGenericDisplayName(memberInfo);
        }

        /// <summary>
        /// Generates a display name for the given method.
        /// </summary>
        /// <param name="methodBase">The method to generate a display name for.</param>
        /// <returns>A display name.</returns>
        public string GetDisplayName(MethodBase methodBase) {
            if (null == methodBase) throw new ArgumentNullException("methodBase");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            string name;
            if (methodBase.IsConstructor) {
                Contract.Assume(methodBase.DeclaringType != null);
                var typeName = methodBase.DeclaringType.Name;
                if (methodBase.DeclaringType.GetGenericArguments().Length > 0) {
                    var tickIndex = typeName.LastIndexOf('`');
                    if (tickIndex >= 0)
                        typeName = typeName.Substring(0, tickIndex);
                }
                name = typeName;
            }
            else if (methodBase.IsOperatorOverload()) {
                if (CSharpOperatorNameSymbolMap.TryGetOperatorSymbol(methodBase.Name, out name)) {
                    name = String.Concat("operator ", name);
                }
                else {
                    name = methodBase.Name;
                    if (name.StartsWith("op_"))
                        name = name.Substring(3);
                }
            }
            else {
                name = methodBase.Name;
                var genericParameters = methodBase.GetGenericArguments();
                if (genericParameters.Length > 0) {
                    var tickIndex = name.LastIndexOf('`');
                    if (tickIndex >= 0)
                        name = name.Substring(0, tickIndex);
                    name = String.Concat(
                        name,
                        '<',
                        String.Join(ListSeparator, genericParameters.Select(GetDisplayName)),
                        '>');
                }
            }

            var parameters = methodBase.GetParameters();
            Contract.Assume(parameters != null);
            name = String.Concat(name, '(', GetParameterText(parameters), ')');

            if (ShowTypeNameForMembers) {
                Contract.Assume(null != methodBase.DeclaringType);
                name = String.Concat(GetDisplayName(methodBase.DeclaringType), '.', name);
            }

            return name;
        }

        /// <summary>
        /// Generates a display name for the given property.
        /// </summary>
        /// <param name="propertyInfo">The property to generate a display name for.</param>
        /// <returns>A display name.</returns>
        public string GetDisplayName(PropertyInfo propertyInfo) {
            if (null == propertyInfo) throw new ArgumentNullException("propertyInfo");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var name = propertyInfo.Name;
            var parameters = propertyInfo.GetIndexParameters();
            Contract.Assume(parameters != null);
            if (parameters.Length > 0) {
                char openParen, closeParen;
                if ("Item".Equals(name)) {
                    openParen = '[';
                    closeParen = ']';
                }
                else {
                    openParen = '(';
                    closeParen = ')';
                }

                name = String.Concat(
                    name,
                    openParen,
                    GetParameterText(parameters),
                    closeParen);
            }
            if (ShowTypeNameForMembers) {
                Contract.Assume(null != propertyInfo.DeclaringType);
                name = String.Concat(GetDisplayName(propertyInfo.DeclaringType), '.', name);
            }
            return name;
        }

        /// <summary>
        /// Generates a display name for the given type.
        /// </summary>
        /// <param name="type">The type to generate a display name for.</param>
        /// <returns>A display name.</returns>
        public string GetDisplayName(Type type) {
            if (null == type) throw new ArgumentNullException("type");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            return GetDisplayName(type, false);
        }

        private string GetParameterText(IEnumerable<ParameterInfo> parameters) {
            if (null == parameters) throw new ArgumentNullException("parameters");
            Contract.EndContractBlock();
            return String.Join(ListSeparator, parameters.Select(GetParameterText));
        }

        private string GetParameterText(ParameterInfo parameterInfo) {
            Contract.Requires(null != parameterInfo);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            Contract.Assume(parameterInfo.ParameterType != null);
            return GetDisplayName(parameterInfo.ParameterType, false);
        }

        private string GetTypeDisplayName(Type type) {
            Contract.Requires(type != null);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var result = type.Name;
            if (ShowGenericParametersOnDefinition) {
                var genericParameters = type.GetGenericArguments();
                if (genericParameters.Length > 0) {
                    var tickIndex = result.LastIndexOf('`');
                    if (tickIndex >= 0)
                        result = result.Substring(0, tickIndex);

                    if (type.IsNested) {
                        Contract.Assume(type.DeclaringType != null);
                        var parentGenericParameters = type.DeclaringType.GetGenericArguments();
                        if (parentGenericParameters.Length > 0)
                            genericParameters = genericParameters.Where(p => parentGenericParameters.All(t => t.Name != p.Name)).ToArray();
                    }

                    if (genericParameters.Length > 0) {
                        result = String.Concat(
                            result,
                            '<',
                            String.Join(
                                ListSeparator,
                                genericParameters.Select(GetDisplayName)),
                            '>'
                        );
                    }
                }
            }
            return result;
        }

        private string GetDisplayName(Type type, bool hideParams) {
            if (null == type) throw new ArgumentNullException("type");
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            if (type.IsGenericParameter)
                return type.Name;

            var rootTypeReference = type;
            string fullTypeName;
            if (ShowTypeNameForMembers) {
                fullTypeName = GetNestedTypeDisplayName(ref rootTypeReference);
            }
            else {
                fullTypeName = GetTypeDisplayName(type);
                while (rootTypeReference.DeclaringType != null) {
                    rootTypeReference = rootTypeReference.DeclaringType;
                }
            }

            if (IncludeNamespaceForTypes && !String.IsNullOrEmpty(rootTypeReference.Namespace))
                fullTypeName = String.Concat(rootTypeReference.Namespace, '.', fullTypeName);

            if (type.IsDelegateType() && !hideParams)
                fullTypeName = String.Concat(fullTypeName, '(', GetParameterText(type.GetDelegateTypeParameters()), ')');

            return fullTypeName;
        }

        /// <summary>
        /// Walks up the declaring types while accumulating a nested type name as the result.
        /// </summary>
        /// <param name="type">The type to be walked and mutated.</param>
        /// <returns>The full nested type name.</returns>
        private string GetNestedTypeDisplayName(ref Type type) {
            Contract.Requires(null != type);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var typeParts = new List<string>();
            while (null != type) {
                typeParts.Insert(0, GetTypeDisplayName(type));
                if (!type.IsNested)
                    break;

                Contract.Assume(null != type.DeclaringType);
                type = type.DeclaringType;
            }
            return typeParts.Count == 1
                ? typeParts[0]
                : String.Join(".", typeParts);
        }

        private string GetGenericDisplayName(MemberInfo memberInfo) {
            Contract.Requires(null != memberInfo);
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            var name = memberInfo.Name;
            if (ShowTypeNameForMembers) {
                Contract.Assume(null != memberInfo.DeclaringType);
                name = String.Concat(GetDisplayName(memberInfo.DeclaringType), '.', name);
            }
            return name;
        }

    }
}
