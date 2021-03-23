using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DTCommandPalette {
	public static class AppDomainExtensions {
		/// <summary>
		/// Using reflection, get all derived types of a given type
		/// </summary>
		public static System.Type[] GetAllDerivedTypes(this AppDomain appDomain, Type type) {
			List<Type> result = new List<Type>();
			Assembly[] assemblies = appDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies) {
				Type[] types = assembly.GetTypes();
				foreach (Type assemblyType in types) {
					if (assemblyType.IsSubclassOf(type)) {
						result.Add(assemblyType);
					}
				}
			}
			return result.ToArray();
		}
	}
}