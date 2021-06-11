﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperUnityBuild.BuildTool
{
    [System.Serializable]
    public class BuildPlatform : ScriptableObject
    {
        public bool enabled = false;
        public BuildDistributionList distributionList = new BuildDistributionList();
        public BuildArchitecture[] architectures = new BuildArchitecture[0];
        public BuildVariant[] variants = new BuildVariant[0];

        public string platformName;
        public string dataDirNameFormat;
        public BuildTargetGroup targetGroup;

        public virtual void Init()
        {
        }

        public virtual void ApplyVariant()
        {
        }

        #region Public Properties

        public bool atLeastOneArch
        {
            get
            {
                bool atLeastOneArch = false;
                for (int i = 0; i < architectures.Length && !atLeastOneArch; i++)
                {
                    atLeastOneArch |= architectures[i].enabled;
                }

                return atLeastOneArch;
            }
        }

        public bool atLeastOneDistribution
        {
            get
            {
                bool atLeastOneDist = false;
                for (int i = 0; i < distributionList.distributions.Length && !atLeastOneDist; i++)
                {
                    atLeastOneDist |= distributionList.distributions[i].enabled;
                }

                return atLeastOneDist;
            }
        }

        public string variantKey
        {
            get
            {
                string retVal = "";

                // Build key string.
                if (variants != null && variants.Length > 0)
                {
                    foreach (var variant in variants)
                        retVal += variant.variantKey + ",";
                }

                // Remove trailing delimiter.
                if (retVal.Length > 0)
                    retVal = retVal.Substring(0, retVal.Length - 1);

                return retVal;
            }
        }

        #endregion

        public void Draw(SerializedObject obj)
        {
            EditorGUILayout.BeginVertical(UnityBuildGUIUtility.dropdownContentStyle);

            SerializedProperty archList = obj.FindProperty("architectures");

            if (archList.arraySize > 1)
            {
                GUILayout.Label("Architectures", UnityBuildGUIUtility.midHeaderStyle);
                for (int i = 0; i < archList.arraySize; i++)
                {
                    SerializedProperty archProperty = archList.GetArrayElementAtIndex(i);
                    SerializedProperty archName = archProperty.FindPropertyRelative("name");
                    SerializedProperty archEnabled = archProperty.FindPropertyRelative("enabled");

                    archEnabled.boolValue = GUILayout.Toggle(archEnabled.boolValue, archName.stringValue);
                    archProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            SerializedProperty variantList = obj.FindProperty("variants");

            if (variantList.arraySize > 0)
            {
                GUILayout.Label("Variant Options", UnityBuildGUIUtility.midHeaderStyle);

                for (int i = 0; i < variantList.arraySize; i++)
                {
                    SerializedProperty variantProperty = variantList.GetArrayElementAtIndex(i);
                    SerializedProperty variantName = variantProperty.FindPropertyRelative("variantName");
                    SerializedProperty variantValues = variantProperty.FindPropertyRelative("values");
                    SerializedProperty selectedVariantIndex = variantProperty.FindPropertyRelative("selectedIndex");

                    List<string> valueNames = new List<string>(variantValues.arraySize);
                    for (int j = 0; j < variantValues.arraySize; j++)
                    {
                        valueNames.Add(variantValues.GetArrayElementAtIndex(j).stringValue);
                    }

                    GUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(variantName.stringValue);
                    selectedVariantIndex.intValue =
                        EditorGUILayout.Popup(selectedVariantIndex.intValue, valueNames.ToArray(), UnityBuildGUIUtility.popupStyle, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(250));

                    GUILayout.EndHorizontal();
                }
            }

            SerializedProperty distList = obj.FindProperty("distributionList.distributions");

            if (distList.arraySize > 0)
            {
                GUILayout.Label("Distributions", UnityBuildGUIUtility.midHeaderStyle);

                for (int i = 0; i < distList.arraySize; i++)
                {
                    SerializedProperty dist = distList.GetArrayElementAtIndex(i);
                    SerializedProperty distEnabled = dist.FindPropertyRelative("enabled");
                    SerializedProperty distName = dist.FindPropertyRelative("distributionName");

                    GUILayout.BeginHorizontal();

                    distEnabled.boolValue = GUILayout.Toggle(distEnabled.boolValue, GUIContent.none, GUILayout.ExpandWidth(false));
                    distName.stringValue = BuildProject.SanitizeFolderName(GUILayout.TextField(distName.stringValue));

                    if (GUILayout.Button("X", UnityBuildGUIUtility.helpButtonStyle))
                    {
                        distList.DeleteArrayElementAtIndex(i);
                    }

                    dist.serializedObject.ApplyModifiedProperties();

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Space(10);
            GUILayout.BeginVertical();

            if (GUILayout.Button("Add Distribution", GUILayout.ExpandWidth(true)))
            {
                int addedIndex = distList.arraySize;
                distList.InsertArrayElementAtIndex(addedIndex);

                SerializedProperty addedProperty = distList.GetArrayElementAtIndex(addedIndex);
                addedProperty.FindPropertyRelative("enabled").boolValue = true;
                addedProperty.FindPropertyRelative("distributionName").stringValue = "DistributionName";

                addedProperty.serializedObject.ApplyModifiedProperties();
                distList.serializedObject.ApplyModifiedProperties();
                GUIUtility.keyboardControl = 0;
            }
            GUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            obj.ApplyModifiedProperties();
        }

        protected static T EnumValueFromKey<T>(string label)
        {
            return (T)Enum.Parse(typeof(T), label.Replace(" ", ""));
        }

        protected static string[] EnumNamesToArray<T>(bool toWords = false)
        {
            return Enum.GetNames(typeof(T))
                .Select(item => toWords ? UnityBuildGUIUtility.ToWords(item) : item)
                .ToArray();
        }
    }
}
