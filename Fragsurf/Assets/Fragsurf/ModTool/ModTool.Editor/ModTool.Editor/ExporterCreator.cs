using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEditor;
using ModTool.Shared;
using ModTool.Shared.Editor;
using ModTool.Exporting.Editor;
using Mediatonic.Tools;

//Note: ModTool uses an old version of Mono.Cecil in the editor
#pragma warning disable CS0618

namespace ModTool.Editor
{
    internal class ExporterCreator
    {

        private static List<string> _builtInPaths = new List<string>()
        {
            "Assets\\Fragsurf\\RealtimeCSG",
        };

        /// <summary>
        /// Create a mod exporter package for this game.
        /// </summary>
        [MenuItem("Fragsurf/ModTool/Create Exporter")]
        public static void CreateExporter()
        {
            CreateExporter(Directory.GetCurrentDirectory(), true);
        }

        /// <summary>
        /// Create a mod exporter package after building the game.
        /// </summary>
        [UnityEditor.Callbacks.PostProcessBuild]
        public static void CreateExporterPostBuild(BuildTarget target, string pathToBuiltProject)
        {
            pathToBuiltProject = Path.GetDirectoryName(pathToBuiltProject);

            CreateExporter(pathToBuiltProject);
        }
        
        private static void CreateExporter(string path, bool revealPackage = false)
        {
            var destinationFolder = EditorUtility.OpenFolderPanel("Choose Directory", Application.dataPath, "Modding Toolkit");
            if (string.IsNullOrEmpty(destinationFolder))
            {
                return;
            }

            LogUtility.LogInfo("Creating Exporter");

            UpdateSettings();

            var codeSettings = CodeSettings.instance;
            var modToolSettings = ModToolSettings.instance;

            string modToolDirectory = AssetUtility.GetModToolDirectory();
            string exporterPath = Path.Combine(modToolDirectory, Path.Combine("Editor", "ModTool.Exporting.Editor.dll"));
            string fileName = Path.Combine(path, "Modding Toolkit.unitypackage");
            string projectSettingsDirectory = "ProjectSettings";

            List<string> assetPaths = new List<string>
            {
                AssetDatabase.GetAssetPath(codeSettings),
                AssetDatabase.GetAssetPath(modToolSettings),
                Path.Combine(modToolDirectory, Path.Combine("Editor", "ModTool.Exporting.Editor.dll")),
                Path.Combine(modToolDirectory, Path.Combine("Editor", "ModTool.Shared.Editor.dll")),
                Path.Combine(modToolDirectory, "ModTool.Shared.dll"),
                Path.Combine(modToolDirectory, "ModTool.Shared.xml"),
                Path.Combine(modToolDirectory, "ModTool.Interface.dll"),
                Path.Combine(modToolDirectory, "ModTool.Interface.xml"),
                Path.Combine(projectSettingsDirectory, "InputManager.asset"),
                Path.Combine(projectSettingsDirectory, "TagManager.asset"),
                Path.Combine(projectSettingsDirectory, "Physics2DSettings.asset"),
                Path.Combine(projectSettingsDirectory, "DynamicsManager.asset")
            };

            assetPaths.AddRange(CodeSettings.customPaths);
            assetPaths.AddRange(_builtInPaths);

            SetPluginEnabled(exporterPath, true);

            List<string> assemblyPaths = new List<string>();

            GetApiAssemblies("Assets", assemblyPaths);
            GetApiAssemblies("Library", assemblyPaths);

            assetPaths.AddRange(assemblyPaths);

            AssetDatabase.ExportPackage(assetPaths.ToArray(), fileName, ExportPackageOptions.Default | ExportPackageOptions.Recurse);

            foreach (string assemblyPath in assemblyPaths)
                AssetDatabase.DeleteAsset(assemblyPath);

            SetPluginEnabled(exporterPath, false);

            //if(revealPackage)
            //    EditorUtility.RevealInFinder(fileName);

            var extractDir = Path.Combine(Application.dataPath, "Extraction Point");
            if (Directory.Exists(extractDir))
            {
                Directory.Delete(extractDir);
            }
            Directory.CreateDirectory(extractDir);

            PackageExtractor.ExtractPackage(fileName, extractDir);

            var fname = Path.GetFileNameWithoutExtension(fileName);
            var dirToCopy = Path.Combine(extractDir, fname, "Assets", "Fragsurf");
            CopyDirectory(dirToCopy, destinationFolder);
        }

        private static void CopyDirectory(string source, string dest)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(source, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, dest));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(source, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, dest), true);
        }

        private static void SetPluginEnabled(string pluginPath, bool enabled)
        {
            PluginImporter pluginImporter = AssetImporter.GetAtPath(pluginPath) as PluginImporter;

            if (!pluginImporter || pluginImporter.GetCompatibleWithEditor() == enabled)
                return;

            pluginImporter.SetCompatibleWithEditor(enabled);
            pluginImporter.SaveAndReimport();
        }

        private static void GetApiAssemblies(string path, List<string> assemblies)
        {
            List<string> assemblyPaths = AssemblyUtility.GetAssemblies(path, AssemblyFilter.ApiAssemblies | AssemblyFilter.ModToolAssemblies);
            
            string modToolDirectory = AssetUtility.GetModToolDirectory();
            
            foreach(string assemblyPath in assemblyPaths)
            {
                string fileName = Path.GetFileName(assemblyPath);
                string newPath = Path.Combine(modToolDirectory, fileName);

                File.Copy(assemblyPath, newPath, true);
                AssetDatabase.ImportAsset(newPath);

                assemblies.Add(newPath);
            }            
        }   
        
        private static void UpdateSettings()
        {
            if (string.IsNullOrEmpty(ModToolSettings.productName) || ModToolSettings.productName != Application.productName)
            {
                typeof(ModToolSettings).GetField("_productName", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ModToolSettings.instance, Application.productName);
            }

            if (string.IsNullOrEmpty(ModToolSettings.unityVersion) || ModToolSettings.unityVersion != Application.unityVersion)
            {
                typeof(ModToolSettings).GetField("_unityVersion", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ModToolSettings.instance, Application.unityVersion);
            }

            EditorUtility.SetDirty(ModToolSettings.instance);
        }
    }
}
