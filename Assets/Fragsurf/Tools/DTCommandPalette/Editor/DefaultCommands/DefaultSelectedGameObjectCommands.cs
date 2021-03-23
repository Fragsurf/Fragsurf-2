using System;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public static class DefaultSelectedGameObjectCommands {
		[MethodCommand]
		public static void CreateEmptyChildOnSelectedGameObject() {
			GameObject obj = Selection.activeGameObject;
			if (obj == null) {
				return;
			}

			GameObject newChild = new GameObject();
			newChild.transform.SetParent(obj.transform);
			Selection.activeGameObject = newChild;
		}

		[MethodCommand]
		public static void RenameSelectedGameObject(string name) {
			GameObject obj = Selection.activeGameObject;
			if (obj == null) {
				return;
			}

			obj.name = name;
		}
	}
}
