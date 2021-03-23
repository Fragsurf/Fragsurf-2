using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using ModTool.Shared;
using ModTool.Shared.Verification;
using ModTool.Shared.Editor;
using Mono.Cecil;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace ModTool.Exporting.Editor
{
    public class ImportScripts : ExportStep
    {
        public override string message { get { return "Importing Script Assemblies"; } }

        internal override void Execute(ExportData data)
        {
            if((data.content & ModContent.Code) != ModContent.Code)
                return;

            foreach (Asset script in data.scripts)
                script.Delete();

            string prefix = data.prefix.Replace(" ", "");

            if (!string.IsNullOrEmpty(ExportSettings.version))
                prefix += ExportSettings.version.Replace(" ", "") + "-";

            List<string> searchDirectories = GetSearchDirectories();

            foreach (string scriptAssembly in scriptAssemblies)
            {
                string scriptAssemblyPath = Path.Combine(tempAssemblyDirectory, scriptAssembly);

                if (!File.Exists(scriptAssemblyPath))
                    continue;

                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(scriptAssemblyPath);
                AssemblyNameDefinition assemblyName = assembly.Name;

                DefaultAssemblyResolver resolver = (DefaultAssemblyResolver)assembly.MainModule.AssemblyResolver;

                foreach (string searchDirectory in searchDirectories)
                    resolver.AddSearchDirectory(searchDirectory);

                assemblyName.Name = prefix + assemblyName.Name;

                foreach (var reference in assembly.MainModule.AssemblyReferences)
                {
                    if (reference.Name.Contains("firstpass"))
                        reference.Name = prefix + reference.Name;
                }
                                
                scriptAssemblyPath = Path.Combine(modToolDirectory, assemblyName.Name + ".dll");                

                assembly.Write(scriptAssemblyPath);

                data.scriptAssemblies.Add(new Asset(scriptAssemblyPath));
            }

            if (data.scriptAssemblies.Count > 0)
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer);
                ForceAssemblyReload();
            }            
        }

        private static List<string> GetSearchDirectories()
        {
            List<string> searchDirectories = new List<string>()
            {
                Path.GetDirectoryName(typeof(UnityEngine.Object).Assembly.Location),
                assetsDirectory
            };

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.GetName().Name == "netstandard")
                    searchDirectories.Add(Path.GetDirectoryName(a.Location));
            }

            return searchDirectories;
        }
    }
}