using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DTCommandPalette {
	public static class IEnumerableGenericExtensions {
		public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T item) {
			foreach (T elem in enumerable) {
				yield return elem;
			}

			yield return item;
		}

		public static T MaxOrDefault<T>(this IEnumerable<T> enumerable, Func<T, int> valueTransformation) {
			int maxValue = int.MinValue;
			T maxElem = default(T);

			foreach (T elem in enumerable) {
				int value = valueTransformation.Invoke(elem);
				if (value > maxValue) {
					maxValue = value;
					maxElem = elem;
				}
			}

			return maxElem;
		}

		public static IEnumerable<T> ConcatSingle<T>(this IEnumerable<T> enumerable, T addedElem) {
			foreach (T elem in enumerable) {
				yield return elem;
			}

			yield return addedElem;
		}
	}
}
