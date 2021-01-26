using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Extensions;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using Debug = UnityEngine.Debug;

namespace UIForia.Parsing {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class GenericElementTypeResolvedByAttribute : Attribute {

        public readonly string propertyName;

        public GenericElementTypeResolvedByAttribute(string propertyName) {
            this.propertyName = propertyName;
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ResolveGenericTemplateArguments : Attribute { }

    public static class TypeProcessor {

        internal struct TypeList {

            public ProcessedType mainType;
            public ProcessedType[] types;

        }

        public static bool processedTypes;
        private static readonly LightList<ProcessedType> templateTypes = new LightList<ProcessedType>(64);
        public static readonly Dictionary<Type, ProcessedType> typeMap = new Dictionary<Type, ProcessedType>();
        internal static readonly Dictionary<string, TypeList> templateTypeMap = new Dictionary<string, TypeList>();
        public static readonly Dictionary<string, LightList<Assembly>> s_NamespaceMap = new Dictionary<string, LightList<Assembly>>();
        public static readonly Dictionary<string, ProcessedType> s_GenericMap = new Dictionary<string, ProcessedType>();
        private static readonly List<ProcessedType> dynamicTypes = new List<ProcessedType>();

        private static int currentTypeId;
        private static int NextTypeId => currentTypeId++;

        private static readonly string[] s_SingleNamespace = new string[1];

        private static void FilterAssemblies() {
            if (processedTypes) return;
            processedTypes = true;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            int count = 0;
            for (int i = 0; i < assemblies.Length; i++) {
                Assembly assembly = assemblies[i];

                if (assembly == null || assembly.IsDynamic) {
                    continue;
                }

                bool filteredOut = !FilterAssembly(assembly);
                bool shouldProcessTypes = ShouldProcessTypes(assembly, filteredOut);

                if (!shouldProcessTypes) {
                    continue;
                }

                count++;

                try {
                    Type[] types = assembly.GetTypes();

                    for (int j = 0; j < types.Length; j++) {
                        Type currentType = types[j];
                        // can be null if assembly referenced is unavailable
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (currentType == null) {
                            continue;
                        }

                        if (!filteredOut && currentType.IsClass && currentType.Name[0] != '<' && currentType.IsGenericTypeDefinition) {
                            if (currentType.IsSubclassOf(typeof(UIElement))) {
                                Attribute[] attrs = Attribute.GetCustomAttributes(currentType, false);
                                string tagName = GetTemplateAttribute(currentType, attrs, out TemplateAttribute templateAttr);

                                // todo -- support namespaces in the look up map
                                tagName = tagName.Split('`')[0];

                                ProcessedType processedType = new ProcessedType(currentType, templateAttr, tagName);
                                processedType.IsUnresolvedGeneric = true;
                                try {
                                    s_GenericMap.Add(tagName, processedType);
                                }
                                catch (Exception) {
                                    Debug.LogError($"UIForia does not support multiple elements with the same tag name. Tried to register type {processedType.rawType} for `{tagName}` " +
                                                   $"but this tag name was already taken by type {s_GenericMap[tagName].rawType}. For generic overload types with multiple arguments you need to supply a unique [TagName] attribute");
                                    continue;
                                }

                                typeMap[currentType] = processedType;

                                if (!s_NamespaceMap.TryGetValue(currentType.Namespace ?? "null", out LightList<Assembly> namespaceList)) {
                                    namespaceList = new LightList<Assembly>(2);
                                    s_NamespaceMap.Add(currentType.Namespace ?? "null", namespaceList);
                                }

                                if (!namespaceList.Contains(assembly)) {
                                    namespaceList.Add(assembly);
                                }

                                continue;
                            }
                        }

                        if (!filteredOut && currentType.IsClass && !currentType.IsGenericTypeDefinition) {
                            Attribute[] attrs = Attribute.GetCustomAttributes(currentType, false);
                            Application.ProcessClassAttributes(currentType, attrs);

                            if (typeof(UIElement).IsAssignableFrom(currentType)) {
                                string tagName = GetTemplateAttribute(currentType, attrs, out TemplateAttribute templateAttr);

                                ProcessedType processedType = new ProcessedType(currentType, templateAttr, tagName);

                                if (templateAttr != null) {
                                    templateTypes.Add(processedType);
                                }

                                // if (templateTypeMap.ContainsKey(tagName)) {
                                //     Debug.Log($"Tried to add template key `{tagName}` from type {currentType} but it was already defined by {templateTypeMap.GetOrDefault(tagName).rawType}");
                                // }

                                if (templateTypeMap.TryGetValue(tagName, out TypeList typeList)) {
                                    if (typeList.types != null) {
                                        Array.Resize(ref typeList.types, typeList.types.Length + 1);
                                        typeList.types[typeList.types.Length - 1] = processedType;
                                    }
                                    else {
                                        typeList.types = new ProcessedType[2];
                                        typeList.types[0] = typeList.mainType;
                                        typeList.types[1] = processedType;
                                    }
                                }
                                else {
                                    typeList.mainType = processedType;
                                    templateTypeMap[tagName] = typeList;
                                }

                                // templateTypeMap.Add(tagName, processedType);
                                processedType.id = NextTypeId;
                                typeMap[currentType] = processedType;
                            }
                        }

                        if (filteredOut && !currentType.IsPublic) {
                            continue;
                        }

                        if (!s_NamespaceMap.TryGetValue(currentType.Namespace ?? "null", out LightList<Assembly> list)) {
                            list = new LightList<Assembly>(2);
                            s_NamespaceMap.Add(currentType.Namespace ?? "null", list);
                        }

                        if (!list.Contains(assembly)) {
                            list.Add(assembly);
                        }
                    }
                }
                catch (ReflectionTypeLoadException) {
                    Debug.Log($"{assembly.FullName}");
                    throw;
                }
            }

            watch.Stop();
            Debug.Log($"Loaded types in: {watch.ElapsedMilliseconds} ms from {count} assemblies");
        }

        private static string GetTemplateAttribute(Type currentType, Attribute[] attrs, out TemplateAttribute templateAttr) {
            string tagName = currentType.Name;
            templateAttr = null;

            for (int index = 0; index < attrs.Length; index++) {
                Attribute attr = attrs[index];

                if (attr is TemplateTagNameAttribute templateTagNameAttr) {
                    tagName = templateTagNameAttr.tagName;
                }

                if (attr is TemplateAttribute templateAttribute) {
                    templateAttr = templateAttribute;
                }
            }

            // if no template attribute is defined, assume the default scheme
            if (templateAttr == null) {
                templateAttr = new TemplateAttribute(TemplateType.DefaultFile, null);
            }

            return tagName;
        }

        private static Type ResolveSimpleType(string typeName) {
            switch (typeName) {
                case "bool": return typeof(bool);
                case "byte": return typeof(byte);
                case "sbyte": return typeof(sbyte);
                case "char": return typeof(char);
                case "decimal": return typeof(decimal);
                case "double": return typeof(double);
                case "float": return typeof(float);
                case "int": return typeof(int);
                case "uint": return typeof(uint);
                case "long": return typeof(long);
                case "ulong": return typeof(ulong);
                case "object": return typeof(object);
                case "short": return typeof(short);
                case "ushort": return typeof(ushort);
                case "string": return typeof(string);
            }

            return null;
        }

        public static Type ResolveTypeExpression(Type invokingType, IList<string> namespaces, string typeExpression) {
            typeExpression = typeExpression.Replace("[", "<").Replace("]", ">");
            if (ExpressionParser.TryParseTypeName(typeExpression, out TypeLookup typeLookup)) {
                return ResolveType(typeLookup, (IReadOnlyList<string>) namespaces, invokingType);
            }

            return null;
        }

        private static LightList<Type> ResolveGenericTypes(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null) {
            int count = typeLookup.generics.size;

            LightList<Type> results = LightList<Type>.Get();
            results.EnsureCapacity(count);

            Type[] array = results.array;
            Type[] generics = null;
            Type[] concreteArgs = null;
            if (scopeType != null) {
                if (scopeType.IsGenericType) {
                    generics = scopeType.GetGenericTypeDefinition().GetGenericArguments();
                    concreteArgs = scopeType.GetGenericArguments();
                }
            }

            for (int i = 0; i < count; i++) {
                if (generics != null) {
                    for (int j = 0; j < generics.Length; j++) {
                        if (typeLookup.generics[i].typeName == generics[j].Name) {
                            array[i] = concreteArgs[i];
                            break;
                        }
                    }
                }

                array[i] = array[i] ?? ResolveType(typeLookup.generics[i], namespaces);

                if (array[i] == null) {
                    throw new TypeResolutionException($"Failed to find a type from string {typeLookup.generics[i]}");
                }
            }

            results.Count = typeLookup.generics.size;
            return results;
        }

        private static Type ResolveBaseTypePath(TypeLookup typeLookup, IReadOnlyList<string> namespaces) {
            Type retn = ResolveSimpleType(typeLookup.typeName);

            if (retn != null) {
                return retn;
            }

            string baseTypeName = "." + typeLookup.GetBaseTypeName(); // save some string concat
            if (!string.IsNullOrEmpty(typeLookup.namespaceName)) {
                LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(typeLookup.namespaceName);

                if (assemblies == null) {
                    throw new TypeResolutionException($"No loaded assemblies found for namespace {typeLookup.namespaceName}");
                }

                string typename = typeLookup.namespaceName + baseTypeName;

                for (int a = 0; a < assemblies.Count; a++) {
                    retn = assemblies[a].GetType(typename);
                    if (retn != null) {
                        return retn;
                    }
                }

                LightList<Assembly> lastDitchAssemblies = s_NamespaceMap.GetOrDefault("null");
                if (lastDitchAssemblies != null) {
                    typename = typeLookup.typeName;
                    for (int a = 0; a < lastDitchAssemblies.Count; a++) {
                        retn = lastDitchAssemblies[a].GetType(typename);
                        if (retn != null) {
                            return retn;
                        }
                    }
                }
            }
            else {
                if (namespaces != null) {
                    for (int i = 0; i < namespaces.Count; i++) {
                        LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(namespaces[i]);
                        if (assemblies == null) {
                            continue;
                        }

                        string typename = namespaces[i] + baseTypeName;
                        for (int a = 0; a < assemblies.Count; a++) {
                            retn = assemblies[a].GetType(typename);
                            if (retn != null) {
                                return retn;
                            }
                        }
                    }
                }

                LightList<Assembly> lastDitchAssemblies = s_NamespaceMap.GetOrDefault("null");
                if (lastDitchAssemblies != null) {
                    string typename = typeLookup.typeName;
                    for (int a = 0; a < lastDitchAssemblies.Count; a++) {
                        retn = lastDitchAssemblies[a].GetType(typename);
                        if (retn != null) {
                            return retn;
                        }
                    }
                }
            }

            if (namespaces != null && namespaces.Count > 0 && namespaces[0] != string.Empty) {
                string checkedNamespaces = string.Join(",", namespaces.ToArray());
                throw new TypeResolutionException($"Unable to resolve type {typeLookup}. Looked in namespaces: {checkedNamespaces}");
            }
            else {
                throw new TypeResolutionException($"Unable to resolve type {typeLookup}.");
            }
        }

        public static Type ResolveType(string typeName, string namespaceName) {
            s_SingleNamespace[0] = namespaceName ?? string.Empty;
            return ResolveType(new TypeLookup(typeName), s_SingleNamespace);
        }

        public static Type ResolveType(TypeLookup typeLookup, string namespaceName) {
            s_SingleNamespace[0] = namespaceName ?? string.Empty;
            return ResolveType(typeLookup, s_SingleNamespace);
        }

        public static Type ResolveType(TypeLookup typeLookup, IReadOnlyList<string> namespaces = null, Type scopeType = null) {
            if (typeLookup.resolvedType != null) {
                return typeLookup.resolvedType;
            }

            FilterAssemblies();

            // base type will valid or an exception will be thrown
            Type baseType = ResolveBaseTypePath(typeLookup, namespaces);

            if (typeLookup.generics != null && typeLookup.generics.Count != 0) {
                if (!baseType.IsGenericTypeDefinition) {
                    throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
                }

                baseType = ReflectionUtil.CreateGenericType(baseType, ResolveGenericTypes(typeLookup, namespaces, scopeType));
            }

            if (typeLookup.isArray) {
                baseType = baseType.MakeArrayType();
            }

            return baseType;
        }

        public static Type ResolveNestedGenericType(Type containingType, Type baseType, TypeLookup typeLookup, LightList<string> namespaces) {
            FilterAssemblies();

            if (!baseType.IsGenericTypeDefinition) {
                throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
            }

            if (!baseType.IsGenericTypeDefinition) {
                throw new TypeResolutionException($"{baseType} is not a generic type definition but we are trying to resolve a generic type with it because generic arguments were provided");
            }

            if (typeLookup.generics == null || typeLookup.generics.Count == 0) {
                throw new TypeResolutionException($"Tried to resolve generic types from {baseType} but no generic types were given in the {nameof(typeLookup)} argument");
            }

            return ReflectionUtil.CreateNestedGenericType(containingType, baseType, ResolveGenericTypes(typeLookup, namespaces));
        }

        public static Type ResolveType(string typeName, IReadOnlyList<string> namespaces) {
            FilterAssemblies();

            for (int i = 0; i < namespaces.Count; i++) {
                LightList<Assembly> assemblies = s_NamespaceMap.GetOrDefault(namespaces[i]);
                if (assemblies == null) {
                    continue;
                }

                string prefixedTypeName = namespaces[i] + "." + typeName + ", ";
                foreach (Assembly assembly in assemblies) {
                    string fullTypeName = prefixedTypeName + assembly.FullName;

                    Type retn = Type.GetType(fullTypeName);

                    if (retn != null) {
                        return retn;
                    }
                }
            }

            LightList<Assembly> lastDitchAssemblies = s_NamespaceMap.GetOrDefault("null");
            if (lastDitchAssemblies != null) {
                string typename = typeName;
                for (int a = 0; a < lastDitchAssemblies.Count; a++) {
                    Type retn = lastDitchAssemblies[a].GetType(typename);
                    if (retn != null) {
                        return retn;
                    }
                }
            }

            return null;
        }

        internal static ProcessedType GetProcessedType(Type type) {
            FilterAssemblies();
            typeMap.TryGetValue(type, out ProcessedType retn);
            if (retn != null) {
                retn.references++;
            }

            return retn;
        }

        private static bool ShouldProcessTypes(Assembly assembly, bool wasFilteredOut) {
            string name = assembly.FullName;
            return !wasFilteredOut || (name.StartsWith("System") || name.StartsWith("Unity") || name.Contains("mscorlib"));
        }

        private static bool FilterAssembly(Assembly assembly) {
            string name = assembly.FullName;

            if (assembly.IsDynamic ||
                name.StartsWith("System,") ||
                name.StartsWith("Accessibility") ||
                name.StartsWith("Boo") ||
                name.StartsWith("I18N") ||
                name.StartsWith("TextMeshPro") ||
                name.StartsWith("nunit") ||
                name.StartsWith("System.") ||
                name.StartsWith("Microsoft.") ||
                name.StartsWith("Mono") ||
                name.StartsWith("Unity.") ||
                name.StartsWith("ExCSS.") ||
                name.Contains("mscorlib") ||
                name.Contains("JetBrains") ||
                name.Contains("UnityEngine") ||
                name.Contains("UnityEditor") ||
                name.Contains("Jetbrains")) {
                return false;
            }

            return name.IndexOf("-firstpass", StringComparison.Ordinal) == -1;
        }

        public static LightList<ProcessedType> GetTemplateTypes() {
            FilterAssemblies();
            return templateTypes;
        }

        private static readonly List<string> EmptyNamespaceList = new List<string>();


        // Namespace resolution
        //    if there is only one element with a name then no namespace is needed
        //    if there are multiple elements with a name
        //        namespace is required in order to match the correct one
        //    using declarations can provide implicit namespaces
        public static ProcessedType ResolveTagName(string tagName, string namespacePrefix, IReadOnlyList<string> namespaces) {
            FilterAssemblies();

            namespaces = namespaces ?? EmptyNamespaceList;

            if (string.IsNullOrEmpty(namespacePrefix)) namespacePrefix = null;
            if (string.IsNullOrWhiteSpace(namespacePrefix)) namespacePrefix = null;

            if (templateTypeMap.TryGetValue(tagName, out TypeList typeList)) {
                // if this is null we resolve using just the tag name
                if (namespacePrefix == null) {
                    // if only one type has this tag name we can safely return it
                    if (typeList.types == null) {
                        return typeList.mainType.Reference();
                    }

                    // if there are multiple tags with this name, we need to search our namespaces 
                    // if only one match is found, we can return it. If multiple are found, throw
                    // and ambiguous reference exception
                    LightList<ProcessedType> resultList = LightList<ProcessedType>.Get();
                    for (int i = 0; i < namespaces.Count; i++) {
                        for (int j = 0; j < typeList.types.Length; j++) {
                            string namespaceName = namespaces[i];
                            ProcessedType testType = typeList.types[j];
                            if (namespaceName == testType.namespaceName) {
                                resultList.Add(testType);
                            }
                        }
                    }

                    if (resultList.size == 1) {
                        ProcessedType retn = resultList[0];
                        resultList.Release();
                        return retn.Reference();
                    }

                    List<string> list = resultList.Select((s) => s.namespaceName).ToList();
                    throw new ParseException("Ambiguous TagName reference: " + tagName + ". References found in namespaces " + StringUtil.ListToString(list, ", "));
                }

                if (typeList.types == null) {
                    if (namespacePrefix == typeList.mainType.namespaceName) {
                        return typeList.mainType.Reference();
                    }
                }
                else {
                    // if prefix is not null we can only return a match for that namespace
                    for (int j = 0; j < typeList.types.Length; j++) {
                        ProcessedType testType = typeList.types[j];
                        if (namespacePrefix == testType.namespaceName) {
                            return testType.Reference();
                        }
                    }
                }

                return null;
            }

            if (s_GenericMap.TryGetValue(tagName, out ProcessedType processedType)) {
                return processedType;
            }

            return null;
        }

        public static bool IsNamespace(string toCheck) {
            FilterAssemblies();
            return s_NamespaceMap.ContainsKey(toCheck);
        }

        public static ProcessedType AddResolvedGenericElementType(Type newType, TemplateAttribute templateAttr, string tagName) {
            ProcessedType retn = null;
            if (!typeMap.TryGetValue(newType, out retn)) {
                retn = new ProcessedType(newType, templateAttr, tagName);
                retn.id = NextTypeId;
                typeMap.Add(retn.rawType, retn);
            }

            if (retn != null) {
                retn.references++;
            }

            return retn;
        }

        // todo -- would be good to have this be an instance property because we need to clear dynamics every time we compile
        public static void AddDynamicElementType(ProcessedType processedType) {
            processedType.id = NextTypeId;
            typeMap[processedType.rawType] = processedType;
            dynamicTypes.Add(processedType);
            // templateTypes.Add(processedType);
            // todo -- maybe add to namespace map?
        }

        public static void ClearDynamics() {
            for (int i = 0; i < dynamicTypes.Count; i++) {
                typeMap.Remove(dynamicTypes[i].rawType);
            }
        }
        
    }

    public class TypeResolutionException : Exception {

        public TypeResolutionException(string message) : base(message) { }

    }

}