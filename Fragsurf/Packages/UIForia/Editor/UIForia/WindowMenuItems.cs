using UnityEditor;

namespace UIForia.Editor {

    public static class WindowMenuItems {

        [MenuItem("Window/UIForia/UIForia Hierarchy")]
        private static void UIForiaHierarchy() {
            EditorWindow.GetWindow<UIForiaHierarchyWindow>("UIForia Hierarchy");
        }

        [MenuItem("Window/UIForia/UIForia Inspector")]
        private static void UIForiaInspector() {
            EditorWindow.GetWindow<UIForiaInspectorWindow>("UIForia Inspector");
        }

        [MenuItem("UIForia/Refresh UI Templates %g")]
        public static void Refresh() {
            if (UnityEngine.Application.isPlaying) {
                Application.RefreshAll();
            }
        }
    }

}