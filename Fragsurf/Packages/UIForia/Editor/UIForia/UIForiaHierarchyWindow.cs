using System.Collections.Generic;
using System.Reflection;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UIForia.Editor {

    public class UIForiaHierarchyWindow : EditorWindow {

        public static readonly List<int> EmptyList = new List<int>();
        public const string k_InspectedAppKey = "UIForia.Inspector.ApplicationName";

        private int tab;
        public static readonly string[] s_TabNames = {"Hierarchy", "Settings", "Metrics"};

        private Color contentColor = new Color32(140, 182, 193, 175);
        private Color allocatedContentColor = new Color32(90, 212, 193, 175);
        private Color borderColor = new Color32(253, 221, 155, 175);
        private Color marginColor = new Color32(249, 204, 157, 175);
        private Color paddingColor = new Color32(196, 208, 139, 175);
        private Color allocatedSpaceColor = Color.red;
        private Color descenderColor = Color.blue;
        private Color outlineColor = new Color32(196, 208, 139, 175);
        
        private bool showTextBaseline;
        private bool showTextDescender;
        private bool drawDebugBox;
        
        public TreeViewState state;
        public HierarchyView treeView;
        private bool needsReload;
        private string inspectedAppId;
        private bool firstLoad;
        
        private Path2D path = new Path2D();

        private void OnInspectorUpdate() {
            Repaint();
        }

        private static MethodInfo s_GameWindowSizeMethod;

        public static int s_SelectedElementId;
        public static Application s_SelectedApplication;

        public void OnEnable() {
            firstLoad = true;
            state = new TreeViewState();
            autoRepaintOnSceneChange = true;
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            
            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.ContentColor"), out contentColor)) {
                contentColor = new Color32(140, 182, 193, 175);
            }

            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.BorderColor"), out borderColor)) {
                borderColor = new Color32(153, 121, 155, 175);
            }

            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.PaddingColor"), out paddingColor)) {
                paddingColor = new Color32(153, 221, 155, 175);
            }

            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.MarginColor"), out marginColor)) {
                marginColor = new Color32(103, 121, 255, 175);
            }

            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.AllocatedSpaceColor"), out allocatedSpaceColor)) {
                allocatedSpaceColor = new Color32(230, 220, 15, 75);
            }

            if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString("UIForia.Inspector.DescenderColor"), out descenderColor)) {
                descenderColor = Color.blue;
            }

            showTextBaseline = EditorPrefs.GetBool("UIForia.Inspector.ShowTextBaseline", false);
            showTextDescender = EditorPrefs.GetBool("UIForia.Inspector.ShowTextDescender", false);
            drawDebugBox = EditorPrefs.GetBool("UIForia.Inspector.DrawDebugBox", true);
        }

        private void OnElementSelectionChanged(UIElement element) {
            if (element != null) {
                s_SelectedElementId = element.id;
            }
            else {
                s_SelectedElementId = -1;
            }
        }

        private void Refresh(UIElement element) {
            needsReload = true;
        }
        
        private void Refresh(UIView view) {
            needsReload = true;
        }

        public void OnRefresh() {
            s_SelectedElementId = -1;
            treeView?.Destroy();

            Application app = Application.Find(inspectedAppId);

            if (app == null) return;

            if (s_SelectedApplication != null) {
                s_SelectedApplication.RenderSystem.DrawDebugOverlay2 -= DrawDebugOverlay;
            }

            app.RenderSystem.DrawDebugOverlay2 += DrawDebugOverlay;
            treeView = new HierarchyView(app.GetViews(), state);
            treeView.onSelectionChanged += OnElementSelectionChanged;
//            treeView.view = app.GetView(0);
        }

        private void Update() {
            if (!EditorApplication.isPlaying) {
                return;
            }

            if (treeView != null && treeView.selectMode && s_SelectedApplication?.InputSystem.DebugElementsThisFrame.Count > 0) {
                if (s_SelectedApplication.InputSystem.DebugMouseUpThisFrame) {
                    treeView.selectMode = false;
                }
                else {
                    s_SelectedElementId = s_SelectedApplication.InputSystem.DebugElementsThisFrame[s_SelectedApplication.InputSystem.DebugElementsThisFrame.Count - 1].id;
                    IList<int> selectedIds = new List<int>(s_SelectedApplication.InputSystem.DebugElementsThisFrame.Count);

                    int selectIdx = 0;

                    s_SelectedElementId = s_SelectedApplication.InputSystem.DebugElementsThisFrame[selectIdx].id;
                    selectedIds.Add(s_SelectedElementId);
                    treeView.SetSelection(selectedIds);
                    if (selectedIds.Count > 0) {
                        treeView.FrameItem(s_SelectedElementId);
                    }
                }
            }

            Repaint();
        }

        private void SetApplication(string appId) {

            Application oldApp = Application.Find(inspectedAppId);

            if (oldApp != null) {
                oldApp.onElementDestroyed -= Refresh;
               // oldApp.onViewAdded -= Refresh;
                oldApp.onElementEnabled -= Refresh;
                //oldApp.onRefresh -= OnRefresh;
            }

            treeView?.Destroy();

            inspectedAppId = appId;
            EditorPrefs.SetString(k_InspectedAppKey, appId);

            Application app = Application.Find(appId);

            if (app != null) {
                needsReload = true;

                treeView = new HierarchyView(app.GetViews(), state);
                treeView.onSelectionChanged += OnElementSelectionChanged;

                app.onElementDestroyed += Refresh;
                //app.onViewAdded += Refresh;
                app.onElementEnabled += Refresh;
                app.onRefresh += OnRefresh;
            }
            
            if (s_SelectedApplication != null) {
                s_SelectedApplication.RenderSystem.DrawDebugOverlay2 -= DrawDebugOverlay;
            }

            if (app != null) {
                app.RenderSystem.DrawDebugOverlay2 += DrawDebugOverlay;
            }
            
            s_SelectedApplication = app;
            s_SelectedElementId = -1;
        }

        public void OnGUI() {
            if (!EditorApplication.isPlaying) {
                EditorGUILayout.LabelField("Enter play mode to inspect a UIForia Application");
                return;
            }

            tab = GUILayout.Toolbar(tab, s_TabNames);
            EditorGUIUtility.labelWidth += 50;
            switch (tab) {
                case 0:
                    DrawHierarchyInfo();
                    break;
                case 1:
                    DrawSettings();
                    break;
                case 2:
                    DrawMetrics();
                    break;
            }

            EditorGUIUtility.labelWidth -= 50;
        }

        private void DrawHierarchyInfo() {

            EditorGUILayout.BeginVertical();
            string[] names = new string[Application.Applications.Count + 1];
            names[0] = "None";

            int oldIdx = 0;

            for (int i = 1; i < names.Length; i++) {
                names[i] = Application.Applications[i - 1].id;
                if (names[i] == inspectedAppId) {
                    oldIdx = i;
                }
            }

            int idx = EditorGUILayout.Popup(new GUIContent("Application"), oldIdx, names);
            if (firstLoad || idx != oldIdx) {
                SetApplication(names[idx]);
                if (firstLoad) {
                    SetApplication(names[idx]);
                    s_SelectedElementId = -1;
                    firstLoad = false;
                }
            }

            if (s_SelectedApplication == null) {
                treeView?.Destroy();
                treeView = null;
            }
            
            if (treeView == null) {
                EditorGUILayout.EndVertical();
                return;
            }

            treeView.showChildrenAndId = EditorGUILayout.ToggleLeft("Show Meta Data", treeView.showChildrenAndId);
            treeView.selectMode = EditorGUILayout.ToggleLeft("Activate Select Mode", treeView.selectMode);
            
            bool wasShowingDisabled = treeView.showDisabled;
            treeView.showDisabled = EditorGUILayout.ToggleLeft("Show Disabled", treeView.showDisabled);
            treeView.showLayoutStats = EditorGUILayout.ToggleLeft("Show Layout Stats", treeView.showLayoutStats);
            
            if (treeView.showDisabled != wasShowingDisabled) {
                needsReload = true;
            }
            if (needsReload) {
                needsReload = false;
                treeView.views = s_SelectedApplication.GetViews();
                treeView.Reload();
                treeView.ExpandAll();
            }

            needsReload = treeView.RunGUI();

            EditorGUILayout.EndVertical();
        }

        private void DrawHorizontalDotted(float y, in Color color) {
            path.BeginPath();
            path.SetStrokeWidth(2);
            path.SetStroke(color);
            float x = 0;
            while (x < Screen.width + 5) {
                path.MoveTo(x, y);
                path.LineTo(x + 5, y);
                x += 8;
            }

            path.EndPath();
            path.Stroke();
        }

        private void DrawVerticalDotted(float x, in Color color) {
            path.BeginPath();
            path.SetStrokeWidth(2);
            path.SetStroke(color);
            float y = 0;
            while (y < Screen.height + 5) {
                path.MoveTo(x, y);
                path.LineTo(x, y + 5);
                y += 8;
            }

            path.EndPath();
            path.Stroke();
        }

        private void DrawDebugOverlay(RenderContext ctx) {
            if (!drawDebugBox) return;
            // path.DisableScissorRect();

            path.Clear();
            path.SetFillOpacity(1);
            path.SetStrokeOpacity(1);

            var selectedElement = s_SelectedApplication.GetElement(s_SelectedElementId);

            if (selectedElement != null && selectedElement.isEnabled) {

                LayoutResult result = selectedElement.layoutResult;

                OffsetRect padding = selectedElement.layoutResult.padding;
                OffsetRect border = selectedElement.layoutResult.border;
                OffsetRect margin = selectedElement.layoutResult.margin;

                float width = result.actualSize.width;
                float height = result.actualSize.height;

                float x = result.screenPosition.x;
                float y = result.screenPosition.y;

                // allocatedX and Y: still not correct but closer to reality, mostly
                float allocatedX = result.ScreenRect.x;
                float allocatedY = result.ScreenRect.y;
                float allocatedW = result.allocatedSize.width;
                float allocatedH = result.allocatedSize.height;

                path.SetFill(allocatedSpaceColor);
                path.BeginPath();
                path.Rect(allocatedX, allocatedY, allocatedW, allocatedH);
                path.Fill();

                path.SetFill(contentColor);
                float contentX = (result.screenPosition.x) + border.left + padding.left;
                float contentY = (result.screenPosition.y) + border.top + padding.top;
                float contentWidth = result.actualSize.width - border.Horizontal - padding.Horizontal;
                float contentHeight = result.actualSize.height - border.Vertical - padding.Vertical;
                // float actualWidth = result.allocatedSize.width - border.Horizontal - padding.Horizontal;
                // float allocatedHeight = result.allocatedSize.height - border.Vertical - padding.Vertical;
                path.BeginPath();
                path.Rect(contentX, contentY, contentWidth, contentHeight);
                path.Fill();

                path.SetFill(allocatedContentColor);
                path.BeginPath();
                path.Rect(contentX, contentY, contentWidth, contentHeight);
                path.Fill();
                float paddingHorizontalWidth = width - padding.Horizontal - border.left;
                float paddingVerticalHeight = height - border.Vertical;

                path.SetFill(paddingColor);
                if (padding.top > 0) {
                    path.BeginPath();
                    path.Rect(x + padding.left + border.left, y + border.top, paddingHorizontalWidth, padding.top);
                    path.Fill();
                }

                if (padding.right > 0) {
                    path.BeginPath();
                    path.Rect(x + width - padding.right - border.right, y + border.top, padding.right, paddingVerticalHeight);
                    path.Fill();
                }

                if (padding.left > 0) {
                    path.BeginPath();
                    path.Rect(x + border.left, y + border.top, padding.left, paddingVerticalHeight);
                    path.Fill();
                }

                if (padding.bottom > 0) {
                    path.BeginPath();
                    path.Rect(x + border.left + padding.left, y - border.top + height - padding.bottom, paddingHorizontalWidth, padding.bottom);
                    path.Fill();
                }

                path.SetFill(borderColor);

                if (border.top > 0) {
                    path.BeginPath();
                    path.Rect(x + border.left, y, width - border.Horizontal, border.top);
                    path.Fill();
                }

                if (border.right > 0) {
                    path.BeginPath();
                    path.Rect(x + width - border.right, y, border.right, height);
                    path.Fill();
                }

                if (border.left > 0) {
                    path.BeginPath();
                    path.Rect(x, y, border.left, height);
                    path.Fill();
                }

                if (border.bottom > 0) {
                    path.BeginPath();
                    path.Rect(x + border.left, y + height - border.bottom, width - border.Horizontal, border.bottom);
                    path.Fill();
                }

                path.SetFill(marginColor);
                if (margin.left > 0) {
                    path.BeginPath();
                    path.Rect(x - margin.left, y, margin.left, height);
                    path.Fill();
                }

                if (margin.right > 0) {
                    path.BeginPath();
                    path.Rect(x + width, y, margin.right, height);
                    path.Fill();
                }

                if (margin.top > 0) {
                    path.BeginPath();
                    path.Rect(x - margin.left, y - margin.top, width + margin.Horizontal, margin.top);
                    path.Fill();
                }

                if (margin.bottom > 0) {
                    path.BeginPath();
                    path.Rect(x - margin.left, y + height, width + margin.Horizontal, margin.bottom);
                    path.Fill();
                }

                DrawHorizontalDotted(contentY, contentColor);
                DrawHorizontalDotted(contentY + contentHeight, contentColor);
                DrawVerticalDotted(contentX, contentColor);
                DrawVerticalDotted(contentX + contentWidth, contentColor);

                if (selectedElement.layoutBox is AwesomeGridLayoutBox layoutBox) {
                    path.SetTransform(SVGXMatrix.TRS(selectedElement.layoutResult.screenPosition + selectedElement.layoutResult.ContentRect.min, 0, Vector2.one).ToMatrix4x4());
                    path.BeginPath();
                    path.SetStrokeWidth(1);
                    path.SetStroke(Color.black);

                    StructList<AwesomeGridLayoutBox.GridTrack> rows = layoutBox.rowTrackList;
                    StructList<AwesomeGridLayoutBox.GridTrack> cols = layoutBox.colTrackList;

                    Rect contentRect = selectedElement.layoutResult.ContentRect;
                    for (int i = 0; i < rows.Count; i++) {
                        path.MoveTo(contentX, contentY + rows[i].position);
                        path.LineTo(contentX + contentRect.width, contentY + rows[i].position);
                    }

                    for (int i = 0; i < cols.Count; i++) {
                        path.MoveTo(contentX + cols[i].position, contentY);
                        path.LineTo(contentX + cols[i].position, contentY + contentRect.height);
                    }

                    path.Stroke();
                }
                
                ctx.DrawPath(path);
            }
        }

        private void DrawMetrics() {
            if (s_SelectedApplication != null) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Frame time: ");
                EditorGUILayout.LabelField(s_SelectedApplication.loopTimer.Elapsed.TotalMilliseconds.ToString("F3"));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Layout: ");
                EditorGUILayout.LabelField(s_SelectedApplication.layoutTimer.Elapsed.TotalMilliseconds.ToString("F3"));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Bindings: ");
                EditorGUILayout.LabelField(s_SelectedApplication.bindingTimer.Elapsed.TotalMilliseconds.ToString("F3"));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Rendering: ");
                EditorGUILayout.LabelField(s_SelectedApplication.renderTimer.Elapsed.TotalMilliseconds.ToString("F3"));
                EditorGUILayout.EndHorizontal();

                int totalElements = 0;
                int enabledElements = 0;
                int disableElements = 0;
                s_SelectedApplication.GetElementCount(out totalElements, out enabledElements, out disableElements);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Element count: ");
                EditorGUILayout.LabelField(totalElements.ToString());
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Enabled element count: ");
                EditorGUILayout.LabelField(enabledElements.ToString());
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Disabled element count: ");
                EditorGUILayout.LabelField(disableElements.ToString());
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawSettings() {
            bool newShowBaseLine = EditorGUILayout.Toggle("Show Text Baseline", showTextBaseline);
            bool newShowDescenderLine = EditorGUILayout.Toggle("Show Text Descender", showTextDescender);

            drawDebugBox = EditorGUILayout.Toggle("Draw Debug Box", drawDebugBox);

            Color newContentColor = EditorGUILayout.ColorField("Content Color", contentColor);
            Color newPaddingColor = EditorGUILayout.ColorField("Padding Color", paddingColor);
            Color newBorderColor = EditorGUILayout.ColorField("Border Color", borderColor);
            Color newMarginColor = EditorGUILayout.ColorField("Margin Color", marginColor);

            Color newBaseLineColor = EditorGUILayout.ColorField("Allocated Space Color", allocatedSpaceColor);
            Color newDescenderColor = EditorGUILayout.ColorField("Text Descender Color", descenderColor);

            if (newContentColor != contentColor) {
                contentColor = newContentColor;
                EditorPrefs.SetString("UIForia.Inspector.ContentColor", "#" + ColorUtility.ToHtmlStringRGBA(contentColor));
            }

            if (newPaddingColor != paddingColor) {
                paddingColor = newPaddingColor;
                EditorPrefs.SetString("UIForia.Inspector.PaddingColor", "#" + ColorUtility.ToHtmlStringRGBA(paddingColor));
            }

            if (newBorderColor != borderColor) {
                borderColor = newBorderColor;
                EditorPrefs.SetString("UIForia.Inspector.BorderColor", "#" + ColorUtility.ToHtmlStringRGBA(borderColor));
            }

            if (marginColor != newMarginColor) {
                marginColor = newMarginColor;
                EditorPrefs.SetString("UIForia.Inspector.MarginColor", "#" + ColorUtility.ToHtmlStringRGBA(marginColor));
            }

            if (allocatedSpaceColor != newBaseLineColor) {
                allocatedSpaceColor = newBaseLineColor;
                EditorPrefs.SetString("UIForia.Inspector.AllocatedSpaceColor", "#" + ColorUtility.ToHtmlStringRGBA(allocatedSpaceColor));
            }

            if (descenderColor != newDescenderColor) {
                descenderColor = newDescenderColor;
                EditorPrefs.SetString("UIForia.Inspector.DescenderColor", ColorUtility.ToHtmlStringRGBA(descenderColor));
            }

            if (newShowBaseLine != showTextBaseline) {
                showTextBaseline = newShowBaseLine;
                EditorPrefs.SetBool("UIForia.Inspector.ShowTextBaseline", showTextBaseline);
            }

            if (newShowDescenderLine != showTextDescender) {
                showTextDescender = newShowDescenderLine;
                EditorPrefs.SetBool("UIForia.Inspector.ShowTextDescender", showTextDescender);
            }

            EditorPrefs.SetBool("UIForia.Inspector.DrawDebugBox", drawDebugBox);
        }
    }

}