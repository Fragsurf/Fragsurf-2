using System.IO;
using UnityEditor;
using UnityEngine;

namespace UIForia {

    public static class Builder {

        [MenuItem("UIForia/Create UIForia Settings and Materials")]
        public static void CreateOptionsObject() {
         
            UIForiaSettings asset = ScriptableObject.CreateInstance<UIForiaSettings>();
            Material uiforiaStdMaterial = new Material(Shader.Find("UIForia/Standard"));
            Material sdfPathMaterial = new Material(Shader.Find("UIForia/UIForiaPathSDF"));
            Material spriteAtlasMaterial = new Material(Shader.Find("UIForia/UIForiaSpriteAtlas"));
            Material clearClipRegionsMaterial = new Material(Shader.Find("UIForia/UIForiaClearClipRegions"));
            Material clipCountMaterial = new Material(Shader.Find("UIForia/UIForiaClipCount"));
            Material clipBlitMaterial = new Material(Shader.Find("UIForia/UIForiaClipBlit"));

            Directory.CreateDirectory(Path.Combine(UnityEngine.Application.dataPath, "Resources"));

            AssetDatabase.CreateAsset(uiforiaStdMaterial, "Assets/Resources/UIForiaStandardMaterial.mat");
            asset.batchedMaterial = uiforiaStdMaterial;

            AssetDatabase.CreateAsset(sdfPathMaterial, "Assets/Resources/UIForiaSDFPathMaterial.mat");
            asset.sdfPathMaterial = sdfPathMaterial;

            AssetDatabase.CreateAsset(spriteAtlasMaterial, "Assets/Resources/UIForiaSpriteAtlasMaterial.mat");
            asset.spriteAtlasMaterial = spriteAtlasMaterial;

            AssetDatabase.CreateAsset(clearClipRegionsMaterial, "Assets/Resources/UIForiaClearClipRegionsMaterial.mat");
            asset.clearClipRegionsMaterial = clearClipRegionsMaterial;

            AssetDatabase.CreateAsset(clipCountMaterial, "Assets/Resources/UIForiaClipCountMaterial.mat");
            asset.clipCountMaterial = clipCountMaterial;

            AssetDatabase.CreateAsset(clipBlitMaterial, "Assets/Resources/UIForiaClipBlitMaterial.mat");
            asset.clipBlitMaterial = clipBlitMaterial;

            AssetDatabase.CreateAsset(asset, "Assets/Resources/UIForiaSettings.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        [MenuItem("UIForia/Build Templates")]
        public static void BuildTemplates() {
            string userPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForia");
            string internalDestPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForiaInternal");
            string internalSourcePath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Src");

            if (Directory.Exists(userPath)) {
                Directory.Delete(userPath, true);
            }

            if (Directory.Exists(internalDestPath)) {
                Directory.Delete(internalDestPath, true);
            }

            string[] files = Directory.GetFiles(UnityEngine.Application.dataPath, "*.xml", SearchOption.AllDirectories);

            foreach (string file in files) {
                string newPath = file.Replace(UnityEngine.Application.dataPath, userPath);
                Directory.CreateDirectory(new FileInfo(newPath).Directory.FullName);
                File.Copy(file, newPath, true);
            }

            files = Directory.GetFiles(UnityEngine.Application.dataPath, "*.style", SearchOption.AllDirectories);
            
            foreach (string file in files) {
                string newPath = file.Replace(UnityEngine.Application.dataPath, userPath);
                Directory.CreateDirectory(new FileInfo(newPath).Directory.FullName);
                File.Copy(file, newPath, true);
            }
            
            files = Directory.GetFiles(internalSourcePath, "*.xml", SearchOption.AllDirectories);

            foreach (string file in files) {
                string newPath = file.Replace(internalSourcePath, internalDestPath);
                Directory.CreateDirectory(new FileInfo(newPath).Directory.FullName);
                File.Copy(file, newPath, true);
            }

            AssetDatabase.Refresh();
        }

    }

}