using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using ModTool.Shared;
using Mono.Cecil;
using System.Text.RegularExpressions;

namespace ModTool.Exporting.Editor
{
    public class UpdateAssets : ExportStep
    {
        public override string message { get { return "Updating Assets"; } }

        internal override void Execute(ExportData data)
        {
            var allAssets = data.assets.Concat(data.scenes);
            UpdateReferences(allAssets, data.scriptAssemblies);
            
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer);

            if ((data.content & ModContent.Assets) == ModContent.Assets)
            {
                foreach (Asset asset in data.assets)
                    asset.SetAssetBundle(ExportSettings.name, "assets");
            }

            if ((data.content & ModContent.Scenes) == ModContent.Scenes)
            {
                foreach (Asset scene in data.scenes)
                {
                    scene.name = data.prefix + scene.name;
                    scene.SetAssetBundle(ExportSettings.name, "scenes");
                }
            }
        }

        private static void UpdateReferences(IEnumerable<Asset> assets, IEnumerable<Asset> scriptAssemblies)
        {
            foreach(Asset scriptAssembly in scriptAssemblies)
                UpdateReferences(assets, scriptAssembly);            
        }

        private static void UpdateReferences(IEnumerable<Asset> assets, Asset scriptAssembly)
        {
            string assemblyGuid = AssetDatabase.AssetPathToGUID(scriptAssembly.assetPath);
            ModuleDefinition module = ModuleDefinition.ReadModule(scriptAssembly.assetPath);
            
            foreach (Asset asset in assets)
                UpdateReferences(asset, assemblyGuid, module.Types);
        }

        private static void UpdateReferences(Asset asset, string assemblyGuid, IEnumerable<TypeDefinition> types)
        {
            string[] lines = File.ReadAllLines(asset.assetPath);

            for (int i = 0; i < lines.Length; i++)
            {
                //Note: Line references script file - 11500000 is Unity's YAML class ID for MonoScript
                if (lines[i].Contains("11500000"))
                    lines[i] = UpdateReference(lines[i], assemblyGuid, types);                
            }

            File.WriteAllLines(asset.assetPath, lines);
        }

        private static string UpdateReference(string line, string assemblyGuid, IEnumerable<TypeDefinition> types)
        {
            string guid = GetGuid(line);
            string scriptPath = AssetDatabase.GUIDToAssetPath(guid);
            string scriptName = Path.GetFileNameWithoutExtension(scriptPath);

            foreach (TypeDefinition type in types)
            {
                //script's type found, replace reference
                if (type.Name == scriptName)
                {
                    string fileID = GetTypeID(type.Namespace, type.Name).ToString();
                    line = line.Replace("11500000", fileID);
                    return line.Replace(guid, assemblyGuid);
                }
            }

            return line;
        }

        private static string GetGuid(string line)
        {
            string[] properties = Regex.Split(line, ", ");

            foreach (string property in properties)
            {
                if (property.Contains("guid: "))
                    return property.Remove(0, 6);
            }

            return "";
        }
        
        private static int GetTypeID(TypeDefinition type)
        {
            return GetTypeID(type.Namespace, type.Name);
        }

        private static int GetTypeID(string nameSpace, string typeName)
        {
            string toBeHashed = "s\0\0\0" + nameSpace + typeName;

            using (MD4 hash = new MD4())
            {
                byte[] hashed = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(toBeHashed));

                int result = 0;

                for (int i = 3; i >= 0; --i)
                {
                    result <<= 8;
                    result |= hashed[i];
                }

                return result;
            }
        }    
    }
}