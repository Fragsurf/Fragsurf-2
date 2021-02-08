using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DTCommandPalette {
	public static class DictionaryExtensions {
		private static Random rand_ = new Random();

		public static T PickRandomWeighted<T>(this IDictionary<T, int> source) {
			if (source.Count <= 0) {
				return default(T);
			}

			int weightSum = source.Sum(x => x.Value);
			int chosenIndex = rand_.Next(weightSum);

			foreach (KeyValuePair<T, int> pair in source) {
				int weight = pair.Value;
				if (chosenIndex < weight) {
					return pair.Key;
				}
				chosenIndex -= weight;
			}
			return default(T);
		}

		public static V GetValueOrDefault<U, V>(this IDictionary<U, V> source, U key, V defaultValue = default(V)) {
			if (source.ContainsKey(key)) {
				return source[key];
			} else {
				return defaultValue;
			}
		}

		public static V GetAndCreateIfNotFound<U, V>(this IDictionary<U, V> source, U key) where V : new() {
			if (!source.ContainsKey(key)) {
				source[key] = new V();
			}
			return source[key];
		}

		public static bool DoesntContainKey<K, V>(this Dictionary<K, V> source, K key) {
			return !source.ContainsKey(key);
		}
	}
}