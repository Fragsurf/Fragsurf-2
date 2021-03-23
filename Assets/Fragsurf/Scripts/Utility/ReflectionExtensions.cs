using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Fragsurf.Utility
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetTypesImplementing<T>()
            where T : class
        {
            var result = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = asm.GetTypes()
                    .Where(myType => typeof(T).IsAssignableFrom(myType));
                result.AddRange(types);
            }
            return result;
        }

        public static IEnumerable<Type> GetTypesOf<T>()
            where T : class
        {
            var result = new List<Type>();
            foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = asm.GetTypes()
                    .Where(myType => !myType.IsAbstract && myType.IsClass && myType.IsSubclassOf(typeof(T)));
                result.AddRange(types);
            }
            return result;
        }

        public static IEnumerable<Type> GetEnumerableOfTypesWithAttribute<T>()
            where T : Attribute
        {
            var result = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attribs = type.GetCustomAttributes(typeof(T), false);
                    if (attribs != null && attribs.Length > 0)
                    {
                        result.Add(type);
                    }
                }
            }
            return result;
        }

        public static IEnumerable<MethodInfo> GetMethodsRecursive(this Type type)
        {
            IEnumerable<MethodInfo> methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (type.BaseType != null)
            {
                methods = methods.Concat(GetMethodsRecursive(type.BaseType));
            }

            return methods;
        }
    }
}

