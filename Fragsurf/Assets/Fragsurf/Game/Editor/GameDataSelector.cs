using UnityEngine;
using UnityEditor;
using Fragsurf.Shared;

public class GameDataSelector
{

    [MenuItem("Fragsurf/Game Data")]
    public static void SelectGameData()
    {
        var asset = AssetDatabase.LoadAssetAtPath<DataSet>("Assets/Resources/GameData.asset");
        if (!asset)
        {
            var so = ScriptableObject.CreateInstance<DataSet>();
            AssetDatabase.CreateAsset(so, "Assets/Resources/GameData.asset");
            asset = AssetDatabase.LoadAssetAtPath<DataSet>("Assets/Resources/GameData.asset");
        }
        Selection.activeObject = asset;
    }

}
