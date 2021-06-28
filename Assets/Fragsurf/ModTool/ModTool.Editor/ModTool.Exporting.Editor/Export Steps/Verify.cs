using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using ModTool.Shared;
using ModTool.Shared.Verification;
using DTCommandPalette;

namespace ModTool.Exporting.Editor
{
    public class Verify : ExportStep
    {
        public override string message { get { return "Verifying Project"; } }

        internal override void Execute(ExportData data)
        {
            CheckSerializationMode();
            VerifyProject();
            VerifySettings();
        }

        private void CheckSerializationMode()
        {
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                LogUtility.LogInfo("Changed serialization mode from " + EditorSettings.serializationMode + " to Force Text");
                EditorSettings.serializationMode = SerializationMode.ForceText;
            }
        }

        private void VerifyProject()
        {
            if (!ModToolSettings.VerifyVersion())
            {
                throw new Exception("Mods for " + ModToolSettings.productName + " can only be exported with Unity " + ModToolSettings.unityVersion);
            }
            
            if (Application.isPlaying)
                throw new Exception("Unable to export mod in play mode");

            if (ModToolSettings.supportedContent.HasFlag(ModContent.Code) && !VerifyAssemblies())
                throw new Exception("Incompatible scripts or assemblies found");           
        }

        private void VerifySettings()
        {
            if (string.IsNullOrEmpty(ExportSettings.name))
                throw new Exception("Mod has no name");

            if (ExportSettings.scene == null)
                throw new Exception("No scene specified in export menu");

            if (string.IsNullOrEmpty(ExportSettings.outputDirectory))            
                throw new Exception("No output directory set");             

            if (!Directory.Exists(ExportSettings.outputDirectory))            
                throw new Exception("Output directory " + ExportSettings.outputDirectory + " does not exist");
            
            if (ExportSettings.platforms == 0)            
                throw new Exception("No platforms selected");            

            if (ExportSettings.content == 0)            
                throw new Exception("No content selected");
        }

        private static bool VerifyAssemblies()
        {
            List<string> assemblies = AssemblyUtility.GetAssemblies(assetsDirectory, AssemblyFilter.ModAssemblies);

            foreach (string scriptAssembly in scriptAssemblies)
            {
                string scriptAssemblyFile = Path.Combine(assemblyDirectory, scriptAssembly);

                if (File.Exists(scriptAssemblyFile))                
                    assemblies.Add(scriptAssemblyFile);                
            }

            List<string> messages = new List<string>();

            AssemblyVerifier.VerifyAssemblies(assemblies, messages);

            foreach (var message in messages)
                LogUtility.LogWarning(message);

            if (messages.Count > 0)
                return false;

            return true;            
        }

        [MethodCommand("Mod/Verify Scripts")]
        [MenuItem("Fragsurf/Mod/Verify Scripts")]
        public static void VerifyScriptsMenuItem()
        {
            if (VerifyAssemblies())
                LogUtility.LogInfo("Scripts Verified!");
            else
                LogUtility.LogWarning("Scripts Not verified!");
        }
    }
}