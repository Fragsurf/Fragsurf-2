using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HighlightPlus {
    [CustomEditor(typeof(HighlightManager))]
    public class HighlightManagerEditor : Editor {
        public override void OnInspectorGUI() {
            EditorGUILayout.Separator();
            EditorGUILayout.HelpBox("Only objects with a collider can be highlighted automatically.", MessageType.Info);
            DrawDefaultInspector();
        }


		[MenuItem ("GameObject/Effects/Highlight Plus/Create Manager", false, 10)]
		static void CreateManager (MenuCommand menuCommand) {
			HighlightManager manager = FindObjectOfType<HighlightManager> ();
			if (manager == null) {
				GameObject managerGO = new GameObject ("HighlightPlusManager");
				manager = managerGO.AddComponent<HighlightManager> ();
				// Register root object for undo.
				Undo.RegisterCreatedObjectUndo (manager, "Create Highlight Plus Manager");
			}
			Selection.activeObject = manager;
		}

    }

}
