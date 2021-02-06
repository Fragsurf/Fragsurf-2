using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ModTool.Shared;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor.Build.Reporting;
using ModTool.Shared.Editor;
using System.Diagnostics;

namespace Fragsurf
{
    public class BuildStandaloneMod
    {

        [MenuItem("Fragsurf/Mod/Experimental/Build Standalone")]
        public static void BuildStandloneMod() 
        {

            var targetPath = EditorUtility.SaveFilePanel("Choose Location", "", "Mod", "exe");

            if (string.IsNullOrEmpty(targetPath))
            {
                return;
            }

            var warpPacker = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("warp-packer")[0]);
            var warpPath = Path.Combine(Directory.GetCurrentDirectory(), warpPacker);
            var levels = new string[] { SceneManager.GetActiveScene().path };

            var targetFileName = Path.GetFileName(targetPath);
            var tempBuildFolder = Path.Combine(Application.temporaryCachePath, "standalone-mod-build");
            if (Directory.Exists(tempBuildFolder))
            {
                Directory.Delete(tempBuildFolder, true);
            }
            Directory.CreateDirectory(tempBuildFolder);

            var tempBuildPath = Path.Combine(tempBuildFolder, targetFileName);

            BuildPipeline.BuildPlayer(levels, tempBuildPath, BuildTarget.StandaloneWindows64, BuildOptions.None);

            Process.Start(warpPath, $"--arch windows-x64 --input_dir {tempBuildFolder} --exec {targetFileName} --output warp.exe")
                .WaitForExit();
            var warpPackagePath = Path.Combine(Directory.GetCurrentDirectory(), "warp.exe");
            if (File.Exists(warpPackagePath))
            {
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                File.Copy(warpPackagePath, targetPath);
                File.Delete(warpPackagePath);
            }
        }

    }
}

