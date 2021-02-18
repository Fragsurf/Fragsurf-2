using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DTCommandPalette {
	public static class IListExtensions {
		public static bool ContainsIndex(this IList list, int index) {
			return index >= 0 && index < list.Count;
		}

		public static bool IsNullOrEmpty(this IList list) {
			return list == null || list.Count == 0;
		}

		public static bool IsNullOrEmpty<T>(this IList<T> list) {
			return list == null || list.Count == 0;
		}

		public static void ReverseForeach<T>(this IList<T> list, Action<T> action) {
			for (int i = list.Count - 1; i >= 0; i--) {
				action.Invoke(list[i]);
			}
		}
	}
}
