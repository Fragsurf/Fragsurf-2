﻿//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using UnityEditor;
using UnityEngine;

namespace SteamAudio
{
    //
    // SimulationSettingsValueDrawer
    // Custom property drawer for SimulationSettingsValue.
    //

    [CustomPropertyDrawer(typeof(SimulationSettingsValue))]
    public class SimulationSettingsDrawer : PropertyDrawer
    {
        //
        //	Returns the overall height of the drawing area.
        //
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 280;
        }

        //
        //	Draws the property.
        //
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 16f;

            if (position.x <= 0)
            {
                position.x += 4f;
                position.width -= 8f;
            }

            EditorGUI.PropertyField(position, property.FindPropertyRelative("Duration"), new GUIContent("Duration (s)"));
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("AmbisonicsOrder"));
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("MaxSources"));
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("IrradianceMinDistance"));
            position.y += 24f;
            EditorGUI.LabelField(position, "Realtime Settings", EditorStyles.boldLabel);
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("RealtimeRays"));
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("RealtimeSecondaryRays"));
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("RealtimeBounces"));
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("RealtimeThreadsPercentage"), new GUIContent("Realtime CPU Cores (%)"));
            position.y += 24f;
            EditorGUI.LabelField(position, "Baking Settings", EditorStyles.boldLabel);
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("BakeRays"));
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("BakeSecondaryRays"));
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("BakeBounces"));
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("BakeThreadsPercentage"), new GUIContent("Baking CPU Cores (%)"));
            position.y += 24f;
            EditorGUI.LabelField(position, "Occlusion Settings", EditorStyles.boldLabel);
            position.y += 16f;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("MaxOcclusionSamples"));
        }
    }
}