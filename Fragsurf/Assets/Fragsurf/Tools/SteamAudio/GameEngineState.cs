﻿//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using System;
using System.Collections.Generic;
using System.IO;
using AOT;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteamAudio
{
    public enum GameEngineStateInitReason
    {
        ExportingScene,
        GeneratingProbes,
        EditingProbes,
        Baking,
        Playing
    }

    public class GameEngineState
    {
        public void Initialize(byte[] data, SimulationSettingsValue settings, ComponentCache componentCache,
            GameEngineStateInitReason reason)
        {
            PhononCore.iplCreateContext(LogMessage, IntPtr.Zero, IntPtr.Zero, ref context);

            if (reason != GameEngineStateInitReason.EditingProbes)
            {
                var customSettings = componentCache.SteamAudioCustomSettings();

                var useOpenCL = false;
                var computeDeviceType = ComputeDeviceType.Any;
                var maxCUsToReserve = 0;
                var fractionCUsForIRUpdate = .0f;

                convolutionType = ConvolutionOption.Phonon;
                var rayTracer = SceneType.Phonon;

                // TAN is enabled for realtime.
                if (customSettings && customSettings.ConvolutionType() == ConvolutionOption.TrueAudioNext 
                    && reason == GameEngineStateInitReason.Playing)
                {
                    convolutionType = customSettings.ConvolutionType();

                    useOpenCL = true;
                    computeDeviceType = ComputeDeviceType.GPU;
                    maxCUsToReserve = customSettings.maxComputeUnitsToReserve;
                    fractionCUsForIRUpdate = customSettings.fractionComputeUnitsForIRUpdate;
                }

                // Enable some settings which are commong whether Radeon Rays is enabled for baking or realtime.
                if (customSettings 
                    && (reason == GameEngineStateInitReason.Baking 
                    || reason == GameEngineStateInitReason.GeneratingProbes 
                    || reason == GameEngineStateInitReason.Playing))
                {
                    if (customSettings.RayTracerType() != SceneType.RadeonRays)
                    {
                        rayTracer = customSettings.RayTracerType();
                    }
                    else
                    {
                        useOpenCL = true;
                        rayTracer = SceneType.RadeonRays;
                        computeDeviceType = ComputeDeviceType.GPU;
                    }
                }

                // Enable additional settings when Radeon Rays is enabled for realtime but TAN is not.
                if (customSettings && customSettings.RayTracerType() == SceneType.RadeonRays 
                    && customSettings.ConvolutionType() != ConvolutionOption.TrueAudioNext 
                    && reason == GameEngineStateInitReason.Playing)
                {
                    maxCUsToReserve = customSettings.maxComputeUnitsToReserve;
                    fractionCUsForIRUpdate = 1.0f;
                }

                try
                {
                    var deviceFilter = new ComputeDeviceFilter
                    {
                        type = computeDeviceType,
                        maxCUsToReserve = maxCUsToReserve,
                        fractionCUsForIRUpdate = fractionCUsForIRUpdate
                    };

                    computeDevice.Create(context, useOpenCL, deviceFilter);
                }
                catch (Exception e)
                {
                    if (customSettings && convolutionType == ConvolutionOption.TrueAudioNext) 
                    {
                        var eInEditor = !SteamAudioManager.IsAudioEngineInitializing();
                        if (eInEditor && (!File.Exists(Directory.GetCurrentDirectory() + "/Assets/Plugins/x86_64/tanrt64.dll"))) 
                        {
                            throw new Exception(
                                "Steam Audio configured to use TrueAudio Next, but TrueAudio Next support package " +
                                "not installed. Please import SteamAudio_TrueAudioNext.unitypackage in order to use " +
                                "TrueAudio Next support for Steam Audio.");
                        }
                        else
                        {
                            Debug.LogWarning(String.Format("Unable to create compute device: {0}. Using Phonon convolution and raytracer.",
                                e.Message));
                        }
                    }
                    else 
                    {
                        Debug.LogWarning(String.Format("Unable to create compute device: {0}. Using Phonon convolution and raytracer.",
                            e.Message));
                    }

                    convolutionType = ConvolutionOption.Phonon;
                    rayTracer = SceneType.Phonon;
                }

                var inEditor = !SteamAudioManager.IsAudioEngineInitializing();

                var maxSources = settings.MaxSources;
                if (customSettings && convolutionType == ConvolutionOption.TrueAudioNext) {
                    maxSources = customSettings.MaxSources;
                }
                if (rayTracer == SceneType.RadeonRays && reason == GameEngineStateInitReason.Baking) {
                    maxSources = customSettings.BakingBatchSize;
                }

                simulationSettings = new SimulationSettings
                {
                    sceneType               = rayTracer,
                    maxOcclusionSamples     = settings.MaxOcclusionSamples,
                    rays                    = (inEditor) ? settings.BakeRays : settings.RealtimeRays,
                    secondaryRays           = (inEditor) ? settings.BakeSecondaryRays : settings.RealtimeSecondaryRays,
                    bounces                 = (inEditor) ? settings.BakeBounces : settings.RealtimeBounces,
                    threads                 = (inEditor) ? (int) Mathf.Max(1, (settings.BakeThreadsPercentage * SystemInfo.processorCount) / 100.0f) : (int) Mathf.Max(1, (settings.RealtimeThreadsPercentage * SystemInfo.processorCount) / 100.0f),
                    irDuration              = (customSettings && convolutionType == ConvolutionOption.TrueAudioNext) ? customSettings.Duration : settings.Duration,
                    ambisonicsOrder         = (customSettings && convolutionType == ConvolutionOption.TrueAudioNext) ? customSettings.AmbisonicsOrder : settings.AmbisonicsOrder,
                    maxConvolutionSources   = maxSources,
                    bakingBatchSize         = (rayTracer == SceneType.RadeonRays) ? customSettings.BakingBatchSize : 1,
                    irradianceMinDistance   = settings.IrradianceMinDistance
                };

#if UNITY_EDITOR
                if (customSettings) {
                    if (rayTracer == SceneType.RadeonRays) {
                        if (!File.Exists(Directory.GetCurrentDirectory() + "/Assets/Plugins/x86_64/RadeonRays.dll")) {
                            throw new Exception(
                                "Steam Audio configured to use Radeon Rays, but Radeon Rays support package not " +
                                "installed. Please import SteamAudio_RadeonRays.unitypackage in order to use Radeon " +
                                "Rays support for Steam Audio.");
                        }
                    }

                    if (convolutionType == ConvolutionOption.TrueAudioNext) {
                        if (!File.Exists(Directory.GetCurrentDirectory() + "/Assets/Plugins/x86_64/tanrt64.dll")) {
                            throw new Exception(
                                "Editor: Steam Audio configured to use TrueAudio Next, but TrueAudio Next support package " +
                                "not installed. Please import SteamAudio_TrueAudioNext.unitypackage in order to use " +
                                "TrueAudio Next support for Steam Audio.");
                        }
                    }
                }
#endif

                if (reason != GameEngineStateInitReason.ExportingScene)
                {
                    scene.Create(data, computeDevice, simulationSettings, context);
                }

                // Add other scenes in the hierarchy
                if (scene.GetScene() != IntPtr.Zero 
                    && (reason == GameEngineStateInitReason.GeneratingProbes || reason == GameEngineStateInitReason.Baking))
                {
                    for (int i = 0; i < SceneManager.sceneCount; ++i)
                    {
                        var unityScene = SceneManager.GetSceneAt(i);
                        if (!unityScene.isLoaded)
                            continue;

                        if (unityScene == SceneManager.GetActiveScene())
                            continue;

                        IntPtr additiveScene, additiveMesh;
                        var error = scene.AddAdditiveScene(unityScene, scene.GetScene(), 
                            computeDevice, simulationSettings, context,  out additiveScene, out additiveMesh);

                        if (error != Error.None)
                            continue;

                        if (additiveScenes == null)
                            editorAdditiveScenes = new List<IntPtr>();
                        editorAdditiveScenes.Add(additiveScene);

                        if (editorAdditiveSceneMeshes == null)
                            editorAdditiveSceneMeshes = new List<IntPtr>();
                        editorAdditiveSceneMeshes.Add(additiveMesh);
                    }

                    PhononCore.iplCommitScene(scene.GetScene());
                }

                if (reason == GameEngineStateInitReason.Playing)
                    probeManager.Create(context);

                if (reason != GameEngineStateInitReason.ExportingScene &&
                    reason != GameEngineStateInitReason.GeneratingProbes)
                {
                    try
                    {
                        environment.Create(computeDevice, simulationSettings, scene, probeManager, context);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }
        }

        public void Destroy()
        {
            if (editorAdditiveSceneMeshes != null)
            {
                for (int i = 0; i < editorAdditiveSceneMeshes.Count; ++i)
                {
                    IntPtr additiveMesh = editorAdditiveSceneMeshes[i];
                    PhononCore.iplRemoveInstancedMesh(scene.GetScene(), additiveMesh);
                    PhononCore.iplDestroyInstancedMesh(ref additiveMesh);
                }

                editorAdditiveSceneMeshes.Clear();
                editorAdditiveSceneMeshes = null;
            }

            if (editorAdditiveScenes != null)
            {
                for (int i = 0; i < editorAdditiveScenes.Count; ++i)
                {
                    IntPtr additiveScene = editorAdditiveScenes[i];
                    PhononCore.iplDestroyScene(ref additiveScene);
                }

                editorAdditiveScenes.Clear();
                editorAdditiveScenes = null;
            }

            environment.Destroy();
            probeManager.Destroy();
            scene.Destroy();
            computeDevice.Destroy();

            PhononCore.iplDestroyContext(ref context);
        }

        public IntPtr Context()
        {
            return context;
        }

        public ComputeDevice ComputeDevice()
        {
            return computeDevice;
        }

        public SimulationSettings SimulationSettings()
        {
            return simulationSettings;
        }

        public Scene Scene()
        {
            return scene;
        }

        public ProbeManager ProbeManager()
        {
            return probeManager;
        }

        public Environment Environment()
        {
            return environment;
        }

        public ConvolutionOption ConvolutionType()
        {
            return convolutionType;
        }

        public void ExportScene(MaterialValue defaultMaterial, bool exportOBJ)
        {
            try
            {
                scene.Export(computeDevice, defaultMaterial, context, exportOBJ);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public void BuildScene(MaterialValue defaultMaterial, out byte[] data)
        {
            data = null;   
            try
            {
                scene.Build(computeDevice, defaultMaterial, context, out data);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        [MonoPInvokeCallback(typeof(LogCallback))]
        static void LogMessage(string message)
        {
            Debug.Log(message);
        }

        IntPtr              context;
        ComputeDevice       computeDevice       = new ComputeDevice();
        SimulationSettings  simulationSettings;
        Scene               scene               = new Scene();
        ProbeManager        probeManager        = new ProbeManager();
        Environment         environment         = new Environment();
        ConvolutionOption   convolutionType     = ConvolutionOption.Phonon;

        // Scene instances for dynamic objects at runtime.
        public Dictionary<string, IntPtr> instancedScenes = null;

        // Scene instances for additive scenes at runtime.
        public Dictionary<UnityEngine.SceneManagement.Scene, IntPtr> additiveScenes = null;
        public Dictionary<UnityEngine.SceneManagement.Scene, IntPtr> additiveSceneMeshes = null;

        // Scene instances for additive scenes during editing. This data is destroyed by GameEngineState
        // as opposed to Steam Audio Manager.
        List<IntPtr> editorAdditiveScenes = null;
        List<IntPtr> editorAdditiveSceneMeshes = null;
    }
}