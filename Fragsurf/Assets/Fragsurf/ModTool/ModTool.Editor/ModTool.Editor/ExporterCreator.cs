using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using ModTool.Shared;
using ModTool.Shared.Editor;
using Mediatonic.Tools;
using Fragsurf;

//Note: ModTool uses an old version of Mono.Cecil in the editor
#pragma warning disable CS0618

namespace ModTool.Editor
{
    internal class ExporterCreator
    {

        private static string PackageVersion => Structure.Version;
        private static string TempExtractionFolder => Path.Combine(Application.temporaryCachePath, "Modkit Extraction");

        [MenuItem("Fragsurf Dev/ModTool Build")]
        public static void CreateExporter()
        {
            var filePath = Path.Combine(Application.dataPath, "../", "Modding Toolkit.unitypackage");
            CreateExporter(filePath);
            CreatePackage(filePath, Path.GetDirectoryName(filePath));
        }

        [PostProcessBuild]
        public static void CreateExporterPostBuild(BuildTarget target, string pathToBuiltProject)
        {
            if(IsFileBelowDirectory(pathToBuiltProject, Application.temporaryCachePath))
            {
                Debug.Log("Is temp");
                return;
            }
            var filePath = Path.Combine(Application.dataPath, "../", "Modding Toolkit.unitypackage");
            CreateExporter(filePath);
            CreatePackage(filePath, Path.GetDirectoryName(pathToBuiltProject));
        }

        public static bool IsFileBelowDirectory(string fileInfo, string directoryInfo)
        {
            var di1 = new DirectoryInfo(Path.GetDirectoryName(fileInfo));
            var di2 = new DirectoryInfo(directoryInfo);
            return di1.FullName.Contains(di2.FullName);
        }

        private static void CreatePackage(string unityPackagePath, string directory)
        {
            if (!File.Exists(unityPackagePath))
            {
                throw new System.Exception("Missing UnityPackage at path: " + unityPackagePath);
            }

            var packageDirectory = Path.Combine(directory, "fragsurf-modding-toolkit");

            if (Directory.Exists(packageDirectory))
            {
                Directory.Delete(packageDirectory, true);
            }

            Directory.CreateDirectory(packageDirectory);

            var packageJsonPath = Path.Combine(packageDirectory, "package.json");
            File.WriteAllText(packageJsonPath, _packageJson);

            var extractedPackagePath = Path.Combine(packageDirectory, "UnityPackage");
            ExtractModdingPackage(unityPackagePath, extractedPackagePath);
        }

        private static void ExtractModdingPackage(string unitypackagePath, string destinationFolder)
        {
            var tempExtractDir = TempExtractionFolder;
            if (Directory.Exists(tempExtractDir))
            {
                Directory.Delete(tempExtractDir, true);
            }
            Directory.CreateDirectory(tempExtractDir);

            PackageExtractor.ExtractPackage(unitypackagePath, tempExtractDir);

            var fname = Path.GetFileNameWithoutExtension(unitypackagePath);
            var dirToCopy = Path.Combine(tempExtractDir, fname, "Assets");
            if (Directory.Exists(destinationFolder))
            {
                Directory.Delete(destinationFolder, true);
            }
            Directory.CreateDirectory(destinationFolder);
            CopyDirectory(dirToCopy, destinationFolder);

            Directory.Delete(tempExtractDir, true);
        }

        private static void CreateExporter(string filePath)
        {
            LogUtility.LogInfo("Creating Exporter");

            UpdateSettings();

            var codeSettings = CodeSettings.instance;
            var modToolSettings = ModToolSettings.instance;

            string modToolDirectory = AssetUtility.GetModToolDirectory();
            string exporterPath = Path.Combine(modToolDirectory, Path.Combine("Editor", "ModTool.Exporting.Editor.dll"));
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

            SetPluginEnabled(exporterPath, true);

            List<string> assemblyPaths = new List<string>();

            GetApiAssemblies("Assets", assemblyPaths);
            GetApiAssemblies("Library", assemblyPaths);

            assetPaths.AddRange(assemblyPaths);

            AssetDatabase.ExportPackage(assetPaths.ToArray(), filePath, ExportPackageOptions.Default | ExportPackageOptions.Recurse);

            foreach (string assemblyPath in assemblyPaths)
            {
                AssetDatabase.DeleteAsset(assemblyPath);
            } 

            SetPluginEnabled(exporterPath, false);
        }

        private static void CopyDirectory(string source, string dest)
        {
            foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(source, dest));
            }

            foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(source, dest), true);
            }
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

        private static string _packageJson => "{\r\n  \"name\": \"com.fragsurf.modding-toolkit\",\r\n  \"displayName\": \"Fragsurf - Modding Toolkit\",\r\n  \"version\": \"" + PackageVersion + "\",\r\n  \"unity\": \"2020.2\",\r\n  \"description\": \"Modding tools for Fragsurf\",\r\n  \"keywords\": [ \"fragsurf\" ],\r\n  \"homepage\": \"https://github.com/fragsurf/modding-toolkit.git\",\r\n  \"bugs\": {\r\n    \"url\": \"https://github.com/fragsurf/modding-toolkit/issues\"\r\n  },\r\n  \"repository\": {\r\n    \"type\": \"git\",\r\n    \"url\": \"git+ssh://git@github.com/fragsurf/modding-toolkit.git\"\r\n  },\r\n  \"license\": \"Fragsurf Only\",\r\n  \"author\": \"Fragsurf (https://fragsurf.com)\",\r\n  \"scripts\": {\r\n    \"test\": \"echo \\\"Error: no test specified\\\" && exit 1\"\r\n  },\r\n \"dependencies\":\r\n  {\r\n\t\"com.unity.render-pipelines.universal\": \"10.2.2\"\r\n  }\r\n}\r\n";

    }
}
