﻿using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using DandyDoc.Utility;

namespace DandyDoc.Reflection
{
    public static class ReflectionUtilities
    {

        public static string GetFilePath(this Assembly assembly) {
            if (assembly == null) throw new ArgumentNullException("assembly");
            Contract.EndContractBlock();

            var codeBase = assembly.CodeBase;
            Uri uri;
            if (Uri.TryCreate(codeBase, UriKind.Absolute, out uri) && "FILE".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
                return uri.AbsolutePath;

            return assembly.Location;
        }

        public static bool IsStatic(this PropertyInfo propertyInfo){
            if(propertyInfo == null) throw new NullReferenceException("propertyInfo is null");
            Contract.EndContractBlock();
            var method = propertyInfo.GetGetMethod(true) ?? propertyInfo.GetSetMethod(true);
            return method != null && method.IsStatic;
        }

        public static bool IsStatic(this EventInfo eventInfo){
            if(eventInfo == null) throw new NullReferenceException("eventInfo is null");
            Contract.EndContractBlock();
            var method = eventInfo.GetAddMethod(true) ?? eventInfo.GetRaiseMethod(true);
            return method != null && method.IsStatic;
        }

        public static bool IsStatic(this Type type){
            if(type == null) throw new NullReferenceException("type is null");
            Contract.EndContractBlock();
            return type.IsAbstract && type.IsSealed;
        }

        public static bool IsOperatorOverload(this MethodBase methodBase) {
            if (methodBase == null) throw new NullReferenceException("methodBase is null");
            Contract.EndContractBlock();
            if (!methodBase.IsStatic)
                return false;
            return CSharpOperatorNameSymbolMap.IsOperatorName(methodBase.Name);
        }

        public static bool IsFinalizer(this MethodBase methodBase) {
            if (methodBase == null) throw new NullReferenceException("methodBase is null");
            Contract.EndContractBlock();
            return !methodBase.IsStatic
                && "Finalize".Equals(methodBase.Name)
                && methodBase.GetParameters().Length == 0;
        }

        public static bool IsItemIndexerProperty(this PropertyInfo propertyInfo) {
            if(propertyInfo == null) throw new NullReferenceException("propertyInfo is null");
            Contract.EndContractBlock();
            return "Item".Equals(propertyInfo.Name)
                && propertyInfo.GetIndexParameters().Length > 0;
        }

        public static bool IsDelegateType(this Type type) {
            if (type == null)
                return false;

            var baseType = type.BaseType;
            if (baseType == null)
                return false;

            if (!"System.MulticastDelegate".Equals(baseType.FullName))
                return false;

            var methods = type.GetAllMethods();
            return methods.Length > 0
                && methods.Any(x => "Invoke".Equals(x.Name));
        }

        public static ParameterInfo[] GetDelegateTypeParameters(this Type type) {
            Contract.Ensures(Contract.Result<ParameterInfo[]>() != null);

            if (!IsDelegateType(type))
                return new ParameterInfo[0];

            var invokeMethod = type.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
            if (invokeMethod == null)
                return new ParameterInfo[0];

            return invokeMethod.GetParameters();
        }

        public static Type GetDelegateReturnType(this Type type) {
            if (!IsDelegateType(type))
                return null;

            var invokeMethod = type.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public);
            if (invokeMethod == null)
                return null;

            return invokeMethod.ReturnType;
        }

        public static ConstructorInfo[] GetAllConstructors(this Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<ConstructorInfo[]>() != null);
            return type.GetConstructors(
                BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic);
        }

        public static MethodInfo[] GetAllMethods(this Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<MethodInfo[]>() != null);
            return type.GetMethods(
                BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic);
        }

        public static PropertyInfo[] GetAllProperties(this Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<PropertyInfo[]>() != null);
            return type.GetProperties(
                BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic);
        }

        public static FieldInfo[] GetAllFields(this Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<FieldInfo[]>() != null);
            return type.GetFields(
                BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic);
        }

        public static EventInfo[] GetAllEvents(this Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<EventInfo[]>() != null);
            return type.GetEvents(
                BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic);
        }

        public static Type[] GetAllNestedTypes(this Type type) {
            if (type == null) throw new ArgumentNullException("type");
            Contract.Ensures(Contract.Result<Type[]>() != null);

            var result = type.GetNestedTypes(BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic);
            return result;
        }

    }
}
