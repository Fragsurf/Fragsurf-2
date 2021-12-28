using UnityEngine;
using UnityEditor;
using DTCommandPalette;

public class MiscShortcuts
{

    private static void SceneView_duringSceneGui(SceneView sceneView)
    {
        if (!Lightmapping.isRunning)
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
            return;
        }

        var label = GUI.skin.label;
        label.richText = true;
        label.alignment = TextAnchor.UpperCenter;

        var x = sceneView.camera.pixelWidth;
        var y = sceneView.camera.pixelHeight;

        var msg = $"Lightmap is baking!";

        Handles.BeginGUI();
        GUI.Label(new Rect(1, 1, x, y), $"<color=black><size=18>{msg}</size></color>", label);
        GUI.Label(new Rect(0, 0, x, y), $"<color=red><size=18>{msg}</size></color>", label);
        Handles.EndGUI();

    }

}
