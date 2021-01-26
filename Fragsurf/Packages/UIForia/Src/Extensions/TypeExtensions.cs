using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UIForia.Util;

namespace UIForia.Extensions {

    public static class TypeExtensions {

        internal class CacheDict<TKey, TValue> {

            protected readonly int mask;
            protected readonly Entry[] entries;

            internal CacheDict(int size) {
                int length = AlignSize(size);
                this.mask = length - 1;
                this.entries = new Entry[length];
            }

            private static int AlignSize(int size) {
                --size;
                size |= size >> 1;
                size |= size >> 2;
                size |= size >> 4;
                size |= size >> 8;
                size |= size >> 16;
                return size + 1;
            }

            internal bool TryGetValue(TKey key, out TValue value) {
                int hashCode = key.GetHashCode();
                Entry entry = Volatile.Read<Entry>(ref this.entries[hashCode & this.mask]);
                if (entry != null && entry.hash == hashCode && entry.key.Equals((object) key)) {
                    value = entry.value;
                    return true;
                }

                value = default(TValue);
                return false;
            }

            internal void Add(TKey key, TValue value) {
                int hashCode = key.GetHashCode();
                int index = hashCode & this.mask;
                Entry entry = Volatile.Read<Entry>(ref this.entries[index]);
                if (entry != null && entry.hash == hashCode && entry.key.Equals((object) key))
                    return;
                Volatile.Write<Entry>(ref this.entries[index], new Entry(hashCode, key, value));
            }

            internal TValue this[TKey key] {
                get {
                    TValue obj;
                    if (this.TryGetValue(key, out obj))
                        return obj;
                    throw new KeyNotFoundException();
                }
                set { this.Add(key, value); }
            }

            internal class Entry {

                internal readonly int hash;
                internal readonly TKey key;
                internal readonly TValue value;

                internal Entry(int hash, TKey key, TValue value) {
                    this.hash = hash;
                    this.key = key;
                    this.value = value;
                }

            }

        }

        private static readonly CacheDict<MethodBase, ParameterInfo[]> _ParamInfoCache = new CacheDict<MethodBase, ParameterInfo[]>(75);

        // internal static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType, object target) {
        //     DynamicMethod dynamicMethod = methodInfo as DynamicMethod;
        //     if ((MethodInfo) dynamicMethod != (MethodInfo) null)
        //         return dynamicMethod.CreateDelegate(delegateType, target);
        //     return Delegate.CreateDelegate(delegateType, target, methodInfo);
        // }

        internal static Type GetReturnType(this MethodBase mi) {
            if (!mi.IsConstructor)
                return ((MethodInfo) mi).ReturnType;
            return mi.DeclaringType;
        }

        internal static ParameterInfo[] GetParametersCached(this MethodBase method) {
            CacheDict<MethodBase, ParameterInfo[]> paramInfoCache = TypeExtensions._ParamInfoCache;
            ParameterInfo[] parameters;
            if (!paramInfoCache.TryGetValue(method, out parameters)) {
                parameters = method.GetParameters();
                Type declaringType = method.DeclaringType;
                if (declaringType != (Type) null && declaringType.CanCache())
                    paramInfoCache[method] = parameters;
            }

            return parameters;
        }

        internal static bool IsByRefParameter(this ParameterInfo pi) {
            if (pi.ParameterType.IsByRef)
                return true;
            return (pi.Attributes & ParameterAttributes.Out) == ParameterAttributes.Out;
        }

        internal static MethodInfo GetMethodValidated(
            this Type type,
            string name,
            BindingFlags bindingAttr,
            Binder binder,
            Type[] types,
            ParameterModifier[] modifiers) {
            MethodInfo method = type.GetMethod(name, bindingAttr, binder, types, modifiers);
            if (!method.MatchesArgumentTypes(types))
                return (MethodInfo) null;
            return method;
        }

        private static bool MatchesArgumentTypes(this MethodInfo mi, Type[] argTypes) {
            if (mi == (MethodInfo) null || argTypes == null)
                return false;
            ParameterInfo[] parameters = mi.GetParameters();
            if (parameters.Length != argTypes.Length)
                return false;
            for (int index = 0; index < parameters.Length; ++index) {
                if (!TypeUtil.AreReferenceAssignable(parameters[index].ParameterType, argTypes[index]))
                    return false;
            }

            return true;
        }


        public static bool Implements(this Type type, Type interfaceType) {
            if (!interfaceType.IsInterface) {
                return false;
            }

            return interfaceType.IsAssignableFrom(type);
        }

        public static Type GetTypeFromSimpleName(string typeName) {
            if (typeName == null) {
                throw new ArgumentNullException(nameof(typeName));
            }

            bool isArray = false, isNullable = false;

            if (typeName.IndexOf("[]", StringComparison.Ordinal) != -1) {
                isArray = true;
                typeName = typeName.Remove(typeName.IndexOf("[]", StringComparison.Ordinal), 2);
            }

            if (typeName.IndexOf("?", StringComparison.Ordinal) != -1) {
                isNullable = true;
                typeName = typeName.Remove(typeName.IndexOf("?", StringComparison.Ordinal), 1);
            }

            typeName = typeName.ToLower();

            string parsedTypeName = null;
            switch (typeName) {
                case "bool":
                case "boolean":
                    parsedTypeName = "System.Boolean";
                    break;
                case "byte":
                    parsedTypeName = "System.Byte";
                    break;
                case "char":
                    parsedTypeName = "System.Char";
                    break;
                case "datetime":
                    parsedTypeName = "System.DateTime";
                    break;
                case "datetimeoffset":
                    parsedTypeName = "System.DateTimeOffset";
                    break;
                case "decimal":
                    parsedTypeName = "System.Decimal";
                    break;
                case "double":
                    parsedTypeName = "System.Double";
                    break;
                case "float":
                    parsedTypeName = "System.Single";
                    break;
                case "int16":
                case "short":
                    parsedTypeName = "System.Int16";
                    break;
                case "int32":
                case "int":
                    parsedTypeName = "System.Int32";
                    break;
                case "int64":
                case "long":
                    parsedTypeName = "System.Int64";
                    break;
                case "object":
                    parsedTypeName = "System.Object";
                    break;
                case "sbyte":
                    parsedTypeName = "System.SByte";
                    break;
                case "string":
                    parsedTypeName = "System.String";
                    break;
                case "timespan":
                    parsedTypeName = "System.TimeSpan";
                    break;
                case "uint16":
                case "ushort":
                    parsedTypeName = "System.UInt16";
                    break;
                case "uint32":
                case "uint":
                    parsedTypeName = "System.UInt32";
                    break;
                case "uint64":
                case "ulong":
                    parsedTypeName = "System.UInt64";
                    break;
            }

            if (parsedTypeName != null) {
                if (isArray) {
                    parsedTypeName = parsedTypeName + "[]";
                }

                if (isNullable) {
                    parsedTypeName = string.Concat("System.Nullable`1[", parsedTypeName, "]");
                }
            }
            else {
                parsedTypeName = typeName;
            }

            // Expected to throw an exception in case the type has not been recognized.
            return Type.GetType(parsedTypeName);
        }

    }

}