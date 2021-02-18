using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HighlightPlus {

    [CustomEditor(typeof(HighlightProfile))]
    [CanEditMultipleObjects]
    public class HighlightProfileEditor : Editor {

        SerializedProperty effectGroup, effectGroupLayer, effectNameFilter, combineMeshes, alphaCutOff, cullBackFaces;
        SerializedProperty overlay, overlayColor, overlayAnimationSpeed, overlayMinIntensity, overlayBlending;
        SerializedProperty fadeInDuration, fadeOutDuration, constantWidth, normalsOption;
        SerializedProperty outline, outlineColor, outlineWidth, outlineQuality, outlineDownsampling, outlineVisibility, outlineIndependent;
        SerializedProperty glow, glowWidth, glowQuality, glowDownsampling, glowHQColor, glowDithering, glowMagicNumber1, glowMagicNumber2, glowAnimationSpeed;
        SerializedProperty glowBlendPasses, glowVisibility, glowPasses;
        SerializedProperty innerGlow, innerGlowWidth, innerGlowColor, innerGlowVisibility;
        SerializedProperty targetFX, targetFXTexture, targetFXColor, targetFXRotationSpeed, targetFXInitialScale, targetFXEndScale, targetFXTransitionDuration, targetFXStayDuration;
        SerializedProperty seeThrough, seeThroughIntensity, seeThroughTintAlpha, seeThroughTintColor, seeThroughNoise, seeThroughBorder, seeThroughBorderWidth, seeThroughBorderColor;

        void OnEnable() {
            effectGroup = serializedObject.FindProperty("effectGroup");
            effectGroupLayer = serializedObject.FindProperty("effectGroupLayer");
            effectNameFilter = serializedObject.FindProperty("effectNameFilter");
            combineMeshes = serializedObject.FindProperty("combineMeshes");
            alphaCutOff = serializedObject.FindProperty("alphaCutOff");
            normalsOption = serializedObject.FindProperty("normalsOption");
            cullBackFaces = serializedObject.FindProperty("cullBackFaces");
            fadeInDuration = serializedObject.FindProperty("fadeInDuration");
            fadeOutDuration = serializedObject.FindProperty("fadeOutDuration");
            constantWidth = serializedObject.FindProperty("constantWidth");
            overlay = serializedObject.FindProperty("overlay");
            overlayColor = serializedObject.FindProperty("overlayColor");
            overlayAnimationSpeed = serializedObject.FindProperty("overlayAnimationSpeed");
            overlayMinIntensity = serializedObject.FindProperty("overlayMinIntensity");
            overlayBlending = serializedObject.FindProperty("overlayBlending");
            outline = serializedObject.FindProperty("outline");
            outlineColor = serializedObject.FindProperty("outlineColor");
            outlineWidth = serializedObject.FindProperty("outlineWidth");
            outlineQuality = serializedObject.FindProperty("outlineQuality");
            outlineDownsampling = serializedObject.FindProperty("outlineDownsampling");
            outlineVisibility = serializedObject.FindProperty("outlineVisibility");
            outlineIndependent = serializedObject.FindProperty("outlineIndependent");
            glow = serializedObject.FindProperty("glow");
            glowWidth = serializedObject.FindProperty("glowWidth");
            glowQuality = serializedObject.FindProperty("glowQuality");
            glowDownsampling = serializedObject.FindProperty("glowDownsampling");
            glowHQColor = serializedObject.FindProperty("glowHQColor");
            glowAnimationSpeed = serializedObject.FindProperty("glowAnimationSpeed");
            glowDithering = serializedObject.FindProperty("glowDithering");
            glowMagicNumber1 = serializedObject.FindProperty("glowMagicNumber1");
            glowMagicNumber2 = serializedObject.FindProperty("glowMagicNumber2");
            glowAnimationSpeed = serializedObject.FindProperty("glowAnimationSpeed");
            glowBlendPasses = serializedObject.FindProperty("glowBlendPasses");
            glowVisibility = serializedObject.FindProperty("glowVisibility");
            glowPasses = serializedObject.FindProperty("glowPasses");
            innerGlow = serializedObject.FindProperty("innerGlow");
            innerGlowColor = serializedObject.FindProperty("innerGlowColor");
            innerGlowWidth = serializedObject.FindProperty("innerGlowWidth");
            innerGlowVisibility = serializedObject.FindProperty("innerGlowVisibility");
            targetFX = serializedObject.FindProperty("targetFX");
            targetFXTexture = serializedObject.FindProperty("targetFXTexture");
            targetFXRotationSpeed = serializedObject.FindProperty("targetFXRotationSpeed");
            targetFXInitialScale = serializedObject.FindProperty("targetFXInitialScale");
            targetFXEndScale = serializedObject.FindProperty("targetFXEndScale");
            targetFXColor = serializedObject.FindProperty("targetFXColor");
            targetFXTransitionDuration = serializedObject.FindProperty("targetFXTransitionDuration");
            targetFXStayDuration = serializedObject.FindProperty("targetFXStayDuration");
            seeThrough = serializedObject.FindProperty("seeThrough");
            seeThroughIntensity = serializedObject.FindProperty("seeThroughIntensity");
            seeThroughTintAlpha = serializedObject.FindProperty("seeThroughTintAlpha");
            seeThroughTintColor = serializedObject.FindProperty("seeThroughTintColor");
            seeThroughNoise = serializedObject.FindProperty("seeThroughNoise");
            seeThroughBorder = serializedObject.FindProperty("seeThroughBorder");
            seeThroughBorderWidth = serializedObject.FindProperty("seeThroughBorderWidth");
            seeThroughBorderColor = serializedObject.FindProperty("seeThroughBorderColor");
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Highlight Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(effectGroup, new GUIContent("Include"));
            if (effectGroup.intValue == (int)TargetOptions.LayerInScene || effectGroup.intValue == (int)TargetOptions.LayerInChildren) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(effectGroupLayer, new GUIContent("Layer"));
                EditorGUI.indentLevel--;
            }
            if (effectGroup.intValue != (int)TargetOptions.OnlyThisObject && effectGroup.intValue != (int)TargetOptions.Scripting) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(effectNameFilter, new GUIContent("Object Name Filter"));
                EditorGUILayout.PropertyField(combineMeshes);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(alphaCutOff);
            EditorGUILayout.PropertyField(cullBackFaces);
            EditorGUILayout.PropertyField(normalsOption);
            EditorGUILayout.PropertyField(fadeInDuration);
            EditorGUILayout.PropertyField(fadeOutDuration);
            EditorGUILayout.PropertyField(constantWidth);
            EditorGUILayout.PropertyField(outline);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(outlineWidth, new GUIContent("Width"));
            EditorGUILayout.PropertyField(outlineColor, new GUIContent("Color"));
            HighlightEffectEditor.QualityPropertyField(outlineQuality);
            if (outlineQuality.intValue == (int)QualityLevel.Highest) {
                EditorGUILayout.PropertyField(outlineDownsampling, new GUIContent("Downsampling"));
            }
            if (outlineQuality.intValue == (int)QualityLevel.Highest && glowQuality.intValue == (int)QualityLevel.Highest) {
                EditorGUILayout.PropertyField(glowVisibility, new GUIContent("Visibility"));
            } else {
                EditorGUILayout.PropertyField(outlineVisibility, new GUIContent("Visibility"));
            }
            EditorGUILayout.PropertyField(outlineIndependent, new GUIContent("Independent", "Do not combine outline with other highlighted objects."));
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(glow, new GUIContent("Outer Glow"));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(glowWidth, new GUIContent("Width"));
            HighlightEffectEditor.QualityPropertyField(glowQuality);
            if (glowQuality.intValue == (int)QualityLevel.Highest) {
                EditorGUILayout.PropertyField(glowDownsampling, new GUIContent("Downsampling"));
                EditorGUILayout.PropertyField(glowHQColor, new GUIContent("Color"));
            }
            EditorGUILayout.PropertyField(glowAnimationSpeed, new GUIContent("Animation Speed"));
            EditorGUILayout.PropertyField(glowVisibility, new GUIContent("Visibility"));
            if (glowQuality.intValue != (int)QualityLevel.Highest) {
                EditorGUILayout.PropertyField(glowDithering, new GUIContent("Dithering"));
                if (glowDithering.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(glowMagicNumber1, new GUIContent("Magic Number 1"));
                    EditorGUILayout.PropertyField(glowMagicNumber2, new GUIContent("Magic Number 2"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(glowBlendPasses, new GUIContent("Blend Passes"));
                EditorGUILayout.PropertyField(glowPasses, true);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(innerGlow, new GUIContent("Inner Glow"));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(innerGlowColor, new GUIContent("Color"));
            EditorGUILayout.PropertyField(innerGlowWidth, new GUIContent("Width"));
            EditorGUILayout.PropertyField(innerGlowVisibility, new GUIContent("Visibility"));
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(overlay);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(overlayColor, new GUIContent("Color"));
            EditorGUILayout.PropertyField(overlayBlending, new GUIContent("Blending"));
            EditorGUILayout.PropertyField(overlayMinIntensity, new GUIContent("Min Intensity"));
            EditorGUILayout.PropertyField(overlayAnimationSpeed, new GUIContent("Animation Speed"));
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(targetFX, new GUIContent("Target"));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(targetFXTexture, new GUIContent("Texture"));
            EditorGUILayout.PropertyField(targetFXColor, new GUIContent("Color"));
            EditorGUILayout.PropertyField(targetFXRotationSpeed, new GUIContent("Rotation Speed"));
            EditorGUILayout.PropertyField(targetFXInitialScale, new GUIContent("Initial Scale"));
            EditorGUILayout.PropertyField(targetFXEndScale, new GUIContent("End Scale"));
            EditorGUILayout.PropertyField(targetFXTransitionDuration, new GUIContent("Transition Duration"));
            EditorGUILayout.PropertyField(targetFXStayDuration, new GUIContent("Stay Duration"));
            EditorGUI.indentLevel--;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("See-Through Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(seeThrough);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(seeThroughIntensity, new GUIContent("Intensity"));
            EditorGUILayout.PropertyField(seeThroughTintColor, new GUIContent("Color"));
            EditorGUILayout.PropertyField(seeThroughTintAlpha, new GUIContent("Color Blend"));
            EditorGUILayout.PropertyField(seeThroughNoise, new GUIContent("Noise"));
            EditorGUILayout.PropertyField(seeThroughBorder, new GUIContent("Border When Hidden"));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(seeThroughBorderWidth, new GUIContent("Width"));
            EditorGUILayout.PropertyField(seeThroughBorderColor, new GUIContent("Color"));
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            if (serializedObject.ApplyModifiedProperties() || (Event.current.type == EventType.ExecuteCommand &&
                Event.current.commandName == "UndoRedoPerformed")) {

                // Triggers profile reload on all Highlight Effect scripts
                HighlightEffect[] effects = FindObjectsOfType<HighlightEffect>();
                for (int t = 0; t < targets.Length; t++) {
                    HighlightProfile profile = (HighlightProfile)targets[t];
                    for (int k = 0; k < effects.Length; k++) {
                        if (effects[k] != null && effects[k].profile == profile && effects[k].profileSync) {
                            profile.Load(effects[k]);
                            effects[k].Refresh();
                        }
                    }
                }
                EditorUtility.SetDirty(target);
            }

        }


    }

}