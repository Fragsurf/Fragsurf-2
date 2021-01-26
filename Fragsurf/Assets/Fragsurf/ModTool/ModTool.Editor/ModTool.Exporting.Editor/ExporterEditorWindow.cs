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

            if(ExportSettings.WorkshopId != 0)
            {
                GUILayout.Space(5);
                GUILayout.Label("This item is on the Workshop!  To add screenshots and videos, visit the Workshop page and click the 'Add/Edit images & videos' button.", EditorStyles.helpBox);
                GUILayout.Space(5);
                if (GUILayout.Button("Visit Workshop", GUILayout.Height(30))
                    && EditorUtility.DisplayDialog("Open Browser", "This will open your browser to visit the Workshop page.  Are you sure?", "Yes", "Cancel"))
                {
                    Application.OpenURL(ExportSettings.WorkshopUrl);
                }
            }

            if(GUILayout.Button("Export Mod", GUILayout.Height(30))
                && EditorUtility.DisplayDialog("Export Mod", "This will start the mod export process.  Are you sure?", "Yes", "Cancel"))
            {
                ModExporter.ExportMod();
            }
        }
    }
}
