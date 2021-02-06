using UnityEngine;
using UnityEditor;

namespace Fragsurf.FSM.Actors
{
    [CustomEditor(typeof(FSMTrack))]
    public class FSMTrackEditor : Editor
    {

        SerializedProperty _linearData,
            _stageData,
            _bonusData,
            _trackType,
            _trackName;

        private void OnEnable()
        {
            _trackType = serializedObject.FindProperty("_trackType");
            _trackName = serializedObject.FindProperty("_trackName");
            _linearData = serializedObject.FindProperty("_linearData");
            _stageData = serializedObject.FindProperty("_stageData");
            _bonusData = serializedObject.FindProperty("_bonusData");
        }

        public override void OnInspectorGUI()
        {
            var tracks = FindObjectsOfType<FSMTrack>(true);
            var tt = serializedObject.targetObject as FSMTrack;
            var mainTrackAlreadyExists = false;

            EditorGUILayout.BeginVertical();

            foreach(var tr in tracks)
            {
                if(tr == tt)
                {
                    continue;
                }

                var isMain = tr.IsMainTrack;
                var trackName = !string.IsNullOrEmpty(tr.TrackName)
                    ? tr.TrackName
                    : tr.gameObject.name;
                var btnStr = $"[{(isMain ? "Main" : "Extra")}, {tr.TrackType} {(tr.isActiveAndEnabled ? ", Disabled" : "")}] {trackName}";

                if (GUILayout.Button(btnStr))
                {
                    Selection.activeObject = tr.gameObject;
                }

                if (isMain)
                {
                    mainTrackAlreadyExists = true;
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(_trackName);
            EditorGUILayout.PropertyField(_trackType);

            if (tt.TrackType != FSMTrackType.Bonus
                && mainTrackAlreadyExists)
            {
                EditorGUILayout.HelpBox($"A main track already exists, change this one to a bonus or remove the main track.", MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space(10);

            switch (tt.TrackType) 
            {
                case FSMTrackType.Linear:
                    EditorGUILayout.PropertyField(_linearData);
                    break;
                case FSMTrackType.Staged:
                    EditorGUILayout.PropertyField(_stageData);
                    break;
                case FSMTrackType.Bonus:
                    EditorGUILayout.PropertyField(_bonusData);
                    break;
            }

            serializedObject.ApplyModifiedProperties();

        }

    }
}

