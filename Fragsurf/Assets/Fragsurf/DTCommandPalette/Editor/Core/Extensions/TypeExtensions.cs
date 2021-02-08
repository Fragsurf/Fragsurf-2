using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DTCommandPalette {
	public static class TypeExtensions {
		public static IEnumerable<Type> GetParentTypes(this Type type) {
			// is there any base type?
			if ((type == null) || (type.BaseType == null)) {
				yield break;
			}

			// return all implemented or inherited interfaces
			foreach (var i in type.GetInterfaces()) {
				yield return i;
			}

			// return all inherited types
			var currentBaseType = type.BaseType;
			while (currentBaseType != null) {
				yield return currentBaseType;
				currentBaseType = currentBaseType.BaseType;
			}
		}
	}
}
