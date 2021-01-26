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
    public class RestoreProject : ExportStep
    {
        public override string message { get { return "Restoring Project"; } }

        internal override void Execute(ExportData data)
        {
            foreach (Asset scriptAssembly in data.scriptAssemblies)
                scriptAssembly.Delete();

            foreach (Asset asset in data.assets)
                asset.Restore();

            foreach (Asset scene in data.scenes)
                scene.Restore();

            foreach (Asset script in data.scripts)
                script.Restore();

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (!string.IsNullOrEmpty(data.loadedScene))
                EditorSceneManager.OpenScene(data.loadedScene);

            if (!string.IsNullOrEmpty(data.entryScenePath))
            {
                ExportSettings.scene = data.entryScenePath;
            }
        }
    }     
}