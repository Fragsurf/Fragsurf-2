using UnityEditor;
using UnityEngine;

namespace ModTool.Exporting.Editor
{
    internal class ExporterEditorWindow : EditorWindow
    {
        private UnityEditor.Editor exportSettingsEditor;

        [MenuItem("Fragsurf/Mod/Export")]
        public static void ShowWindow()
        {
            ExporterEditorWindow window = GetWindow<ExporterEditorWindow>();
            window.minSize = new Vector2(300f, 360);
            window.titleContent = new GUIContent("Mod Exporter");
        }

        void OnEnable()
        {
            ExportSettings exportSettings = ExportSettings.instance;

            exportSettingsEditor = UnityEditor.Editor.CreateEditor(exportSettings);
        }

        void OnDisable()
        {
            DestroyImmediate(exportSettingsEditor);
        }

        void OnGUI()
        {
            GUI.enabled = !EditorApplication.isCompiling && !ModExporter.isExporting && !Application.isPlaying;

            exportSettingsEditor.OnInspectorGUI();

            GUILayout.FlexibleSpace();

            bool buttonPressed = GUILayout.Button("Export Mod", GUILayout.Height(30));

            if (buttonPressed)
                ModExporter.ExportMod();
        }
    }
}
