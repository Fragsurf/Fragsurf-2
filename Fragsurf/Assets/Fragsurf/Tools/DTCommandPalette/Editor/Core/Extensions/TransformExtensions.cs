using System.Collections;
using UnityEngine;

namespace DTCommandPalette {
	public static class TransformExtensions {
		public static T GetComponentInParent<T>(this Transform transform, bool required = true) {
			T found = transform.gameObject.GetComponentInParent<T>();
			if (found == null && required) {
				Debug.LogError("GetComponentInParent<T> - missing parent component!");
			}
			return found;
		}

		public static T GetRequiredComponentInParent<T>(this Transform transform, bool required = true) {
			return transform.gameObject.GetRequiredComponentInParent<T>();
		}

		public static void DestroyAllChildren(this Transform transform, bool immediate = false) {
			GameObject[] children = new GameObject[transform.childCount];

			int index = 0;
			foreach (Transform child in transform) {
				children[index++] = child.gameObject;
			}

			for (int i = children.Length - 1; i >= 0; i--) {
				if (immediate) {
					GameObject.DestroyImmediate(children[i]);
				} else {
					GameObject.Destroy(children[i]);
				}
			}
		}
	}
}
