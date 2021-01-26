using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UIForia.Elements;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UnityEngine;

namespace UIForia.Util {

    public static class ReflectionUtil {

        private struct GenericTypeEntry {

            public readonly Type[] paramTypes;
            public readonly Type retnType;
            public readonly Type baseType;

            public GenericTypeEntry(Type baseType, Type[] paramTypes, Type retnType) {
                this.baseType = baseType;
                this.paramTypes = paramTypes;
                this.retnType = retnType;
            }

        }

        private struct DelegateEntry {

            public readonly Delegate instance;
            public readonly MethodInfo methodInfo;

            public DelegateEntry(Delegate instance, MethodInfo methodInfo) {
                this.instance = instance;
                this.methodInfo = methodInfo;
            }

        }

        public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        public const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        public const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        public const BindingFlags InstanceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        public const BindingFlags InterfaceBindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
        private static readonly LRUCache<Type, MethodInfo[]> s_InstanceMethodCache = new LRUCache<Type, MethodInfo[]>(128);
        private static readonly LRUCache<Type, MethodInfo[]> s_StaticMethodCache = new LRUCache<Type, MethodInfo[]>(128);

        private static readonly List<GenericTypeEntry> generics = new List<GenericTypeEntry>();
        private static readonly List<DelegateEntry> staticDelegates = new List<DelegateEntry>();
        private static readonly List<DelegateEntry> openDelegates = new List<DelegateEntry>();
        // private static ClassBuilder classBuilder;

        private static readonly Dictionary<Type, List<LinqAccessor>> linqDelegates = new Dictionary<Type, List<LinqAccessor>>();

        public static readonly object[] ObjectArray0 = new object[0];
        public static readonly object[] ObjectArray1 = new object[1];
        public static readonly object[] ObjectArray2 = new object[2];
        public static readonly object[] ObjectArray3 = new object[3];
        public static readonly object[] ObjectArray4 = new object[4];
        public static readonly object[] ObjectArray5 = new object[5];

        public static readonly Type[] TypeArray1 = new Type[1];
        public static readonly Type[] TypeArray2 = new Type[2];
        public static readonly Type[] TypeArray3 = new Type[3];
        public static readonly Type[] TypeArray4 = new Type[4];

        public static Type GetArrayElementTypeOrThrow(Type targetType) {
            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (targetType.IsArray) {
                return targetType.GetElementType();
            }

            Type[] interfaces = targetType.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++) {
                if (interfaces[i].IsGenericType) {
                    Type definition = interfaces[i].GetGenericTypeDefinition();
                    if (definition == typeof(IList<>)) {
                        return interfaces[i].GetGenericArguments()[0];
                    }
                }
            }

            for (int i = 0; i < interfaces.Length; i++) {
                if (interfaces[i] == typeof(IList)) {
                    return typeof(object);
                }
            }

            throw new Exception($"Trying to read the element type of {targetType.Name} but it is not a list type");
        }

        public static FieldInfo GetFieldInfo(Type type, string fieldName) {
            return type.GetField(fieldName, InstanceBindFlags);
        }

        public static FieldInfo GetStaticFieldInfo(Type type, string fieldName) {
            return type.GetField(fieldName, StaticFlags);
        }

        public static FieldInfo GetInstanceOrStaticFieldInfo(Type type, string fieldName) {
            return type.GetField(fieldName, StaticFlags | InstanceBindFlags);
        }

        public static PropertyInfo GetPropertyInfo(Type type, string propertyName) {
            return type.GetProperty(propertyName, InstanceBindFlags);
        }

        public static PropertyInfo GetStaticPropertyInfo(Type type, string fieldName) {
            return type.GetProperty(fieldName, StaticFlags);
        }

        public static PropertyInfo GetInstanceOrStaticPropertyInfo(Type type, string propertyType) {
            return type.GetProperty(propertyType, StaticFlags | InstanceBindFlags);
        }

        public static FieldInfo GetFieldInfoOrThrow(Type type, string fieldName) {
            FieldInfo fieldInfo = type.GetField(fieldName, InstanceBindFlags);
            if (fieldInfo == null) {
                throw new Exception($"Field called {fieldName} was not found on type {type.Name}");
            }

            return fieldInfo;
        }

        public static PropertyInfo GetPropertyInfoOrThrow(Type type, string propertyName) {
            PropertyInfo propertyInfo = type.GetProperty(propertyName, InstanceBindFlags);
            if (propertyInfo == null) {
                throw new Exception($"Property called {propertyName} was not found on type {type.Name}");
            }

            return propertyInfo;
        }

        public static bool IsField(Type type, string fieldName) {
            return type.GetField(fieldName, InstanceBindFlags) != null;
        }

        public static bool IsEvent(Type type, string evtName, out EventInfo evtInfo) {
            evtInfo = type.GetEvent(evtName, InstanceBindFlags);
            return evtInfo != null;
        }

        public static bool IsField(Type type, string fieldName, out FieldInfo fieldInfo) {
            try {
                fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                return fieldInfo != null;
            }
            catch (NotSupportedException ex) {
                fieldInfo = null;
                return false;
            }
        }

        public static bool IsProperty(Type type, string propertyName) {
            return type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy) != null;
        }

        public static bool IsProperty(Type type, string propertyName, out PropertyInfo propertyInfo) {
            try {
                propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                return propertyInfo != null;
            }
            catch (NotSupportedException ex) {
                propertyInfo = null;
                return false;
            }
        }

        public static bool IsMethod(Type type, string methodName, out MethodInfo methodInfo) {
            methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            return methodInfo != null;
        }

        public static MethodInfo[] GetInstanceMethods(Type type) {
            if (s_InstanceMethodCache.TryGet(type, out MethodInfo[] v)) {
                return v;
            }
            else {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                s_InstanceMethodCache.Add(type, methods);
                return methods;
            }
        }

        public static IList<MethodInfo> GetAllInstanceMethodsSlow(Type type, string name) {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToList().Where(m => m.Name == name).ToList();
        }

        public static MethodInfo[] GetStaticMethods(Type type) {
            if (s_StaticMethodCache.TryGet(type, out MethodInfo[] v)) {
                return v;
            }

            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            s_StaticMethodCache.Add(type, methods);
            return methods;
        }
//        
//        public static void GetPublicStaticMethods(Type type, LightList<MethodInfo> retn) {
//            MethodInfo[] methods = GetInstanceMethods(type);
//            for (int i = 0; i < methods.Length; i++) {
//                if (methods[i].IsPublic){
//                    retn.Add(methods[i]);
//                }
//            }
//        }
//
//        public static void GetPublicInstanceMethods(Type type, LightList<MethodInfo> retn) {
//            MethodInfo[] methods = GetInstanceMethods(type);
//            for (int i = 0; i < methods.Length; i++) {
//                if (!methods[i].IsStatic && methods[i].IsPublic) {
//                    retn.Add(methods[i]);
//                }
//            }
//        }

        public static bool HasInstanceMethod(Type type, string methodName, LightList<MethodInfo> retn) {
            MethodInfo[] methods = GetInstanceMethods(type);

            for (int i = 0; i < methods.Length; i++) {
                if (methods[i].Name == methodName) {
                    retn.Add(methods[i]);
                }
            }

            return retn.size != 0;
        }

        public static bool HasStaticMethod(Type type, string methodName, LightList<MethodInfo> retn) {
            MethodInfo[] methods = GetStaticMethods(type);

            for (int i = 0; i < methods.Length; i++) {
                if (methods[i].Name == methodName) {
                    retn.Add(methods[i]);
                }
            }

            return retn.size != 0;
        }

        public static Type GetFieldType(Type type, string fieldName) {
            return GetFieldInfoOrThrow(type, fieldName).FieldType;
        }

        public static Type GetPropertyType(Type type, string propertyName) {
            return GetPropertyInfoOrThrow(type, propertyName).PropertyType;
        }

        public static Type GetCommonBaseClass(IList<Type> types) {
            if (types.Count == 0) {
                return null;
            }

            Type ret = types[0];

            for (int i = 1; i < types.Count; ++i) {
                if (types[i].IsAssignableFrom(ret))
                    ret = types[i];
                else {
                    // This will always terminate when ret == typeof(object)
                    while (!ret.IsAssignableFrom(types[i]))
                        ret = ret.BaseType;
                }
            }

            return ret;
        }

        public static Type GetCommonBaseClass(Type type0, Type type1) {
            Type oldT0 = TypeArray2[0];
            Type oldT1 = TypeArray2[1];

            TypeArray2[0] = type0;
            TypeArray2[1] = type1;

            Type retn = GetCommonBaseClass(TypeArray2);

            TypeArray2[0] = oldT0;
            TypeArray2[1] = oldT1;

            return retn;
        }

        public static bool IsIntegralType(Type o) {
            switch (Type.GetTypeCode(o)) {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFloatingPointType(Type o) {
            switch (Type.GetTypeCode(o)) {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNumericType(Type o) {
            switch (Type.GetTypeCode(o)) {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        // todo -- might need to reverse left & right in switches
        public static bool AreNumericTypesCompatible(Type left, Type right) {
            switch (Type.GetTypeCode(left)) {
                case TypeCode.Byte:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte: return true;
                        case TypeCode.Int16: return true;
                        case TypeCode.Int32: return true;
                        case TypeCode.Int64: return true;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default: return false;
                    }

                case TypeCode.Int16:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte: return false;
                        case TypeCode.Int16: return true;
                        case TypeCode.Int32: return true;
                        case TypeCode.Int64: return true;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default: return false;
                    }

                case TypeCode.Int32:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte: return false;
                        case TypeCode.Int16: return false;
                        case TypeCode.Int32: return true;
                        case TypeCode.Int64: return true;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default: return false;
                    }

                case TypeCode.Int64:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte: return false;
                        case TypeCode.Int16: return false;
                        case TypeCode.Int32: return false;
                        case TypeCode.Int64: return true;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default: return false;
                    }

                case TypeCode.Double:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte: return false;
                        case TypeCode.Int16: return false;
                        case TypeCode.Int32: return true;
                        case TypeCode.Int64: return false;
                        case TypeCode.Double: return true;
                        case TypeCode.Single: return true;
                        default: return false;
                    }

                case TypeCode.Single:
                    switch (Type.GetTypeCode(right)) {
                        case TypeCode.Byte: return false;
                        case TypeCode.Int16: return false;
                        case TypeCode.Int32: return true;
                        case TypeCode.Int64: return false;
                        case TypeCode.Double: return false;
                        case TypeCode.Single: return true;
                        default: return false;
                    }

                default:
                    return false;
            }
        }

        public static Type ResolveFieldOrPropertyType(Type type, string name) {
            FieldInfo fieldInfo = GetInstanceOrStaticFieldInfo(type, name);
            if (fieldInfo != null) {
                return fieldInfo.FieldType;
            }

            PropertyInfo propertyInfo = GetInstanceOrStaticPropertyInfo(type, name);

            if (propertyInfo != null) {
                return propertyInfo.PropertyType;
            }

            return null;
        }

        public static bool IsOverride(MethodInfo m) {
            // todo verify this works w/o passing in a type explicitly for base type
            return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }

        // todo -- doesn't warn about parameters, etc
        public static bool IsOverride(object target, string methodName) {
            MethodInfo info = target.GetType().GetMethod(methodName, PublicInstance, null, Type.EmptyTypes, null);
            return IsOverride(info);
        }

        public static object CreateGenericInstance(Type genericBase, params object[] args) {
            return Activator.CreateInstance(genericBase, args);
        }

        public static object CreateGenericInstanceFromOpenType(Type openBaseType, Type genericArgument, params object[] args) {
            Type genericType = openBaseType.MakeGenericType(genericArgument);
            return Activator.CreateInstance(genericType, args);
        }

        public static object CreateGenericInstanceFromOpenType(Type openBaseType, Type[] genericArguments, params object[] args) {
            Type genericType = openBaseType.MakeGenericType(genericArguments);
            return Activator.CreateInstance(genericType, args);
        }

        public static Type CreateGenericType(Type baseType, IReadOnlyList<Type> genericArguments) {
            Type[] genericArray = GetTempTypeArray(genericArguments.Count);


            for (int i = 0; i < genericArray.Length; i++) {
                genericArray[i] = genericArguments[i];
            }

            Type outputType = baseType.MakeGenericType(genericArray);
            ReleaseTempTypeArray(ref genericArray);
            return outputType;
        }

        public static Type CreateGenericType(Type baseType, params Type[] genericArguments) {
            return baseType.MakeGenericType(genericArguments);
            ;
        }

        public static Type[] SetTempTypeArray(Type type, Type type1) {
            TypeArray2[0] = type;
            TypeArray2[1] = type1;
            return TypeArray2;
        }

        public static Type[] GetTempTypeArray(int count) {
            switch (count) {
                case 0: return Type.EmptyTypes;
                case 1: return TypeArray1;
                case 2: return TypeArray2;
                case 3: return TypeArray3;
                case 4: return TypeArray4;
                default: return ArrayPool<Type>.GetExactSize(count);
            }
        }

        public static void ReleaseTempTypeArray(ref Type[] array) {
            if (array.Length == 0) return;
            if (array == TypeArray1) return;
            if (array == TypeArray2) return;
            if (array == TypeArray3) return;
            if (array == TypeArray4) return;
            ArrayPool<Type>.Release(ref array);
        }

        public static Type CreateNestedGenericType(Type containingType, Type nestedType, IList<Type> genericArguments) {
            // if the base type is nested the generic arguments will be projected onto it, be sure to use them
            Type[] projected = containingType.GetGenericArguments();
            Type[] genericArray = GetTempTypeArray(genericArguments.Count + projected.Length);

            int idx = 0;

            for (int i = 0; i < projected.Length; i++) {
                genericArray[idx++] = projected[i];
            }

            for (int i = 0; i < genericArguments.Count; i++) {
                genericArray[idx++] = genericArguments[i];
            }

            Type outputType = nestedType.MakeGenericType(genericArray);
            ReleaseTempTypeArray(ref genericArray);
            return outputType;
        }

        private static bool TypeParamsMatch(IList<Type> params0, IList<Type> params1) {
            if (params0.Count != params1.Count) return false;
            for (int i = 0; i < params0.Count; i++) {
                if (params0[i] != params1[i]) {
                    return false;
                }
            }

            return true;
        }

        public static bool HasAnyAttribute(MethodInfo methodInfo, params Type[] types) {
            for (int i = 0; i < types.Length; i++) {
                if (methodInfo.GetCustomAttribute(types[i]) != null) {
                    return true;
                }
            }

            return false;
        }

        public static bool HasAnyAttribute(FieldInfo fieldInfo, params Type[] types) {
            for (int i = 0; i < types.Length; i++) {
                if (fieldInfo.GetCustomAttribute(types[i]) != null) {
                    return true;
                }
            }

            return false;
        }

        public static TDelegateType CreateOpenDelegate<TDelegateType>(MethodInfo info) where TDelegateType : class {
            return Delegate.CreateDelegate(typeof(TDelegateType), null, info) as TDelegateType;
        }

        public static Delegate CreateOpenDelegate(Type type, MethodInfo info) {
            return Delegate.CreateDelegate(type, null, info);
        }

     
        public static Type GetOpenDelegateType(MethodInfo info) {
            ParameterInfo[] parameters = info.GetParameters();

            int additionalSize = info.ReturnType == typeof(void) ? 1 : 2;

            Type[] signatureTypes = new Type[parameters.Length + additionalSize];

            signatureTypes[0] = info.DeclaringType;

            for (int i = 1; i < parameters.Length + 1; i++) {
                signatureTypes[i] = parameters[i - 1].ParameterType;
            }

            if (info.ReturnType != typeof(void)) {
                signatureTypes[parameters.Length + 1] = info.ReturnType;
                switch (signatureTypes.Length) {
                    case 1:
                        return typeof(Func<>).MakeGenericType(signatureTypes);
                    case 2:
                        return typeof(Func<,>).MakeGenericType(signatureTypes);
                    case 3:
                        return typeof(Func<,,>).MakeGenericType(signatureTypes);
                    case 4:
                        return typeof(Func<,,,>).MakeGenericType(signatureTypes);
                    case 5:
                        return typeof(Func<,,,,>).MakeGenericType(signatureTypes);
                    case 6:
                        return typeof(Func<,,,,,>).MakeGenericType(signatureTypes);
                    case 7:
                        return typeof(Func<,,,,,,>).MakeGenericType(signatureTypes);
                    case 8:
                        return typeof(Func<,,,,,,,>).MakeGenericType(signatureTypes);
                    case 9:
                        return typeof(Func<,,,,,,,,>).MakeGenericType(signatureTypes);
                    case 10:
                        return typeof(Func<,,,,,,,,,>).MakeGenericType(signatureTypes);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            switch (signatureTypes.Length) {
                case 1:
                    return typeof(Action<>).MakeGenericType(signatureTypes);
                case 2:
                    return typeof(Action<,>).MakeGenericType(signatureTypes);
                case 3:
                    return typeof(Action<,,>).MakeGenericType(signatureTypes);
                case 4:
                    return typeof(Action<,,,>).MakeGenericType(signatureTypes);
                case 5:
                    return typeof(Action<,,,,>).MakeGenericType(signatureTypes);
                case 6:
                    return typeof(Action<,,,,,>).MakeGenericType(signatureTypes);
                case 7:
                    return typeof(Action<,,,,,,>).MakeGenericType(signatureTypes);
                case 8:
                    return typeof(Action<,,,,,,,>).MakeGenericType(signatureTypes);
                case 9:
                    return typeof(Action<,,,,,,,,>).MakeGenericType(signatureTypes);
                case 10:
                    return typeof(Action<,,,,,,,,,>).MakeGenericType(signatureTypes);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Type GetClosedDelegateType(Type[] parameters, Type returnType = null) {

            int additionalSize = returnType == typeof(void) ? 0 : 1;

            Type[] signatureTypes = new Type[parameters.Length + additionalSize];

            for (int i = 0; i < parameters.Length; i++) {
                signatureTypes[i] = parameters[i];
            }

            if (returnType != typeof(void)) {
                signatureTypes[parameters.Length] = returnType;

                switch (signatureTypes.Length) {
                    case 1:
                        return typeof(Func<>).MakeGenericType(signatureTypes);
                    case 2:
                        return typeof(Func<,>).MakeGenericType(signatureTypes);
                    case 3:
                        return typeof(Func<,,>).MakeGenericType(signatureTypes);
                    case 4:
                        return typeof(Func<,,,>).MakeGenericType(signatureTypes);
                    case 5:
                        return typeof(Func<,,,,>).MakeGenericType(signatureTypes);
                    case 6:
                        return typeof(Func<,,,,,>).MakeGenericType(signatureTypes);
                    case 7:
                        return typeof(Func<,,,,,,>).MakeGenericType(signatureTypes);
                    case 8:
                        return typeof(Func<,,,,,,,>).MakeGenericType(signatureTypes);
                    case 9:
                        return typeof(Func<,,,,,,,,>).MakeGenericType(signatureTypes);
                    case 10:
                        return typeof(Func<,,,,,,,,,>).MakeGenericType(signatureTypes);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            switch (signatureTypes.Length) {
                case 0:
                    return typeof(Action);
                case 1:
                    return typeof(Action<>).MakeGenericType(signatureTypes);
                case 2:
                    return typeof(Action<,>).MakeGenericType(signatureTypes);
                case 3:
                    return typeof(Action<,,>).MakeGenericType(signatureTypes);
                case 4:
                    return typeof(Action<,,,>).MakeGenericType(signatureTypes);
                case 5:
                    return typeof(Action<,,,,>).MakeGenericType(signatureTypes);
                case 6:
                    return typeof(Action<,,,,,>).MakeGenericType(signatureTypes);
                case 7:
                    return typeof(Action<,,,,,,>).MakeGenericType(signatureTypes);
                case 8:
                    return typeof(Action<,,,,,,,>).MakeGenericType(signatureTypes);
                case 9:
                    return typeof(Action<,,,,,,,,>).MakeGenericType(signatureTypes);
                case 10:
                    return typeof(Action<,,,,,,,,,>).MakeGenericType(signatureTypes);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        

        public static Type GetClosedDelegateType(MethodInfo info) {
            ParameterInfo[] parameters = info.GetParameters();

            int additionalSize = info.ReturnType == typeof(void) ? 0 : 1;

            Type[] signatureTypes = new Type[parameters.Length + additionalSize];

            for (int i = 0; i < parameters.Length; i++) {
                signatureTypes[i] = parameters[i].ParameterType;
            }

            if (info.ReturnType != typeof(void)) {
                signatureTypes[parameters.Length] = info.ReturnType;

                switch (signatureTypes.Length) {
                    case 1:
                        return typeof(Func<>).MakeGenericType(signatureTypes);
                    case 2:
                        return typeof(Func<,>).MakeGenericType(signatureTypes);
                    case 3:
                        return typeof(Func<,,>).MakeGenericType(signatureTypes);
                    case 4:
                        return typeof(Func<,,,>).MakeGenericType(signatureTypes);
                    case 5:
                        return typeof(Func<,,,,>).MakeGenericType(signatureTypes);
                    case 6:
                        return typeof(Func<,,,,,>).MakeGenericType(signatureTypes);
                    case 7:
                        return typeof(Func<,,,,,,>).MakeGenericType(signatureTypes);
                    case 8:
                        return typeof(Func<,,,,,,,>).MakeGenericType(signatureTypes);
                    case 9:
                        return typeof(Func<,,,,,,,,>).MakeGenericType(signatureTypes);
                    case 10:
                        return typeof(Func<,,,,,,,,,>).MakeGenericType(signatureTypes);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            switch (signatureTypes.Length) {
                case 0:
                    return typeof(Action);
                case 1:
                    return typeof(Action<>).MakeGenericType(signatureTypes);
                case 2:
                    return typeof(Action<,>).MakeGenericType(signatureTypes);
                case 3:
                    return typeof(Action<,,>).MakeGenericType(signatureTypes);
                case 4:
                    return typeof(Action<,,,>).MakeGenericType(signatureTypes);
                case 5:
                    return typeof(Action<,,,,>).MakeGenericType(signatureTypes);
                case 6:
                    return typeof(Action<,,,,,>).MakeGenericType(signatureTypes);
                case 7:
                    return typeof(Action<,,,,,,>).MakeGenericType(signatureTypes);
                case 8:
                    return typeof(Action<,,,,,,,>).MakeGenericType(signatureTypes);
                case 9:
                    return typeof(Action<,,,,,,,,>).MakeGenericType(signatureTypes);
                case 10:
                    return typeof(Action<,,,,,,,,,>).MakeGenericType(signatureTypes);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool SignatureMatches(MethodInfo one, MethodInfo two) {
            if (one.ReturnType != two.ReturnType) return false;
            if (one.Name != two.Name) return false;

            ParameterInfo[] parameters1 = one.GetParameters();
            ParameterInfo[] parameters2 = two.GetParameters();
            if (parameters1.Length != parameters2.Length) {
                return false;
            }

            for (int i = 0; i < parameters1.Length; i++) {
                if (parameters1[i].ParameterType != parameters2[i].ParameterType) {
                    return false;
                }
            }

            return true;
        }

        private static MethodInfo ResolvePossibleInterface(Type declaringType, MethodInfo original) {
            if (original.IsStatic) {
                return original;
            }

            Type[] interfaces = declaringType.GetInterfaces();

            if (interfaces.Length == 0) {
                return original;
            }

            for (int i = 0; i < interfaces.Length; i++) {
                MethodInfo interfaceMethod = interfaces[i]
                    .GetMethods(PublicInstance)
                    .FirstOrDefault((methodInfo) => SignatureMatches(original, methodInfo));

                if (interfaceMethod != null) {
                    return interfaceMethod;
                }
            }

            return original;
        }

        private static MethodInfo ResolvePossibleBaseClassMethod(Type declaringType, MethodInfo original) {
            Type baseClass = declaringType.BaseType;

            while (baseClass != null) {
                MethodInfo method = baseClass
                    .GetMethods(PublicInstance)
                    .FirstOrDefault((methodInfo) => SignatureMatches(original, methodInfo));

                if (method != null) {
                    return method;
                }

                baseClass = baseClass.BaseType;
            }

            return original;
        }

        private static Delegate GetClosedDelegate(MethodInfo methodInfo) {
            for (int i = 0; i < staticDelegates.Count; i++) {
                DelegateEntry entry = staticDelegates[i];
                if (entry.methodInfo == methodInfo) {
                    return entry.instance;
                }
            }

            Type delegateType = GetClosedDelegateType(methodInfo);
            Delegate instance = Delegate.CreateDelegate(delegateType, methodInfo, true);
            DelegateEntry newEntry = new DelegateEntry(instance, methodInfo);
            staticDelegates.Add(newEntry);
            return instance;
        }

        private static Delegate GetClosedDelegate(Type delegateType, MethodInfo methodInfo) {
            for (int i = 0; i < staticDelegates.Count; i++) {
                DelegateEntry entry = staticDelegates[i];
                if (entry.methodInfo == methodInfo) {
                    return entry.instance;
                }
            }

            Delegate instance = Delegate.CreateDelegate(delegateType, methodInfo, true);
            DelegateEntry newEntry = new DelegateEntry(instance, methodInfo);
            staticDelegates.Add(newEntry);
            return instance;
        }

        private static Delegate GetOpenDelegate(MethodInfo methodInfo) {
            for (int i = 0; i < openDelegates.Count; i++) {
                DelegateEntry entry = openDelegates[i];
                if (entry.methodInfo == methodInfo) {
                    return entry.instance;
                }
            }

            Type delegateType = GetOpenDelegateType(methodInfo);
            Delegate openDelegate = CreateOpenDelegate(delegateType, methodInfo);
            DelegateEntry openEntry = new DelegateEntry(openDelegate, methodInfo);
            openDelegates.Add(openEntry);

            return openDelegate;
        }

        private static Delegate GetOpenDelegate(Type delegateType, MethodInfo methodInfo) {
            for (int i = 0; i < openDelegates.Count; i++) {
                DelegateEntry entry = openDelegates[i];
                if (entry.methodInfo == methodInfo) {
                    return entry.instance;
                }
            }

            Delegate openDelegate = CreateOpenDelegate(delegateType, methodInfo);
            DelegateEntry openEntry = new DelegateEntry(openDelegate, methodInfo);
            openDelegates.Add(openEntry);

            return openDelegate;
        }

        public static Delegate GetDelegate(MethodInfo methodInfo) {
            methodInfo = ResolvePossibleBaseClassMethod(methodInfo.DeclaringType, methodInfo);
            methodInfo = ResolvePossibleInterface(methodInfo.DeclaringType, methodInfo);
            return methodInfo.IsStatic ? GetClosedDelegate(methodInfo) : GetOpenDelegate(methodInfo);
        }

        public static Delegate GetDelegate(Type delegateType, MethodInfo methodInfo) {
            methodInfo = ResolvePossibleInterface(methodInfo.DeclaringType, methodInfo);
            return methodInfo.IsStatic ? GetClosedDelegate(delegateType, methodInfo) : GetOpenDelegate(delegateType, methodInfo);
        }

        private static Delegate CreateFieldGetter(Type declaredType, string fieldName) {
            ParameterExpression paramExpression = Expression.Parameter(declaredType, "value");
            Expression fieldGetterExpression = Expression.Field(paramExpression, fieldName);
            return Expression.Lambda(fieldGetterExpression, paramExpression).Compile();
        }

        private static Delegate CreateFieldSetter(Type baseType, Type fieldType, string fieldName) {
            FieldInfo fieldInfo = baseType.GetField(fieldName, InstanceBindFlags);

            if (fieldInfo == null) {
                PropertyInfo propertyInfo = baseType.GetProperty(fieldName);
                if (propertyInfo == null || !propertyInfo.CanWrite) {
                    return null;
                }

                return null;
            }

            if (fieldInfo.IsInitOnly) {
                return null;
            }

            ParameterExpression paramExpression0 = Expression.Parameter(baseType);
            ParameterExpression paramExpression1 = Expression.Parameter(fieldType, fieldName);
            MemberExpression fieldGetter = Expression.Field(paramExpression0, fieldName);

            try {
                return Expression.Lambda(
                    Expression.Assign(fieldGetter, paramExpression1),
                    paramExpression0,
                    paramExpression1
                ).Compile();
            }
            catch (ArgumentException) {
                Debug.Log($"baseType: {baseType}, fieldType: {fieldType}, fieldName: {fieldName}");
                throw;
            }
        }

        public struct LinqAccessor {

            public readonly string fieldName;
            public readonly Delegate setter;
            public readonly Delegate getter;

            public LinqAccessor(string fieldName, Delegate getter, Delegate setter) {
                this.fieldName = fieldName;
                this.getter = getter;
                this.setter = setter;
            }

        }

        public static LinqAccessor GetLinqPropertyAccessors(Type baseType, PropertyInfo propertyInfo) {
            return GetLinqPropertyAccessors(baseType, propertyInfo.PropertyType, propertyInfo.Name);
        }

        public static LinqAccessor GetLinqPropertyAccessors(Type baseType, Type propertyType, string propertyName) {
            Delegate getter = CreatePropertyGetter(baseType, propertyName);
            Delegate setter = CreatePropertySetter(baseType, propertyType, propertyName);
            LinqAccessor linqEntry = new LinqAccessor(propertyName, getter, setter);

            return linqEntry;
        }

        public static LinqAccessor GetLinqFieldAccessors(Type baseType, FieldInfo fieldInfo) {
            return GetLinqFieldAccessors(baseType, fieldInfo.FieldType, fieldInfo.Name);
        }

        public static LinqAccessor GetLinqFieldAccessors(Type baseType, Type fieldType, string fieldName) {
            Delegate getter = CreateFieldGetter(baseType, fieldName);
            Delegate setter = CreateFieldSetter(baseType, fieldType, fieldName);
            LinqAccessor linqEntry = new LinqAccessor(fieldName, getter, setter);

            return linqEntry;
        }

        public static Delegate CreatePropertySetter(Type objectType, Type propertyType, string propertyName) {
            try {
                ParameterExpression paramExpression0 = Expression.Parameter(objectType);
                ParameterExpression paramExpression1 = Expression.Parameter(propertyType, propertyName);
                MemberExpression propertyGetterExpression = Expression.Property(paramExpression0, propertyName);

                return Expression.Lambda(
                    Expression.Assign(propertyGetterExpression, paramExpression1),
                    paramExpression0,
                    paramExpression1
                ).Compile();
            }
            catch (Exception) {
                return null;
            }
        }

        public static Delegate CreateStaticPropertyGetter(Type objectType, string propertyName) {
            Expression propertyGetterExpression = Expression.Property(null, objectType, propertyName);
            return Expression.Lambda(propertyGetterExpression, null).Compile();
        }

        public static Delegate CreateStaticFieldGetter(Type objectType, string propertyName) {
            Expression propertyGetterExpression = Expression.Field(null, objectType, propertyName);
            return Expression.Lambda(propertyGetterExpression, null).Compile();
        }

        public static Delegate CreatePropertyGetter(Type objectType, string propertyName) {
            ParameterExpression paramExpression = Expression.Parameter(objectType, "value");
            Expression propertyGetterExpression = Expression.Property(paramExpression, propertyName);
            return Expression.Lambda(propertyGetterExpression, paramExpression).Compile();
        }

        public static Delegate CreateArraySetter(Type arrayType) {
            ParameterExpression arrayExpr = Expression.Parameter(arrayType, "array");
            ParameterExpression indexExpr = Expression.Parameter(typeof(int), "idx");
            ParameterExpression valueExpr = Expression.Parameter(arrayType.GetElementType(), "value");

            Expression arrayAccess = Expression.ArrayAccess(arrayExpr, indexExpr);

            return Expression.Lambda(
                Expression.Assign(arrayAccess, valueExpr),
                arrayExpr,
                indexExpr,
                valueExpr
            ).Compile();
        }

        public static Delegate CreateIndexGetter(Type type) {
            PropertyInfo info = type.GetProperty("Item");

            if (info == null) {
                return null;
            }

            ParameterInfo[] parameters = info.GetIndexParameters();
            if (parameters.Length != 1) {
                return null;
            }

            ParameterExpression targetExpr = Expression.Parameter(type);
            ParameterExpression keyExpr = Expression.Parameter(parameters[0].ParameterType);

            IndexExpression indexExpr = Expression.Property(targetExpr, info, keyExpr);
            return Expression.Lambda(indexExpr, targetExpr, keyExpr).Compile();
        }

        public static Delegate CreateIndexSetter(Type type) {
            PropertyInfo info = type.GetProperty("Item");

            if (info == null) {
                return null;
            }

            ParameterInfo[] parameters = info.GetIndexParameters();
            if (parameters.Length != 1) {
                return null;
            }

            ParameterExpression targetExpr = Expression.Parameter(type);
            ParameterExpression keyExpr = Expression.Parameter(parameters[0].ParameterType);
            ParameterExpression valueExpr = Expression.Parameter(info.PropertyType);

            IndexExpression indexExpr = Expression.Property(targetExpr, info, keyExpr);
            BinaryExpression assign = Expression.Assign(indexExpr, valueExpr);
            return Expression.Lambda(assign, targetExpr, keyExpr, valueExpr).Compile();
        }

        public static Delegate CreateArrayGetter(Type arrayType) {
            ParameterExpression arrayExpr = Expression.Parameter(arrayType, "array");
            ParameterExpression indexExpr = Expression.Parameter(typeof(int), "idx");

            Expression arrayAccess = Expression.ArrayAccess(arrayExpr, indexExpr);

            return Expression.Lambda(
                arrayAccess,
                arrayExpr,
                indexExpr
            ).Compile();
        }

        // todo -- need some parameter matching at least
        public static MethodInfo GetMethodInfo(Type type, string methodName) {
            return type.GetMethod(methodName, StaticFlags | InstanceBindFlags);
        }

        public static bool IsCallbackType(Type type) {
            if (type == typeof(Action)) return true;
            Type generic = null;
            if (type.IsGenericTypeDefinition) {
                generic = type;
            }
            else if (type.IsGenericType) {
                generic = type.GetGenericTypeDefinition();
            }

            if (generic == null) return false;
            if (generic == typeof(Action<>)) return true;
            if (generic == typeof(Action<,>)) return true;
            if (generic == typeof(Action<,,>)) return true;
            if (generic == typeof(Action<,,,>)) return true;
            if (generic == typeof(Action<,,,,>)) return true;
            if (generic == typeof(Action<,,,,,>)) return true;
            if (generic == typeof(Action<,,,,,,>)) return true;
            if (generic == typeof(Func<>)) return true;
            if (generic == typeof(Func<,>)) return true;
            if (generic == typeof(Func<,,>)) return true;
            if (generic == typeof(Func<,,,>)) return true;
            if (generic == typeof(Func<,,,,>)) return true;
            if (generic == typeof(Func<,,,,,>)) return true;
            if (generic == typeof(Func<,,,,,,>)) return true;
            if (generic == typeof(Func<,,,,,,,>)) return true;
            return false;
        }

        public static bool IsPropertyStatic(PropertyInfo propertyInfo) {
            return propertyInfo.GetMethod?.IsStatic ?? false;
        }

        public static bool IsPropertyReadOnly(PropertyInfo propertyInfo) {
            return propertyInfo.SetMethod == null;
        }

        public static bool IsAction(Type type) {
            if (type == typeof(Action)) {
                return true;
            }

            Type generic = null;
            if (type.IsGenericTypeDefinition) {
                generic = type;
            }
            else if (type.IsGenericType) {
                generic = type.GetGenericTypeDefinition();
            }

            if (generic == typeof(Action<>)) return true;
            if (generic == typeof(Action<,>)) return true;
            if (generic == typeof(Action<,,>)) return true;
            if (generic == typeof(Action<,,,>)) return true;
            if (generic == typeof(Action<,,,,>)) return true;
            if (generic == typeof(Action<,,,,,>)) return true;
            if (generic == typeof(Action<,,,,,,>)) return true;
            return false;
        }

        public static bool IsFunc(Type type) {
            if (!type.IsGenericType) return false;

            Type generic = null;
            if (type.IsGenericTypeDefinition) {
                generic = type;
            }
            else if (type.IsGenericType) {
                generic = type.GetGenericTypeDefinition();
            }

            if (generic == typeof(Func<>)) return true;
            if (generic == typeof(Func<,>)) return true;
            if (generic == typeof(Func<,,>)) return true;
            if (generic == typeof(Func<,,,>)) return true;
            if (generic == typeof(Func<,,,,>)) return true;
            if (generic == typeof(Func<,,,,,>)) return true;
            if (generic == typeof(Func<,,,,,,>)) return true;
            return false;
        }

//        public static MethodInfo GetImplicitConversion(Type targetType, Type inputType) {
//            LightList<MethodInfo> infos = LightList<MethodInfo>.Get();
//            GetStaticMethods(targetType, infos);
//            for (int i = 0; i < infos.size; i++) {
//                if (!infos.array[i].IsStatic || !infos.array[i].IsPublic) {
//                    continue;
//                }
//
//                if (infos[i].Name == "op_Implicit" && infos[i].ReturnType == targetType) {
//                    ParameterInfo pi = infos[i].GetParameters().FirstOrDefault();
//                    if (pi != null && pi.ParameterType == inputType) {
//                        LightList<MethodInfo>.Release(ref infos);
//                        return infos[i];
//                    }
//                }
//            }
//
//            LightList<MethodInfo>.Release(ref infos);
//
//            return null;
//        }

//        public static MethodInfo GetBinaryOperator(string opName, Type leftType, Type rightType) {
//            MethodInfo[] infos = leftType.GetMethods(BindingFlags.Public | BindingFlags.Static);
//            for (int i = 0; i < infos.Length; i++) {
//                if (infos[i].Name == opName && infos[i].ReturnType == leftType) {
//                    ParameterInfo[] pi = infos[i].GetParameters();
//                    if (pi.Length != 2) {
//                        continue;
//                    }
//
//                    if (pi[0].ParameterType == leftType && pi[1].ParameterType == rightType) {
//                        return infos[i];
//                    }
//                }
//            }
//
//            return null;
//        }
//
//        // todo don't require bool return type
//        public static MethodInfo GetComparisonOperator(string opName, Type leftType, Type rightType) {
//            MethodInfo[] infos = leftType.GetMethods(BindingFlags.Public | BindingFlags.Static);
//            for (int i = 0; i < infos.Length; i++) {
//                if (infos[i].Name == opName && infos[i].ReturnType == typeof(bool)) {
//                    ParameterInfo[] pi = infos[i].GetParameters();
//                    if (pi.Length != 2) {
//                        continue;
//                    }
//
//                    if (pi[0].ParameterType == leftType && pi[1].ParameterType == rightType) {
//                        return infos[i];
//                    }
//                }
//            }
//
//            return null;
//        }
//
//        public static MethodInfo GetUnaryOperator(string opName, Type type) {
//            MethodInfo[] infos = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
//            for (int i = 0; i < infos.Length; i++) {
//                if (infos[i].Name == opName) {
//                    ParameterInfo[] pi = infos[i].GetParameters();
//                    if (pi.Length != 1) {
//                        continue;
//                    }
//
//                    if (pi[0].ParameterType == type) {
//                        return infos[i];
//                    }
//                }
//            }
//
//            return null;
//        }

//        public static List<MethodInfo> GetMethodsWithName(Type type, string targetName) {
//            MethodInfo[] infos = type.GetMethods(InstanceBindFlags | StaticFlags);
//            List<MethodInfo> retn = new List<MethodInfo>();
//            for (int i = 0; i < infos.Length; i++) {
//                if (infos[i].Name == targetName) {
//                    retn.Add(infos[i]);
//                }
//            }
//
//            return retn;
//        }

        public static void GetPublicInstanceMethodsWithName(Type type, string targetName, LightList<MethodInfo> retn) {
            MethodInfo[] infos = GetInstanceMethods(type);

            for (int i = 0; i < infos.Length; i++) {
                if (string.Equals(infos[i].Name, targetName)) {
                    retn.Add(infos[i]);
                }
            }
        }

        public static bool IsConstantField(Type rootType, string fieldName) {
            FieldInfo info = rootType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return info != null && info.IsLiteral && !info.IsInitOnly;
        }

        public static FieldInfo GetConstantField(Type rootType, string fieldName) {
            return rootType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        }


        // public static Type CreateType(string id, Type baseType, IList<FieldDefinition> fields, IList<MethodDefinition> methods, IList<string> namespaces) {
        //     if (classBuilder == null) classBuilder = new ClassBuilder();
        //     return classBuilder.CreateRuntimeType(id, baseType, fields, methods, namespaces);
        // }
        //
        // public static Type CreateGenericRuntimeType(string id, Type baseType, GenericTypeDefinition[] genericsArgs, IList<FieldDefinition> fields, IList<string> namespaces) {
        //     if (classBuilder == null) classBuilder = new ClassBuilder();
        //     return classBuilder.CreateGenericRuntimeType(id, baseType, genericsArgs, fields, namespaces);
        // }
        //
        // public static bool TryCreateInstance<T>(string id, out T instance) {
        //     if (classBuilder == null) {
        //         instance = default;
        //         return false;
        //     }
        //
        //     return classBuilder.TryCreateInstance(id, out instance);
        // }

        public struct FieldDefinition {

            public bool isStatic;
            public readonly string fieldName;
            public readonly TypeLookup fieldType;

            public FieldDefinition(TypeLookup fieldType, string fieldName, bool isStatic) {
                this.fieldType = fieldType;
                this.fieldName = fieldName;
                this.isStatic = isStatic;
            }

        }

        public struct MethodDefinition {

            public string methodName;
            public TypeLookup returnType;
            public LambdaArgument[] arguments;
            public BlockNode body;
            public bool isStatic;

            public MethodDefinition(TypeLookup returnType, string methodName, LambdaArgument[] arguments, BlockNode body, bool isStatic) {
                this.returnType = returnType;
                this.methodName = methodName;
                this.arguments = arguments;
                this.body = body;
                this.isStatic = isStatic;
            }

        }

        public struct GenericTypeDefinition {

            public string name;
            public GenericParameterAttributes restrictions;
            public Type[] interfaceTypes;

        }

        private static int typeIdGenerator = 0;

        public static string GetGeneratedTypeName(string name) {
            return name + typeIdGenerator;
        }

        public static MemberInfo GetFieldOrProperty(Type type, string memberName) {
            if (IsField(type, memberName, out FieldInfo fieldInfo)) {
                return fieldInfo;
            }

            if (IsProperty(type, memberName, out PropertyInfo propertyInfo)) {
                return propertyInfo;
            }

            Type[] interfaces = type.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++) {
                if (IsProperty(interfaces[i], memberName, out propertyInfo)) {
                    return propertyInfo;
                }
            }

            return null;
        }

        public static PropertyInfo GetIndexedPropertyWithSignature(Type lastValueType, Type indexType) {
            TypeArray1[0] = indexType;
            return GetIndexedPropertyWithSignature(lastValueType, TypeArray1);
        }

        public static PropertyInfo GetIndexedPropertyWithSignature(Type targetType, Type[] indexTypes) {
            PropertyInfo[] properties = targetType.GetProperties(InstanceBindFlags);

            for (int i = 0; i < properties.Length; i++) {
                PropertyInfo p = properties[i];
                ParameterInfo[] indexParameters = p.GetIndexParameters();
                if (indexParameters.Length != indexTypes.Length) {
                    continue;
                }

                bool matches = true;

                for (int j = 0; j < indexParameters.Length; j++) {
                    if (indexParameters[j].ParameterType != indexTypes[j]) {
                        matches = false;
                        break;
                    }
                }

                if (matches) {
                    return properties[i];
                }
            }

            return null;
        }

        public static Type GetNullableType(Type type) {
            // Use Nullable.GetUnderlyingType() to remove the Nullable<T> wrapper if type is already nullable.
            type = Nullable.GetUnderlyingType(type) ?? type; // avoid type becoming null
            if (type.IsValueType) {
                return typeof(Nullable<>).MakeGenericType(type);
            }
            else {
                return type;
            }
        }

        public struct IndexerInfo {

            public PropertyInfo propertyInfo;
            public ParameterInfo[] parameterInfos;

        }

        public static IList<IndexerInfo> GetIndexedProperties(Type targetType, IList<IndexerInfo> retn = null) {
            if (retn == null) retn = new List<IndexerInfo>();
            PropertyInfo[] properties = targetType.GetProperties(InstanceBindFlags);

            for (int i = 0; i < properties.Length; i++) {
                PropertyInfo p = properties[i];
                ParameterInfo[] indexParameters = p.GetIndexParameters();
                if (indexParameters.Length > 0) {
                    retn.Add(new IndexerInfo() {
                        propertyInfo = p,
                        parameterInfos = indexParameters
                    });
                }
            }

            return retn;
        }

        public static ConstructorInfo GetConstructor(Type type, IReadOnlyList<Type> argTypes) {
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            for (int i = 0; i < constructors.Length; i++) {
                ParameterInfo[] parameters = constructors[i].GetParameters();

                if (parameters.Length > argTypes.Count) {
                    int start = argTypes.Count;
                }
                else if (argTypes.Count == parameters.Length) {
                    if (MatchesSignature(argTypes, parameters)) {
                        return constructors[i];
                    }
                }
            }

            return null;
        }

        private static bool MatchesSignature(IReadOnlyList<Type> argTypes, ParameterInfo[] parameters) {
            for (int j = 0; j < parameters.Length; j++) {
                Type argType = argTypes[j];
                Type paramType = parameters[j].ParameterType;

                if (paramType != argType) {
                    if (!TypeUtil.HasIdentityPrimitiveOrNullableConversion(argType, paramType) && !TypeUtil.HasReferenceConversion(argType, paramType)) {
                        return false;
                    }
                }
            }

            return true;
        }

        public static MemberInfo GetStaticOrConstMemberInfo(Type type, string fieldOrPropertyName) {
            FieldInfo fieldInfo = type.GetField(fieldOrPropertyName, PublicStatic | BindingFlags.FlattenHierarchy);

            if (fieldInfo != null) {
                return fieldInfo;
            }

            PropertyInfo propertyInfo = type.GetProperty(fieldOrPropertyName, PublicStatic | BindingFlags.FlattenHierarchy);
            if (propertyInfo != null) {
                return propertyInfo;
            }

            return null;
        }

        public static bool HasConstOrStaticMember(Type type, string fieldOrPropertyName, out MemberInfo memberInfo) {
            FieldInfo fieldInfo = type.GetField(fieldOrPropertyName, PublicStatic | BindingFlags.FlattenHierarchy);

            if (fieldInfo != null) {
                memberInfo = fieldInfo;
                return true;
            }

            PropertyInfo propertyInfo = type.GetProperty(fieldOrPropertyName, PublicStatic | BindingFlags.FlattenHierarchy);
            if (propertyInfo != null) {
                memberInfo = propertyInfo;
                return true;
            }

            memberInfo = null;
            return false;
        }

        public static void GetGenericArgs(Type type, LightList<string> output) {
            if (!type.IsGenericType) return;
            if (type.IsConstructedGenericType) {
                Type[] args = type.GetGenericArguments();
                for (int i = 0; i < args.Length; i++) {
                    output.Add(args[i].Name);
                    GetGenericArgs(args[i], output);
                }
            }
        }

        public static void GetGenericArgs(Type type, LightList<Type> output) {
            if (!type.IsGenericType) return;
            if (type.IsConstructedGenericType) {
                Type[] args = type.GetGenericArguments();
                output.AddRange(args);
                for (int i = 0; i < args.Length; i++) {
                    GetGenericArgs(args[i], output);
                }
            }
        }

    }

    public struct ConstructorArguments {

        public readonly object arg0;
        public readonly object arg1;
        public readonly object arg2;
        public readonly object arg3;
        public readonly object arg4;
        public int count;

        public ConstructorArguments(object arg0) {
            this.arg0 = arg0;
            this.arg1 = null;
            this.arg2 = null;
            this.arg3 = null;
            this.arg4 = null;
            count = 1;
        }

        public ConstructorArguments(object arg0, object arg1) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = null;
            this.arg3 = null;
            this.arg4 = null;
            count = 2;
        }

        public ConstructorArguments(object arg0, object arg1, object arg2) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = null;
            this.arg4 = null;
            count = 3;
        }

        public ConstructorArguments(object arg0, object arg1, object arg2, object arg3) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = null;
            count = 4;
        }

        public ConstructorArguments(object arg0, object arg1, object arg2, object arg3, object arg4) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            count = 5;
        }

        public object[] GetArguments() {
            switch (count) {
                case 0:
                    return ReflectionUtil.ObjectArray0;
                case 1:
                    ReflectionUtil.ObjectArray1[0] = arg0;
                    return ReflectionUtil.ObjectArray1;
                case 2:
                    ReflectionUtil.ObjectArray2[0] = arg0;
                    ReflectionUtil.ObjectArray2[1] = arg1;
                    return ReflectionUtil.ObjectArray2;
                case 3:
                    ReflectionUtil.ObjectArray3[0] = arg0;
                    ReflectionUtil.ObjectArray3[1] = arg1;
                    ReflectionUtil.ObjectArray3[2] = arg2;
                    return ReflectionUtil.ObjectArray3;
                case 4:
                    ReflectionUtil.ObjectArray4[0] = arg0;
                    ReflectionUtil.ObjectArray4[1] = arg1;
                    ReflectionUtil.ObjectArray4[2] = arg2;
                    ReflectionUtil.ObjectArray4[3] = arg3;
                    return ReflectionUtil.ObjectArray4;
                case 5:
                    ReflectionUtil.ObjectArray5[0] = arg0;
                    ReflectionUtil.ObjectArray5[1] = arg1;
                    ReflectionUtil.ObjectArray5[2] = arg2;
                    ReflectionUtil.ObjectArray5[3] = arg3;
                    ReflectionUtil.ObjectArray5[4] = arg4;
                    return ReflectionUtil.ObjectArray5;
            }

            return null;
        }

        public static implicit operator object[](ConstructorArguments arguments) {
            return arguments.GetArguments();
        }

    }

    public struct GenericArguments {

        public readonly Type arg0;
        public readonly Type arg1;
        public readonly Type arg2;
        public readonly Type arg3;
        public int count;

        public GenericArguments(Type arg0) {
            this.arg0 = arg0;
            this.arg1 = null;
            this.arg2 = null;
            this.arg3 = null;
            count = 1;
        }

        public GenericArguments(Type arg0, Type arg1) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = null;
            this.arg3 = null;
            count = 2;
        }

        public GenericArguments(Type arg0, Type arg1, Type arg2) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = null;
            count = 3;
        }

        public GenericArguments(Type arg0, Type arg1, Type arg2, Type arg3) {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            count = 4;
        }

        public Type[] GetArguments() {
            switch (count) {
                case 1:
                    ReflectionUtil.TypeArray1[0] = arg0;
                    return ReflectionUtil.TypeArray1;
                case 2:
                    ReflectionUtil.TypeArray2[0] = arg0;
                    ReflectionUtil.TypeArray2[1] = arg1;
                    return ReflectionUtil.TypeArray2;
                case 3:
                    ReflectionUtil.TypeArray3[0] = arg0;
                    ReflectionUtil.TypeArray3[1] = arg1;
                    ReflectionUtil.TypeArray3[2] = arg2;
                    return ReflectionUtil.TypeArray3;
                case 4:
                    ReflectionUtil.TypeArray4[0] = arg0;
                    ReflectionUtil.TypeArray4[1] = arg1;
                    ReflectionUtil.TypeArray4[2] = arg2;
                    ReflectionUtil.TypeArray4[3] = arg3;
                    return ReflectionUtil.TypeArray4;
            }

            return null;
        }

        public static implicit operator Type[](GenericArguments arguments) {
            return arguments.GetArguments();
        }

    }

}