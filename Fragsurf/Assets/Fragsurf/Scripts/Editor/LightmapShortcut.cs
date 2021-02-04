using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealtimeCSG.Components;
using InternalRealtimeCSG;

public class LightmapShortcut
{

    private static LightingSettings SuperFast = new LightingSettings
    {
        lightmapper = LightingSettings.Lightmapper.Enlighten,
        ao = true,
        lightmapMaxSize = 512,
        compressLightmaps = true,
        finalGather = false,
        indirectResolution = 1,
        lightmapResolution = 8,
        aoMaxDistance = .5f,
        aoExponentDirect = 1,
        aoExponentIndirect = 2f,
        directionalityMode = LightmapsMode.CombinedDirectional,
        albedoBoost = 1,
        indirectScale = 1,
        bakedGI = true,
        mixedBakeMode = MixedLightingMode.Subtractive
    };

    private static LightingSettings Normal = new LightingSettings
    {
        lightmapper = LightingSettings.Lightmapper.Enlighten,
        ao = true,
        lightmapMaxSize = 1024,
        compressLightmaps = true,
        finalGather = false,
        indirectResolution = 1,
        lightmapResolution = 20,
        aoMaxDistance = .5f,
        aoExponentDirect = 1,
        aoExponentIndirect = 2f,
        directionalityMode = LightmapsMode.CombinedDirectional,
        albedoBoost = 1,
        indirectScale = 1,
        bakedGI = true,
        mixedBakeMode = MixedLightingMode.Subtractive
    };

    private static LightingSettings Dank = new LightingSettings
    {
        lightmapper = LightingSettings.Lightmapper.Enlighten,
        ao = true,
        lightmapMaxSize = 2048,
        compressLightmaps = true,
        finalGather = true,
        indirectResolution = 2,
        lightmapResolution = 40,
        aoMaxDistance = .5f,
        aoExponentDirect = 1,
        aoExponentIndirect = 2f,
        directionalityMode = LightmapsMode.CombinedDirectional,
        albedoBoost = 1,
        indirectScale = 1,
        bakedGI = true,
        mixedBakeMode = MixedLightingMode.Subtractive
    };

    [MenuItem("Fragsurf/Lighting/Bake Fast")]
    public static void BuildFast()
    {
        StartLightmapping(SuperFast);
    }

    [MenuItem("Fragsurf/Lighting/Bake Normal")]
    public static void BuildNormal()
    {
        StartLightmapping(Normal);
    }

    [MenuItem("Fragsurf/Lighting/Bake Dank")]
    public static void BuildDank()
    {
        StartLightmapping(Dank);
    }

    [MenuItem("Fragsurf/Lighting/Cancel Build")]
    public static void CancelBuild()
    {
        if (Lightmapping.isRunning)
        {
            Lightmapping.Cancel();
        }
    }

    private static void StartLightmapping(LightingSettings settings)
    {
        GenerateLightmapUVs();
        CancelBuild();
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
