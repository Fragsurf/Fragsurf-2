using UnityEditor;
using UnityEngine;

namespace UIForia.Editor {

    public class UIForiaEditorTheme {

        internal static Color elementNameNormal = EditorGUIUtility.isProSkin
            ? Color.white
            : Color.black;

        internal static Color elementStyleNormal = EditorGUIUtility.isProSkin
            ? Color.yellow
            : new Color(0.23f, 0.23f, 0f, 1f);

        internal static Color mainColorTemplateRoot = EditorGUIUtility.isProSkin
            ? Color.green
            : new Color(0, 0.3f, 0.3f, 1f);


        internal static Color mainColorRegularChild = EditorGUIUtility.isProSkin
            ? Color.white
            : new Color(0.1f, 0.1f, 0.1f, 1f);
        
        internal static Color mainColorChildrenElement = EditorGUIUtility.isProSkin
            ? new Color(1, 0, 0.34f)
            : new Color(0.45f, 0.1f, 0.1f, 1f);


    }

}