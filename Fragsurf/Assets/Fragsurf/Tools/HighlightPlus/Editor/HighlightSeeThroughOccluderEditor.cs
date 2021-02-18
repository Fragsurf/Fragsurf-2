using UnityEditor;
using UnityEngine;

namespace HighlightPlus {
	
	[CustomEditor (typeof(HighlightSeeThroughOccluder))]
	public class HighlightSeeThroughOccluderEditor : Editor {
        public override void OnInspectorGUI () {
			EditorGUILayout.Separator ();
			EditorGUILayout.HelpBox ("This object will occlude any see-through effect.", MessageType.Info);
			DrawDefaultInspector ();
		}
	}

}
