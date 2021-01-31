using UnityEngine;
using UnityEditor;
using Fragsurf.Shared;

public class GameDataSelector
{

    [MenuItem("Fragsurf/Game Data")]
    public static void SelectGameData()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameData>("Assets/Resources/GameData.asset");
        if (!asset)
        {
            var so = ScriptableObject.CreateInstance<GameData>();
            AssetDatabase.CreateAsset(so, "Assets/Resources/GameData.asset");
            asset = AssetDatabase.LoadAssetAtPath<GameData>("Assets/Resources/GameData.asset");
        }
        Selection.activeObject = asset;
    }

}
