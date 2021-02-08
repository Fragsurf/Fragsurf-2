using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealtimeCSG.Components;
using InternalRealtimeCSG;
using DTCommandPalette;

public class LightmapShortcut
{

    [MethodCommand("Lightmap/Bake Preview")]
    [MenuItem("Fragsurf/Lightmap/Bake (Preview)")]
    public static void BuildFast()
    {
        var settings = Resources.Load<LightingSettings>("Misc/Lightmap Fast");
        StartLightmapping(settings);
    }

    [MethodCommand("Lightmap/Bake Normal")]
    [MenuItem("Fragsurf/Lightmap/Bake (Normal)")]
    public static void BuildNormal()
    {
        var settings = Resources.Load<LightingSettings>("Misc/Lightmap Normal");
        StartLightmapping(settings);
    }

    [MethodCommand("Lightmap/Cancel")]
    [MenuItem("Fragsurf/Lightmap/Cancel")]
    public static void CancelBake()
    {
        if (Lightmapping.isRunning)
        {
            Lightmapping.Cancel();
        }
    }

    [MethodCommand("Lightmap/Clear Baked Data")]
    [MenuItem("Fragsurf/Lightmap/Clear Baked Data")]
    public static void ClearBakedData()
    {
        if(EditorUtility.DisplayDialog("Clear Baked Data", "Really clear baked lightmaps?", "Yes", "Cancel"))
        {
            Lightmapping.Clear();
            GenerateLightmapUVs();
        }
    }

    private static void StartLightmapping(LightingSettings settings)
    {
        GenerateLightmapUVs();
        Lightmapping.lightingSettings = settings;
        if (Lightmapping.BakeAsync())
        {
            SceneView.duringSceneGui += SceneView_duringSceneGui;
        }
    }

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

    private static void GenerateLightmapUVs()
    {
        foreach(var m in GameObject.FindObjectsOfType<CSGModel>())
        {
            MeshInstanceManager.GenerateLightmapUVsForModel(m);
        }
    }

}
