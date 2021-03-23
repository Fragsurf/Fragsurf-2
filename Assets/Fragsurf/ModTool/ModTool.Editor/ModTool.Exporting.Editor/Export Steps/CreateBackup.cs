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
    public class CreateBackup : ExportStep
    {
        public override string message { get { return "Creating Backup"; } }

        internal override void Execute(ExportData data)
        {
            AssetDatabase.SaveAssets();

            if (Directory.Exists(Asset.backupDirectory))
                Directory.Delete(Asset.backupDirectory, true);

            Directory.CreateDirectory(Asset.backupDirectory);

            if (Directory.Exists(tempAssemblyDirectory))
                Directory.Delete(tempAssemblyDirectory, true);
            
            Directory.CreateDirectory(tempAssemblyDirectory);
            
            foreach (Asset asset in data.assets)
                asset.Backup();

            foreach (Asset scene in data.scenes)
                scene.Backup();

            foreach (Asset script in data.scripts)
                script.Backup();

            foreach (string path in Directory.GetFiles(assemblyDirectory))
                File.Copy(path, Path.Combine(tempAssemblyDirectory, Path.GetFileName(path)));
        }        
    }
}