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
    public class GetContent : ExportStep
    {
        public override string message { get { return "Finding Content"; } }

        internal override void Execute(ExportData data)
        {            
            data.assemblies = GetAssemblies();
            data.assets = GetAssets("t:prefab t:scriptableobject");
            //data.scenes = GetAssets("t:scene");
            data.scenes = new List<Asset>() { new Asset(ExportSettings.scene.ScenePath) };
            data.entryScenePath = ExportSettings.scene.ScenePath;
            data.scripts = GetAssets("t:monoscript");
            
            ModContent content = ExportSettings.content;

            if (data.assets.Count == 0)
                content &= ~ModContent.Assets;
            if (data.scenes.Count == 0)
                content &= ~ModContent.Scenes;
            if (data.assemblies.Count == 0 && data.scripts.Count == 0)
                content &= ~ModContent.Code;
            
            data.content = content;
        }

        private List<Asset> GetAssets(string filter)
        {
            List<Asset> assets = new List<Asset>();

            foreach (string path in AssetUtility.GetAssets(filter))
            {
                assets.Add(new Asset(path));
            }

            return assets;
        }

        private List<Asset> GetAssemblies()
        {            
            List<Asset> assemblies = new List<Asset>();

            foreach (string path in AssemblyUtility.GetAssemblies(assetsDirectory, AssemblyFilter.ModAssemblies))
            {
                Asset assembly = new Asset(path);
                assembly.Move(modToolDirectory);
                assemblies.Add(assembly);
            }                        

            return assemblies;
        }          
    }
}