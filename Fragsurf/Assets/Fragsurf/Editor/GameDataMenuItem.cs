using UnityEngine;
using UnityEditor;
using Fragsurf.Shared;

namespace Fragsurf.Editor
{
    public class GameDataMenuItem
    {

        private const string _gameDataPath = "Assets/Fragsurf/Content/Resources/GameData.asset";

        [MenuItem("Fragsurf Dev/GameData")]
        public static void CreateMyAsset()
        {
            var exists = AssetDatabase.LoadAssetAtPath(_gameDataPath, typeof(GameData));
            if (exists)
            {
                Selection.activeObject = exists;
                return;
            }

            GameData asset = ScriptableObject.CreateInstance<GameData>();

            AssetDatabase.CreateAsset(asset, _gameDataPath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}

