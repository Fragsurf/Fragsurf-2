﻿//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using System;
using System.Runtime.InteropServices;

namespace SteamAudio
{
    //
    // Phonon API functions.
    //
    public static class PhononCore
    {
        [DllImport("phonon")]
        public static extern void iplGetVersion([In, Out] ref uint major,
                                                [In, Out] ref uint minor,
                                                [In, Out] ref uint patch);

        [DllImport("phonon")]
        public static extern Error iplCreateContext(LogCallback logCallback, IntPtr allocateCallback, IntPtr freeCallback, [In, Out] ref IntPtr context);

        [DllImport("phonon")]
        public static extern void iplDestroyContext([In, Out] ref IntPtr context);

        [DllImport("phonon")]
        public static extern void iplCleanup();

        //
        // Functions for OpenCL compute devices.
        //

        [DllImport("phonon")]
        public static extern Error iplCreateComputeDevice(IntPtr globalContext, ComputeDeviceFilter deviceFilter, 
            [In, Out] ref IntPtr device);

        [DllImport("phonon")]
        public static extern void iplDestroyComputeDevice([In, Out] ref IntPtr device);

        //
        // Scene export callback functions.
        //

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LoadSceneProgressCallback(float progress);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FinalizeSceneProgressCallback(float progress);

        //
        // Functions for scene export.
        //

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ClosestHitCallback(IntPtr origin,
                                                IntPtr direction, 
                                                float minDistance, 
                                                float maxDistance, 
                                                [In, Out] ref float hitDistance, 
                                                [In, Out] ref Vector3 hitNormal, 
                                                [In, Out] ref IntPtr hitMaterial, 
                                                IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AnyHitCallback(IntPtr origin, 
                                            IntPtr direction, 
                                            float minDistance, 
                                            float maxDistance, 
                                            [In, Out] ref int hitExists, 
                                            IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void BatchedClosestHitCallback(int numRays, Vector3[] origins, Vector3[] directions,
            int rayStride, float[] minDistances, float[] maxDistances, float[] hitDistances, Vector3[] hitNormals,
            IntPtr hitMaterials, int hitStride, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void BatchedAnyHitCallback(int numRays, Vector3[] origins, Vector3[] directions, int rayStride,
            float[] minDistances, float[] maxDistances, byte[] hitExists, IntPtr userData);

        [DllImport("phonon")]
        public static extern Error iplCreateScene(IntPtr context, IntPtr computeDevice,
            SceneType sceneType, int numMaterials, Material[] materials,
            ClosestHitCallback closestHitCallback, AnyHitCallback anyHitCallback, 
            BatchedClosestHitCallback batchedClosestHitCallback, BatchedAnyHitCallback batchedAnyHitCallback, 
            IntPtr userData, [In, Out] ref IntPtr scene);

        [DllImport("phonon")]
        public static extern void iplDestroyScene([In, Out] ref IntPtr scene);

        [DllImport("phonon")]
        public static extern Error iplCreateStaticMesh(IntPtr scene, int numVertices, int numTriangles, 
            Vector3[] vertices, Triangle[] triangles, int[] materialIndices, [In, Out] ref IntPtr staticMesh);

        [DllImport("phonon")]
        public static extern void iplDestroyStaticMesh([In, Out] ref IntPtr staticMesh);

        [DllImport("phonon")]
        public static extern int iplSaveScene(IntPtr scene, [In, Out] byte[] data);

        [DllImport("phonon", CallingConvention = CallingConvention.Cdecl)]
        public static extern Error iplLoadScene(IntPtr globalContext, 
            SceneType sceneType, byte[] data, int size, IntPtr computeDevice, 
            LoadSceneProgressCallback progressCallback, [In, Out] ref IntPtr scene);

        [DllImport("phonon")]
        public static extern void iplSaveSceneAsObj(IntPtr scene, byte[] fileName);

        //
        // Instanced Mesh
        //

        [DllImport("phonon", CallingConvention = CallingConvention.Cdecl)]
        public static extern Error iplCreateInstancedMesh(IntPtr scene, IntPtr instancedScene, Matrix4x4 transform,
            [In, Out] ref IntPtr instancedMesh);

        [DllImport("phonon", CallingConvention = CallingConvention.Cdecl)]
        public static extern void iplDestroyInstancedMesh([In, Out] ref IntPtr instancedMesh);

        [DllImport("phonon", CallingConvention = CallingConvention.Cdecl)]
        public static extern void iplAddInstancedMesh(IntPtr scene, IntPtr instancedMesh);

        [DllImport("phonon", CallingConvention = CallingConvention.Cdecl)]
        public static extern void iplRemoveInstancedMesh(IntPtr scene, IntPtr instancedMesh);

        [DllImport("phonon", CallingConvention = CallingConvention.Cdecl)]
        public static extern void iplUpdateInstancedMeshTransform(IntPtr instancedMesh, Matrix4x4 transform);

        [DllImport("phonon", CallingConvention = CallingConvention.Cdecl)]
        public static extern void iplCommitScene(IntPtr scene);


        //
        // Functions to setup Environment.
        //

        [DllImport("phonon")]
        public static extern Error iplCreateEnvironment(IntPtr globalContext, IntPtr computeDevice, SimulationSettings simulationSettings, IntPtr scene, IntPtr probeManager, [In, Out] ref IntPtr environment);

        [DllImport("phonon")]
        public static extern void iplDestroyEnvironment([In, Out] ref IntPtr environment);

        [DllImport("phonon")]
        public static extern void iplSetNumBounces(IntPtr environment, int numBounces);

        //
        // Functions related to Audio Buffers.
        //

        [DllImport("phonon")]
        public static extern void iplMixAudioBuffers(int numBuffers, AudioBuffer[] inputAudio, AudioBuffer outputAudio);

        [DllImport("phonon")]
        public static extern void iplInterleaveAudioBuffer(AudioBuffer inputAudio, AudioBuffer outputAudio);

        [DllImport("phonon")]
        public static extern void iplDeinterleaveAudioBuffer(AudioBuffer inputAudio, AudioBuffer outputAudio);

        [DllImport("phonon")]
        public static extern void iplConvertAudioBufferFormat(AudioBuffer inputAudio, AudioBuffer outputAudio);

        [DllImport("phonon")]
        public static extern Error iplCreateAmbisonicsRotator(IntPtr globalContext, int order, [In, Out] ref IntPtr rotator);

        [DllImport("phonon")]
        public static extern void iplDestroyAmbisonicsRotator([In, Out] ref IntPtr rotator);

        [DllImport("phonon")]
        public static extern void iplSetAmbisonicsRotation(IntPtr rotator, Vector3 listenerAhead, Vector3 listenerUp);

        [DllImport("phonon")]
        public static extern void iplRotateAmbisonicsAudioBuffer(IntPtr rotator, AudioBuffer inputAudio, AudioBuffer outputAudio);

        [DllImport("phonon")]
        public static extern Vector3 iplCalculateRelativeDirection(Vector3 sourcePosition, Vector3 listenerPosition, Vector3 listenerAhead, Vector3 listenerUp);

        //
        // Functions for Environment renderer.
        //

        [DllImport("phonon")]
        public static extern Error iplCreateEnvironmentalRenderer(IntPtr globalContext, IntPtr environment,
            RenderingSettings renderingSettings, AudioFormat outputFormat, IntPtr threadCreateCallback,
            IntPtr threadDestroyCallback, [In, Out] ref IntPtr renderer);

        [DllImport("phonon")]
        public static extern void iplDestroyEnvironmentalRenderer([In, Out] ref IntPtr renderer);

        //
        // Direct Sound.
        //

        [DllImport("phonon")]
        public static extern DirectSoundPath iplGetDirectSoundPath(IntPtr environment, 
                                                                   Vector3 listenerPosition, 
                                                                   Vector3 listenerAhead, 
                                                                   Vector3 listenerUp, 
                                                                   Source source,
                                                                   float sourceRadius, 
                                                                   int numSamples,
                                                                   OcclusionMode occlusionMode, 
                                                                   OcclusionMethod occlusionMethod);

        //
        // Convolution Effect.
        //

        [DllImport("phonon")]
        public static extern Error iplCreateConvolutionEffect(IntPtr renderer, BakedDataIdentifier identifier, SimulationType simulationType, AudioFormat inputFormat, AudioFormat outputFormat, [In, Out] ref IntPtr effect);

        [DllImport("phonon")]
        public static extern void iplDestroyConvolutionEffect([In, Out] ref IntPtr effect);

        [DllImport("phonon")]
        public static extern void iplSetConvolutionEffectIdentifier(IntPtr effect, BakedDataIdentifier identifier);

        [DllImport("phonon")]
        public static extern void iplSetDryAudioForConvolutionEffect(IntPtr effect, Vector3 sourcePosition, AudioBuffer dryAudio);

        [DllImport("phonon")]
        public static extern void iplGetWetAudioForConvolutionEffect(IntPtr effect, Vector3 listenerPosition, Vector3 listenerAhead, Vector3 listenerUp, AudioBuffer wetAudio);

        [DllImport("phonon")]
        public static extern void iplGetMixedEnvironmentalAudio(IntPtr renderer, Vector3 listenerPosition, Vector3 listenerAhead, Vector3 listenerUp, AudioBuffer mixedWetAudio);

        [DllImport("phonon")]
        public static extern void iplFlushConvolutionEffect(IntPtr effect);

        //
        // Acoustic Probes callback.
        //

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ProbePlacementProgressCallback(float progress);

        //
        // Functions for creating and managing Acoustic Probes for baking.
        //

        [DllImport("phonon")]
        public static extern Error iplCreateProbeBox(IntPtr context, IntPtr scene, float[] boxLocalToWorldTransform, ProbePlacementParameters placementParams, ProbePlacementProgressCallback progressCallback, [In, Out] ref IntPtr probeBox);

        [DllImport("phonon")]
        public static extern void iplDestroyProbeBox([In, Out] ref IntPtr probeBox);

        [DllImport("phonon")]
        public static extern int iplGetProbeSpheres(IntPtr probeBox, [In, Out] Sphere[] probeSpheres);

        [DllImport("phonon")]
        public static extern int iplSaveProbeBox(IntPtr probeBox, [In, Out] Byte[] data);

        [DllImport("phonon")]
        public static extern Error iplLoadProbeBox(IntPtr context, Byte[] data, int size, [In, Out] ref IntPtr probeBox);

        [DllImport("phonon")]
        public static extern Error iplCreateProbeBatch(IntPtr context, [In, Out] ref IntPtr probeBatch);

        [DllImport("phonon")]
        public static extern void iplDestroyProbeBatch([In, Out] ref IntPtr probeBatch);

        [DllImport("phonon")]
        public static extern void iplAddProbeToBatch(IntPtr probeBatch, IntPtr probeBox, int probeIndex);

        [DllImport("phonon")]
        public static extern void iplFinalizeProbeBatch(IntPtr probeBatch);

        [DllImport("phonon")]
        public static extern int iplSaveProbeBatch(IntPtr probeBatch, [In, Out] Byte[] data);

        [DllImport("phonon")]
        public static extern Error iplLoadProbeBatch(IntPtr context, Byte[] data, int size, [In, Out] ref IntPtr probeBatch);

        [DllImport("phonon")]
        public static extern Error iplCreateProbeManager(IntPtr context, [In, Out] ref IntPtr probeManager);

        [DllImport("phonon")]
        public static extern void iplDestroyProbeManager([In, Out] ref IntPtr probeManager);

        [DllImport("phonon")]
        public static extern void iplAddProbeBatch(IntPtr probeManager, IntPtr probeBatch);

        [DllImport("phonon")]
        public static extern void iplRemoveProbeBatch(IntPtr probeManager, IntPtr probeBatch);

        //
        // Baking related callback.
        //

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void BakeProgressCallback(float progress);

        //
        // Functions for baking environmental effects.
        //

        [DllImport("phonon")]
        public static extern void iplBakeReverb(IntPtr environment, IntPtr probeBox, BakingSettings bakingSettings, BakeProgressCallback progressCallback);

        [DllImport("phonon")]
        public static extern void iplBakePropagation(IntPtr environment, IntPtr probeBox, Sphere sourceInfluence, BakedDataIdentifier sourceIdentifier, BakingSettings bakingSettings, BakeProgressCallback progressCallback);

        [DllImport("phonon")]
        public static extern void iplBakeStaticListener(IntPtr environment, IntPtr probeBox, Sphere listenerInfluence, BakedDataIdentifier listenerIdentifier, BakingSettings bakingSettings, BakeProgressCallback progressCallback);

        [DllImport("phonon")]
        public static extern void iplCancelBake();

        [DllImport("phonon")]
        public static extern void iplDeleteBakedDataByIdentifier(IntPtr probeBox, BakedDataIdentifier identifier);

        [DllImport("phonon")]
        public static extern int iplGetBakedDataSizeByIdentifier(IntPtr probeBox, BakedDataIdentifier identifier);
    }
}
