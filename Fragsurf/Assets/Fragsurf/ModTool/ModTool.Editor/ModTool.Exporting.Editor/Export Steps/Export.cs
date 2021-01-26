using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using ModTool.Shared;

namespace ModTool.Exporting.Editor
{
    public class Export : ExportStep
    {
        public override string message { get { return "Exporting Files"; } }

        private string tempModDirectory;
        private string modDirectory;

        internal override void Execute(ExportData data)
        {
            tempModDirectory = Path.Combine("Temp", ExportSettings.name);
            modDirectory = Path.Combine(ExportSettings.outputDirectory, ExportSettings.name);

            if (Directory.Exists(tempModDirectory))
                Directory.Delete(tempModDirectory, true);

            Directory.CreateDirectory(tempModDirectory);

            foreach (Asset assembly in data.assemblies)
                assembly.Copy(tempModDirectory);

            foreach (Asset assembly in data.scriptAssemblies)
                assembly.Copy(tempModDirectory);

            ModPlatform platforms = ExportSettings.platforms;

            BuildAssetBundles(platforms);

            ModInfo modInfo = new ModInfo(
                ExportSettings.name,
                ExportSettings.scene.ScenePath,
                ExportSettings.author,
                ExportSettings.description,
                ExportSettings.version,
                Application.unityVersion,
                platforms,
                data.content);

            ModInfo.Save(Path.Combine(tempModDirectory, ExportSettings.name + ".info"), modInfo);

            CopyToOutput();

            if (data.scriptAssemblies.Count > 0)
                ForceAssemblyReload();
        }

        private void BuildAssetBundles(ModPlatform platforms)
        {
            List<BuildTarget> buildTargets = platforms.GetBuildTargets();

            foreach (BuildTarget buildTarget in buildTargets)
            {
                string platformSubdirectory = Path.Combine(tempModDirectory, buildTarget.GetModPlatform().ToString());
                Directory.CreateDirectory(platformSubdirectory);
                BuildPipeline.BuildAssetBundles(platformSubdirectory, BuildAssetBundleOptions.None, buildTarget);
            }            
        }

        private void CopyToOutput()
        {
            try
            {
                if (Directory.Exists(modDirectory))
                    Directory.Delete(modDirectory, true);

                CopyAll(tempModDirectory, modDirectory);

                LogUtility.LogInfo("Export complete");
            }
            catch (Exception e)
            {
                LogUtility.LogWarning("There was an issue while copying the mod to the output folder. " + e.Message);
            }
        }

        private static void CopyAll(string sourceDirectory, string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);

            foreach (string file in Directory.GetFiles(sourceDirectory))
            {
                string fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(targetDirectory, fileName), true);
            }

            foreach (string subDirectory in Directory.GetDirectories(sourceDirectory))
            {
                string targetSubDirectory = Path.Combine(targetDirectory, Path.GetFileName(subDirectory));
                CopyAll(subDirectory, targetSubDirectory);
            }
        }
    }
}