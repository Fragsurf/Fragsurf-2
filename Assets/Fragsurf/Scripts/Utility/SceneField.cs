using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class SceneField
{

    [SerializeField]
    private Object m_SceneAsset;
    [SerializeField]
    private string m_SceneName = "";
    [SerializeField]
    private string m_ScenePath = "";

    public string SceneName => m_SceneName;
    public string ScenePath => m_ScenePath;
    public Object SceneAsset => m_SceneAsset;

    public static implicit operator string(SceneField sceneField)
    {
        return sceneField.SceneName;
    }

    public static implicit operator SceneField(string scenePath)
    {
#if UNITY_EDITOR
        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        if (asset == null)
        {
            return null;
        }

        return new SceneField()
        {
            m_SceneAsset = asset,
            m_SceneName = asset.name,
            m_ScenePath = scenePath
        };
#else
        return default;
#endif
    }

}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        EditorGUI.BeginProperty(_position, GUIContent.none, _property);
        SerializedProperty sceneAsset = _property.FindPropertyRelative("m_SceneAsset");
        SerializedProperty sceneName = _property.FindPropertyRelative("m_SceneName");
        SerializedProperty scenePath = _property.FindPropertyRelative("m_ScenePath");
        _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
        if (sceneAsset != null)
        {
            sceneAsset.objectReferenceValue = EditorGUI.ObjectField(_position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
            if (sceneAsset.objectReferenceValue != null)
            {
                sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
                scenePath.stringValue = AssetDatabase.GetAssetPath(sceneAsset.objectReferenceValue);
            }
        }
        EditorGUI.EndProperty();
    }
}
#endif