using UnityEditor;
using UnityEngine;

namespace HighlightPlus {
    [CustomEditor(typeof(HighlightTrigger))]
    public class HighlightTriggerEditor : Editor {

        SerializedProperty triggerMode, raycastCamera, raycastSource, maxDistance, volumeLayerMask;
		HighlightTrigger trigger;

		void OnEnable() {
			triggerMode = serializedObject.FindProperty ("triggerMode");
			raycastCamera = serializedObject.FindProperty ("raycastCamera");
			raycastSource = serializedObject.FindProperty ("raycastSource");
            maxDistance = serializedObject.FindProperty("maxDistance");
            volumeLayerMask = serializedObject.FindProperty("volumeLayerMask");
			trigger = (HighlightTrigger)target;
			trigger.Init ();
		}

        public override void OnInspectorGUI() {

			serializedObject.Update ();

			if (trigger.triggerMode == TriggerMode.RaycastOnThisObjectAndChildren) {
				if (trigger.colliders == null || trigger.colliders.Length == 0) {
					EditorGUILayout.HelpBox ("No collider found on this object or any of its children. Add colliders to allow automatic highlighting.", MessageType.Warning);
				}
			} else {
				if (trigger.GetComponent<Collider> () == null) {
					EditorGUILayout.HelpBox ("No collider found on this object. Add a collider to allow automatic highlighting.", MessageType.Error);
				}
            }

            EditorGUILayout.PropertyField(triggerMode);
            switch (trigger.triggerMode) {
                case TriggerMode.RaycastOnThisObjectAndChildren:
                    EditorGUILayout.PropertyField(raycastCamera);
                    EditorGUILayout.PropertyField(raycastSource);
                    EditorGUILayout.PropertyField(maxDistance, new GUIContent("Max Distance", "Max distance for target. 0 = infinity")); ;
                    break;
                case TriggerMode.Volume:
                    EditorGUILayout.PropertyField(volumeLayerMask);
                    break;
            }

            if (serializedObject.ApplyModifiedProperties()) {
                trigger.Init();
            }
        }

    }

}
