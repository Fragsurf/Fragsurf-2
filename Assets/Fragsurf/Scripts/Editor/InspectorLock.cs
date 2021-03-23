using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InspectorLock
{
    [MenuItem("Tools/Toggle Inspector lock %l")]
    static public void ToggleInspectorLock()
    {
        var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");

        var isLocked = inspectorType.GetProperty("isLocked",System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

        var inspectorWindow = EditorWindow.GetWindow(inspectorType);

        var state = isLocked.GetGetMethod().Invoke(inspectorWindow, new object[] { });

        isLocked.GetSetMethod().Invoke(inspectorWindow, new object[] { !(bool)state });
    }
}