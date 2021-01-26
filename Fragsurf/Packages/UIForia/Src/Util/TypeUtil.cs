using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UIForia.Extensions;
using UnityEngine;

namespace UIForia.Util {

    public static class TypeUtil {

        private static readonly Assembly _mscorlib = typeof(object).Assembly;
        private static readonly Assembly _systemCore = typeof(Expression).Assembly;
        private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        internal const MethodAttributes PublicStatic = MethodAttributes.Public | MethodAttributes.Static;

        internal static Type GetNonNullableType(this Type type) {
            if (type.IsNullableType())
                return type.GetGenericArguments()[0];
            return type;
        }

        internal static Type GetNullableType(Type type) {
            if (!type.IsValueType || type.IsNullableType())
                return type;
            return typeof(Nullable<>).MakeGenericType(type);
        }

        internal static bool IsNullableType(this Type type) {
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition() == typeof(Nullable<>);
            return false;
        }

        internal static bool IsBool(Type type) {
            return type.GetNonNullableType() == typeof(bool);
        }

        internal static bool IsNumeric(Type type) {
            type = type.GetNonNullableType();
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                }
            }

            return false;
        }

        internal static bool IsInteger(Type type) {
            type = type.GetNonNullableType();
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type)) {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsArithmetic(Type type) {
            type = type.GetNonNullableType();
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        return true;
                }
            }

            return false;
        }

        internal static bool IsUnsignedInt(Type type) {
            type = type.GetNonNullableType();
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }

            return false;
        }

        internal static bool IsIntegerOrBool(Type type) {
            type = type.GetNonNullableType();
            if (!type.IsEnum) {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return true;
                }
            }

            return false;
        }

        internal static bool AreEquivalent(Type t1, Type t2) {
            if (!(t1 == t2))
                return t1.IsEquivalentTo(t2);
            return true;
        }

        internal static bool AreReferenceAssignable(Type dest, Type src) {
            return AreEquivalent(dest, src) || !dest.IsValueType && !src.IsValueType && dest.IsAssignableFrom(src);
        }

        internal static bool IsValidInstanceType(MemberInfo member, Type instanceType) {
            Type declaringType = member.DeclaringType;
            if (AreReferenceAssignable(declaringType, instanceType))
                return true;
            if (instanceType.IsValueType) {
                if (AreReferenceAssignable(declaringType, typeof(object)) || AreReferenceAssignable(declaringType, typeof(ValueType)) || instanceType.IsEnum && AreReferenceAssignable(declaringType, typeof(Enum)))
                    return true;
                if (declaringType.IsInterface) {
                    foreach (Type src in instanceType.GetInterfaces()) {
                        if (AreReferenceAssignable(declaringType, src))
                            return true;
                    }
                }
            }

            return false;
        }

        internal static bool HasIdentityPrimitiveOrNullableConversion(Type source, Type dest) {
            return AreEquivalent(source, dest) || source.IsNullableType() && AreEquivalent(dest, source.GetNonNullableType()) || dest.IsNullableType() && AreEquivalent(source, dest.GetNonNullableType()) || IsConvertible(source) && IsConvertible(dest) && dest.GetNonNullableType() != typeof(bool);
        }

        internal static bool HasReferenceConversion(Type source, Type dest) {
            if (source == typeof(void) || dest == typeof(void))
                return false;
            Type nonNullableType1 = source.GetNonNullableType();
            Type nonNullableType2 = dest.GetNonNullableType();
            return nonNullableType1.IsAssignableFrom(nonNullableType2) || nonNullableType2.IsAssignableFrom(nonNullableType1) || (source.IsInterface || dest.IsInterface) || (IsLegalExplicitVariantDelegateConversion(source, dest) || source == typeof(object) || dest == typeof(object));
        }

        private static bool IsCovariant(Type t) {
            return (uint) (t.GenericParameterAttributes & GenericParameterAttributes.Covariant) > 0U;
        }

        private static bool IsContravariant(Type t) {
            return (uint) (t.GenericParameterAttributes & GenericParameterAttributes.Contravariant) > 0U;
        }

        private static bool IsInvariant(Type t) {
            return (t.GenericParameterAttributes & GenericParameterAttributes.VarianceMask) == GenericParameterAttributes.None;
        }

        private static bool IsDelegate(Type t) {
            return t.IsSubclassOf(typeof(MulticastDelegate));
        }

        internal static bool IsLegalExplicitVariantDelegateConversion(Type source, Type dest) {
            if (!IsDelegate(source) || !IsDelegate(dest) || (!source.IsGenericType || !dest.IsGenericType))
                return false;
            Type genericTypeDefinition = source.GetGenericTypeDefinition();
            if (dest.GetGenericTypeDefinition() != genericTypeDefinition)
                return false;
            Type[] genericArguments1 = genericTypeDefinition.GetGenericArguments();
            Type[] genericArguments2 = source.GetGenericArguments();
            Type[] genericArguments3 = dest.GetGenericArguments();
            for (int index = 0; index < genericArguments1.Length; ++index) {
                Type type1 = genericArguments2[index];
                Type type2 = genericArguments3[index];
                if (!AreEquivalent(type1, type2)) {
                    Type t = genericArguments1[index];
                    if (IsInvariant(t))
                        return false;
                    if (IsCovariant(t)) {
                        if (!HasReferenceConversion(type1, type2))
                            return false;
                    }
                    else if (IsContravariant(t) && (type1.IsValueType || type2.IsValueType))
                        return false;
                }
            }

            return true;
        }

        internal static bool IsConvertible(Type type) {
            type = type.GetNonNullableType();
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return type.IsEnum;
            }
        }

        internal static bool HasReferenceEquality(Type left, Type right) {
            if (left.IsValueType || right.IsValueType)
                return false;
            if (!left.IsInterface && !right.IsInterface && !AreReferenceAssignable(left, right))
                return AreReferenceAssignable(right, left);
            return true;
        }

        internal static bool HasBuiltInEqualityOperator(Type left, Type right) {
            if (left.IsInterface && !right.IsValueType || right.IsInterface && !left.IsValueType || !left.IsValueType && !right.IsValueType && (AreReferenceAssignable(left, right) || AreReferenceAssignable(right, left)))
                return true;
            if (!AreEquivalent(left, right))
                return false;
            Type nonNullableType = left.GetNonNullableType();
            return nonNullableType == typeof(bool) || IsNumeric(nonNullableType) || nonNullableType.IsEnum;
        }

        internal static bool IsImplicitlyConvertible(Type source, Type destination) {
            if (!AreEquivalent(source, destination) && !IsImplicitNumericConversion(source, destination) && (!IsImplicitReferenceConversion(source, destination) && !IsImplicitBoxingConversion(source, destination)))
                return IsImplicitNullableConversion(source, destination);
            return true;
        }

        internal static MethodInfo GetUserDefinedCoercionMethod(Type convertFrom, Type convertToType, bool implicitOnly) {
            Type nonNullableType1 = convertFrom.GetNonNullableType();
            Type nonNullableType2 = convertToType.GetNonNullableType();
            
            MethodInfo[] methodInfos1 = ReflectionUtil.GetStaticMethods(nonNullableType1);
            MethodInfo conversionOperator1 = FindConversionOperator(methodInfos1, convertFrom, convertToType, implicitOnly);
            
            if (conversionOperator1 != null) {
                return conversionOperator1;
            }

            MethodInfo[] methodInfos2 = ReflectionUtil.GetStaticMethods(nonNullableType2);
            MethodInfo conversionOperator2 = FindConversionOperator(methodInfos2, convertFrom, convertToType, implicitOnly);

            if (conversionOperator2 != null) {
                return conversionOperator2;
            }

            if (!AreEquivalent(nonNullableType1, convertFrom) || !AreEquivalent(nonNullableType2, convertToType)) {
                MethodInfo conversionOperator3 = FindConversionOperator(methodInfos1, nonNullableType1, nonNullableType2, implicitOnly);

                if (conversionOperator3 == null) {
                    conversionOperator3 = FindConversionOperator(methodInfos2, nonNullableType1, nonNullableType2, implicitOnly);
                }

                return conversionOperator3;
            }

            return null;
        }

        internal static bool TryGetUserDefinedCoercionMethod(Type convertFrom, Type convertToType, bool implicitOnly, out MethodInfo info) {

            Type nonNullableType1 = convertFrom.GetNonNullableType();
            Type nonNullableType2 = convertToType.GetNonNullableType();
            MethodInfo[] methodInfos1 = ReflectionUtil.GetStaticMethods(nonNullableType1);

            MethodInfo conversionOperator1 = FindConversionOperator(methodInfos1, convertFrom, convertToType, implicitOnly);

            if (conversionOperator1 != null) {
                info = conversionOperator1;
                return true;
            }

            MethodInfo[] methodInfos2 = ReflectionUtil.GetStaticMethods(nonNullableType2);
            MethodInfo conversionOperator2 = FindConversionOperator(methodInfos2, convertFrom, convertToType, implicitOnly);
            
            if (conversionOperator2 != null) {
                info = conversionOperator2;
                return true;
            }

            if (!AreEquivalent(nonNullableType1, convertFrom) || !AreEquivalent(nonNullableType2, convertToType)) {
                MethodInfo conversionOperator3 = FindConversionOperator(methodInfos1, nonNullableType1, nonNullableType2, implicitOnly);
                if (conversionOperator3 == null) {
                    conversionOperator3 = FindConversionOperator(methodInfos2, nonNullableType1, nonNullableType2, implicitOnly);
                }
                
                if (conversionOperator3 != null) {
                    info = conversionOperator3;
                    return true;
                }
            }

            info = null;
            return false;
        }

        internal static MethodInfo FindConversionOperator(MethodInfo[] methods, Type typeFrom, Type typeTo, bool implicitOnly) {
            for (int i = 0; i < methods.Length; i++) {
                MethodInfo method = methods[i];
                
                string name = method.Name;
                
                if(name.Length != 11) continue;

                if (name[0] != 'o' && name[1] != 'p' && name[3] != '_') {
                    continue;
                }
                
                if ((string.Equals(name, "op_Implicit") || !implicitOnly && string.Equals(name, "op_Explicit"))) {
                    
                    if((AreEquivalent(method.ReturnType, typeTo))) {

                        if (AreEquivalent(method.GetParameters()[0].ParameterType, typeFrom)) {
                            return method;
                        }
                        
                    }
                    
                }
            }

            return null;
        }

        private static bool IsImplicitNumericConversion(Type source, Type destination) {
            TypeCode typeCode1 = Type.GetTypeCode(source);
            TypeCode typeCode2 = Type.GetTypeCode(destination);
            switch (typeCode1) {
                case TypeCode.Char:
                    switch (typeCode2) {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }

                case TypeCode.SByte:
                    switch (typeCode2) {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }

                case TypeCode.Byte:
                    switch (typeCode2) {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }

                case TypeCode.Int16:
                    switch (typeCode2) {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }

                case TypeCode.UInt16:
                    switch (typeCode2) {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }

                case TypeCode.Int32:
                    switch (typeCode2) {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }

                case TypeCode.UInt32:
                    switch (typeCode2) {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (typeCode2) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            return false;
                    }

                case TypeCode.Single:
                    return typeCode2 == TypeCode.Double;
                default:
                    return false;
            }
        }

        private static bool IsImplicitReferenceConversion(Type source, Type destination) {
            return destination.IsAssignableFrom(source);
        }

        private static bool IsImplicitBoxingConversion(Type source, Type destination) {
            return source.IsValueType && (destination == typeof(object) || destination == typeof(ValueType)) || source.IsEnum && destination == typeof(Enum);
        }

        private static bool IsImplicitNullableConversion(Type source, Type destination) {
            if (destination.IsNullableType())
                return IsImplicitlyConvertible(source.GetNonNullableType(), destination.GetNonNullableType());
            return false;
        }

        internal static bool IsSameOrSubclass(Type type, Type subType) {
            if (!AreEquivalent(type, subType))
                return subType.IsSubclassOf(type);
            return true;
        }

        internal static void ValidateType(Type type) {
            if (type.IsGenericTypeDefinition)
                throw new Exception("Type is generic " + type);
            if (type.ContainsGenericParameters)
                throw new Exception("Type contains generic parameters " + type);
        }

        internal static Type FindGenericType(Type definition, Type type) {
            for (; type != (Type) null && type != typeof(object); type = type.BaseType) {
                if (type.IsGenericType && AreEquivalent(type.GetGenericTypeDefinition(), definition))
                    return type;
                if (definition.IsInterface) {
                    foreach (Type type1 in type.GetInterfaces()) {
                        Type genericType = FindGenericType(definition, type1);
                        if (genericType != (Type) null)
                            return genericType;
                    }
                }
            }

            return (Type) null;
        }

        internal static bool IsUnsigned(Type type) {
            type = type.GetNonNullableType();
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsFloatingPoint(Type type) {
            type = type.GetNonNullableType();
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        internal static MethodInfo GetBooleanOperator(Type type, string name) {
            do {
                MethodInfo methodValidated = type.GetMethodValidated(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder) null, new Type[1] {
                    type
                }, (ParameterModifier[]) null);
                if (methodValidated != (MethodInfo) null && methodValidated.IsSpecialName && !methodValidated.ContainsGenericParameters)
                    return methodValidated;
                type = type.BaseType;
            } while (type != (Type) null);

            return (MethodInfo) null;
        }

        internal static Type GetNonRefType(this Type type) {
            if (!type.IsByRef)
                return type;
            return type.GetElementType();
        }

        internal static bool CanCache(this Type t) {
            Assembly assembly = t.Assembly;
            if (assembly != _mscorlib && assembly != _systemCore)
                return false;
            if (t.IsGenericType) {
                foreach (Type genericArgument in t.GetGenericArguments()) {
                    if (!genericArgument.CanCache())
                        return false;
                }
            }

            return true;
        }

    }

}