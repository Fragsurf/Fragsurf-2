using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

namespace Fyrvall.DataEditor
{
    public class BonaDataEditorAssetPostProcessor : AssetPostprocessor
    {
        private static readonly List<string> AssetFileEndings = new List<string> { ".asset", ".prefab" }; 
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var changedAssets = new List<string>();
            changedAssets.AddRange(importedAssets);
            changedAssets.AddRange(deletedAssets);

            var editorsNeedUpdate = changedAssets.Any(a => AssetFileEndings.Contains(Path.GetExtension(a.ToLower())));
            if (editorsNeedUpdate) {
                Resources.FindObjectsOfTypeAll<BonaDataEditorWindow>()
                    .Select(o => o.To<BonaDataEditorWindow>())
                    .ForEach(d => d.RefreshObjects())
                    .ForEach(d => d.Repaint());
            }
        }
    }
}