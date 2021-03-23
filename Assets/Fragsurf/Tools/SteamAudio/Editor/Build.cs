﻿//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace SteamAudio
{

    //
    // Build
    // Command-line build system for Steam Audio.
    //

    public static class Build
    {
        //
        // Steam Audio scripts
        //

        static string Scripts = "Assets/SteamAudio";

        //
        // Steam Audio plugins
        //

        static string[] Plugins =
        {
            "Assets/Plugins/x86/phonon.dll",
            "Assets/Plugins/x86/audioplugin_phonon.dll",
            "Assets/Plugins/x86_64/phonon.dll",
            "Assets/Plugins/x86_64/audioplugin_phonon.dll",
            "Assets/Plugins/phonon.bundle",
            "Assets/Plugins/audioplugin_phonon.bundle",
            "Assets/Plugins/x86/libphonon.so",
            "Assets/Plugins/x86/libaudioplugin_phonon.so",
            "Assets/Plugins/x86_64/libphonon.so",
            "Assets/Plugins/x86_64/libaudioplugin_phonon.so",
            "Assets/Plugins/android/libs/armv7/libphonon.so",
            "Assets/Plugins/android/libs/armv7/libaudioplugin_phonon.so",
            "Assets/Plugins/android/libs/arm64/libphonon.so",
            "Assets/Plugins/android/libs/arm64/libaudioplugin_phonon.so",
            "Assets/Plugins/android/libs/x86/libphonon.so",
            "Assets/Plugins/android/libs/x86/libaudioplugin_phonon.so"
        };

        static string[] FMODStudioPlugins =
        {
            "Assets/Plugins/x86/phonon_fmod.dll",
            "Assets/Plugins/x86_64/phonon_fmod.dll",
            "Assets/Plugins/x86/libphonon_fmod.so",
            "Assets/Plugins/x86_64/libphonon_fmod.so",
            "Assets/Plugins/phonon_fmod.bundle",
            "Assets/Plugins/android/libs/armv7/libphonon_fmod.so",
            "Assets/Plugins/android/libs/arm64/libphonon_fmod.so",
            "Assets/Plugins/android/libs/x86/libphonon_fmod.so"
        };

        static string[] TrueAudioNextPlugins =
        {
            "Assets/Plugins/x86_64/tanrt64.dll",
            "Assets/Plugins/x86_64/GPUUtilities.dll"
        };

        static string[] RadeonRaysPlugins =
        {
            "Assets/Plugins/x86_64/RadeonRays.dll",
            "Assets/Plugins/x86_64/GPUUtilities.dll"
        };

        static string FMODStudioAudioEngineSuffix = "_FMODStudio";

        public static string[] FilteredAssets(string directory, string[] excludeSuffixes, string includeOnlySuffix)
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory() + "/" + directory, "*", 
                SearchOption.AllDirectories);

            var assets = new List<string>();
            foreach (var file in files)
            {
                if (file.Contains(".meta") || file.Contains(".in"))
                    continue;

                if (excludeSuffixes != null)
                {
                    var fileExcluded = false;
                    foreach (var suffix in excludeSuffixes)
                    {
                        if (file.Contains(suffix))
                        {
                            fileExcluded = true;
                            break;
                        }
                    }
                    if (fileExcluded)
                        continue;
                }

                if (includeOnlySuffix != null && !file.Contains(includeOnlySuffix))
                    continue;

                var relativeName = file.Replace(Directory.GetCurrentDirectory() + "/", "");
                relativeName = relativeName.Replace('\\', '/');

                assets.Add(relativeName);
            }

            return assets.ToArray();
        }

        //
        // BuildAssetList
        // Builds an asset list given an array of asset groups.
        //
        static string[] BuildAssetList(string[][] assetGroups)
        {
            int numAssets = 0;
            foreach (string[] assetGroup in assetGroups)
                numAssets += assetGroup.Length;

            string[] assets = new string[numAssets];

            int offset = 0;
            foreach (string[] assetGroup in assetGroups)
            {
                Array.Copy(assetGroup, 0, assets, offset, assetGroup.Length);
                offset += assetGroup.Length;
            }

            return assets;
        }

        //
        // BuildSteamAudio
        // Builds a Unity package for Steam Audio.
        //
        public static void BuildSteamAudio()
        {
            var unityScripts = FilteredAssets(Scripts, new string[] { FMODStudioAudioEngineSuffix }, null);

            string[][] assetGroups = { unityScripts, Plugins };
            string[] assets = BuildAssetList(assetGroups);

            AssetDatabase.ExportPackage(assets, "SteamAudio.unitypackage", ExportPackageOptions.Recurse);
        }

        public static void BuildSteamAudioFMODStudio()
        {
            var fmodScripts = FilteredAssets(Scripts, null, FMODStudioAudioEngineSuffix);

            var assetGroups = new string[][] { fmodScripts, FMODStudioPlugins };
            var assets = BuildAssetList(assetGroups);

            AssetDatabase.ExportPackage(assets, "SteamAudio_FMODStudio.unitypackage", ExportPackageOptions.Recurse);
        }

        public static void BuildSteamAudioTrueAudioNext()
        {
            var assetGroups = new string[][] { TrueAudioNextPlugins };
            var assets = BuildAssetList(assetGroups);

            AssetDatabase.ExportPackage(assets, "SteamAudio_TrueAudioNext.unitypackage", ExportPackageOptions.Recurse);
        }

        public static void BuildSteamAudioRadeonRays()
        {
            var assetGroups = new string[][] { RadeonRaysPlugins };
            var assets = BuildAssetList(assetGroups);

            AssetDatabase.ExportPackage(assets, "SteamAudio_RadeonRays.unitypackage", ExportPackageOptions.Recurse);
        }
    }
}