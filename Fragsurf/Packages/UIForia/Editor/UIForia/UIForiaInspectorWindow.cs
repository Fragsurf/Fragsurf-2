using System;
using System.Collections.Generic;
using TMPro;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.UIInput;
using UIForia.Util;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Editor {

    public class UIForiaInspectorWindow : EditorWindow {

        private UIElement selectedElement;
        private Vector2 scrollPosition;
        private Color overlayColor;
        private Vector3 drawPos;
        private float overlayBorderSize;
        private Color overlayBorderColor;

        private Material lineMaterial;
        private Mesh baselineMesh;

        private readonly Dictionary<UIStyle, bool> m_ExpandedMap = new Dictionary<UIStyle, bool>();
        private static readonly GUIContent s_Content = new GUIContent();
        private static readonly StylePropertyIdComparer s_StyleCompare = new StylePropertyIdComparer();

        private static readonly Dictionary<Type, ValueTuple<int[], GUIContent[]>> m_EnumValueMap = new Dictionary<Type, ValueTuple<int[], GUIContent[]>>();

        private Mesh mesh;
        private Material material;
        private int tab;
        private Application app;

        public static readonly string[] s_TabNames = {"Element", "Applied Styles", "Computed Style"};

        public void Update() {
            if (!EditorApplication.isPlaying) {
                return;
            }

            if (app != UIForiaHierarchyWindow.s_SelectedApplication) {
                app = UIForiaHierarchyWindow.s_SelectedApplication;
                m_ExpandedMap.Clear();
            }

            Repaint();
        }

        private bool showAllComputedStyles;
        private bool showComputedSources;

        private SearchField searchField;
        private string searchString = string.Empty;

        private void OnEnable() {
            searchField = new SearchField();
        }

        private void DrawComputedStyle() {
            // style name, style value, source

            UIStyleSet style = selectedElement.style;

            bool isSet = (selectedElement.flags & UIElementFlags.DebugLayout) != 0;
            if (EditorGUILayout.ToggleLeft("Debug Layout", isSet)) {
                selectedElement.flags |= UIElementFlags.DebugLayout;
            }
            else {
                selectedElement.flags &= ~UIElementFlags.DebugLayout;
            }

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Hover", StyleState.Hover);
            DrawStyleStateButton("Focus", StyleState.Focused);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Active", StyleState.Active);
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            showAllComputedStyles = EditorGUILayout.ToggleLeft("Show All", showAllComputedStyles);
            showComputedSources = EditorGUILayout.ToggleLeft("Show Sources", showComputedSources);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            searchString = searchField.OnGUI(searchString);
            GUILayout.Space(4);

            List<ValueTuple<string, StyleProperty>> properties = ListPool<ValueTuple<string, StyleProperty>>.Get();

            string lowerSearch = searchString.ToLower();

            for (int i = 0; i < StyleUtil.StylePropertyIdList.Length; i++) {
                StylePropertyId propertyId = StyleUtil.StylePropertyIdList[i];
                if (showAllComputedStyles || style.IsDefined(propertyId)) {
                    if (!string.IsNullOrEmpty(searchString)) {
                        string propertyName = StyleUtil.GetPropertyName(propertyId).ToLower();
                        if (!propertyName.Contains(lowerSearch)) {
                            continue;
                        }
                    }

                    string source = selectedElement.style.GetPropertySource(propertyId);
                    StyleProperty property = selectedElement.style.GetComputedStyleProperty(propertyId);
                    if (!property.hasValue) {
                        property = DefaultStyleValues_Generated.GetPropertyValue(propertyId);
                    }

                    properties.Add(ValueTuple.Create(source, property));
                }
            }

            if (properties.Count == 0) {
                return;
            }

            if (showComputedSources) {
                properties.Sort(
                    (a, b) => {
                        if (a.Item1 == b.Item1) return 0;

                        bool aInstance = a.Item1.Contains("Instance");
                        bool bInstance = b.Item1.Contains("Instance");

                        if (aInstance && bInstance) {
                            return string.Compare(a.Item1, b.Item1, StringComparison.Ordinal);
                        }

                        if (aInstance) return -1;
                        if (bInstance) return 1;

                        bool aDefault = a.Item1.Contains("Default");
                        bool bDefault = b.Item1.Contains("Default");

                        if (aDefault && bDefault) {
                            return string.Compare(a.Item1, b.Item1, StringComparison.Ordinal);
                        }

                        if (aDefault) return 1;
                        if (bDefault) return -1;

                        return string.Compare(a.Item1, b.Item1, StringComparison.Ordinal);
                    });

                GUILayout.Space(10);
                string currentSource = properties[0].Item1;
                EditorGUILayout.LabelField(currentSource);
                int start = 0;
                for (int i = 0; i < properties.Count; i++) {
                    if (currentSource != properties[i].Item1) {
                        properties.Sort(start, i - start, s_StyleCompare);

                        for (int j = start; j < i; j++) {
                            DrawStyleProperty(properties[j].Item2, false);
                        }

                        start = i;
                        currentSource = properties[i].Item1;
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField(currentSource);
                    }
                }

                properties.Sort(start, properties.Count - start, s_StyleCompare);
                for (int j = start; j < properties.Count; j++) {
                    DrawStyleProperty(properties[j].Item2, false);
                }
            }
            else {
                properties.Sort(0, properties.Count - 1, s_StyleCompare);
                for (int i = 0; i < properties.Count; i++) {
                    DrawStyleProperty(properties[i].Item2, false);
                }
            }
        }

        private static void DrawLabel(string label, string value) {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            EditorGUILayout.LabelField(value);
            GUILayout.EndHorizontal();
        }

        private static void DrawVector2Value(string label, Vector2 value) {
            DrawLabel(label, $"X: {value.x}, Y: {value.y}");
        }

        private static void DrawSizeValue(string label, Size value) {
            DrawLabel(label, $"Width: {value.width}, Height: {value.height}");
        }

        private void DrawAttributes(List<ElementAttribute> attributes) {
            DrawLabel("Attributes", "");
            EditorGUI.indentLevel++;
            for (int i = 0; i < attributes.Count; i++) {
                DrawLabel(attributes[i].name, attributes[i].value);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawElementInfo() {
            List<ElementAttribute> attributes = selectedElement.GetAttributes();
            if (attributes != null) {
                DrawAttributes(attributes);
            }

            GUI.enabled = true;
            LayoutResult layoutResult = selectedElement.layoutResult;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            Rect contentRect = layoutResult.ContentRect;

            DrawLabel("Enabled", selectedElement.isEnabled.ToString());
            if (selectedElement.isEnabled) {
                DrawLabel("Culled", selectedElement.layoutResult.isCulled.ToString());
                DrawLabel("View", selectedElement.View.name);
                DrawLabel("Viewport", $"X: {selectedElement.View.Viewport.x}, Y: {selectedElement.View.Viewport.y}, W: {selectedElement.View.Viewport.width}, H: {selectedElement.View.Viewport.height}");
                DrawVector2Value("Local Position", layoutResult.localPosition);
                DrawVector2Value("Screen Position", layoutResult.screenPosition);
                DrawVector2Value("Scale", layoutResult.scale);
                DrawSizeValue("Allocated Size", layoutResult.allocatedSize);
                DrawSizeValue("Actual Size", layoutResult.actualSize);

                DrawLabel("Rotation", layoutResult.rotation.ToString());
                DrawLabel("Content Rect", $"X: {contentRect.x}, Y: {contentRect.y}, W: {contentRect.width}, H: {contentRect.height}");

                DrawLabel("Render Layer", selectedElement.style.RenderLayer.ToString());

                GUILayout.Space(16);

                DrawEnumWithValue<LayoutType>(selectedElement.style.GetComputedStyleProperty(StylePropertyId.LayoutType), false);
                DrawMeasurement(selectedElement.style.GetComputedStyleProperty(StylePropertyId.PreferredWidth), false);
                DrawMeasurement(selectedElement.style.GetComputedStyleProperty(StylePropertyId.PreferredHeight), false);

                DrawLabel("Block Width Provider:", selectedElement.layoutBox.GetBlockWidthProvider() + " size: " + selectedElement.layoutBox.ComputeBlockWidth(1));
                DrawLabel("Block Height Provider:", selectedElement.layoutBox.GetBlockHeightProvider() + " size: " + selectedElement.layoutBox.ComputeBlockHeight(1));

                GUILayout.Space(16);

                OffsetRect margin = selectedElement.layoutResult.margin;
                DrawLabel("Margin Top", margin.top.ToString());
                DrawLabel("Margin Right", margin.right.ToString());
                DrawLabel("Margin Bottom", margin.bottom.ToString());
                DrawLabel("Margin Left", margin.left.ToString());

                GUILayout.Space(16);

                OffsetRect border = selectedElement.layoutResult.border;

                DrawLabel("Border Top", border.top.ToString());
                DrawLabel("Border Right", border.right.ToString());
                DrawLabel("Border Bottom", border.bottom.ToString());
                DrawLabel("Border Left", border.left.ToString());

                GUILayout.Space(16);

                OffsetRect padding = selectedElement.layoutResult.padding;
                DrawLabel("Padding Top", padding.top.ToString());
                DrawLabel("Padding Right", padding.right.ToString());
                DrawLabel("Padding Bottom", padding.bottom.ToString());
                DrawLabel("Padding Left", padding.left.ToString());
            }

            EditorGUIUtility.labelWidth = labelWidth;
        }

        private void DrawStyleStateButton(string name, StyleState styleState) {
            bool isInState = selectedElement.style.IsInState(styleState);
            s_Content.text = "Force " + name;
            bool toggle = EditorGUILayout.ToggleLeft(s_Content, isInState);
            if (!isInState && toggle) {
                selectedElement.style.EnterState(styleState);
            }
            else if (isInState && !toggle) {
                selectedElement.style.ExitState(styleState);
            }
        }

        private void DrawStyles() {
            UIStyleSet styleSet = selectedElement.style;

            List<UIStyleGroupContainer> baseStyles = styleSet.GetBaseStyles();

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Hover", StyleState.Hover);
            DrawStyleStateButton("Focus", StyleState.Focused);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawStyleStateButton("Active", StyleState.Active);
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            EditorGUIUtility.labelWidth = labelWidth;

            EditorGUILayout.BeginVertical();

            UIStyleGroup instanceStyle = styleSet.GetInstanceStyle();
            if (instanceStyle != null) {
                s_Content.text = "Instance";
                DrawStyleGroup("", instanceStyle);
            }

//
            for (int i = 0; i < baseStyles.Count; i++) {
                UIStyleGroupContainer container = baseStyles[i];
                s_Content.text = $"{container.name} ({container.styleType.ToString()})";

                for (int j = 0; j < container.groups.Length; j++) {
                    DrawStyleGroup(container.styleSheet?.path, container.groups[j]);
                }
            }

            ListPool<UIStyleGroupContainer>.Release(ref baseStyles);
            GUILayout.EndVertical();
        }

        private void DrawStyleGroup(string fileName, UIStyleGroup group) {
            if (group.normal.style != null) {
                DrawStyle(fileName, group.name + " [Normal]", group.normal.style);
                DrawRunCommands(group.normal.runCommands);
            }

            if (group.hover.style != default) {
                DrawStyle(fileName, group.name + " [Hover]", group.hover.style);
                DrawRunCommands(group.hover.runCommands);
            }

            if (group.focused.style != default) {
                DrawStyle(fileName, group.name + " [Focus]", group.focused.style);
                DrawRunCommands(group.focused.runCommands);
            }

            if (group.active.style != default) {
                DrawStyle(fileName, group.name + " [Active]", group.active.style);
                DrawRunCommands(group.active.runCommands);
            }
        }

        private void DrawRunCommands(LightList<IRunCommand> runCommands) {
            if (runCommands == null) {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"Run Commands");
            EditorGUI.indentLevel++;
            for (int i = 0; i < runCommands.size; i++) {
                if (runCommands[i] is AnimationRunCommand animationRunCommand) {
                    s_Content.text = "Name";
                    GUI.enabled = true;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(s_Content);
                    EditorGUILayout.TextField(animationRunCommand.animationData.name);
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel++;
                    s_Content.text = "File name";
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(s_Content);
                    EditorGUILayout.TextField(animationRunCommand.animationData.fileName);
                    GUILayout.EndHorizontal();
                    s_Content.text = "isExit";
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(s_Content);
                    // EditorGUILayout.TextField(animationRunCommand.IsExit ? "yes" : "no");
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        public void OnGUI() {
            EditorGUIUtility.wideMode = true;

            if (app == null) {
                return;
            }

            int elementId = UIForiaHierarchyWindow.s_SelectedElementId;

            selectedElement = app.GetElement(elementId);

            if (selectedElement == null) {
                GUILayout.Label("Select an element in the UIForia Hierarchy Window");
                return;
            }

            tab = GUILayout.Toolbar(tab, s_TabNames);

            EditorGUIUtility.labelWidth += 50;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            switch (tab) {
                case 0:
                    DrawElementInfo();
                    break;

                case 1:
                    DrawStyles();
                    break;

                case 2:
                    DrawComputedStyle();
                    break;
            }

            EditorGUIUtility.labelWidth -= 50;

            // set from code = defined & not in template & not in bound style 

            GUILayout.EndScrollView();
        }

        private void DrawStyle(string fileName, string name, UIStyle style) {
            bool expanded = true;

            if (m_ExpandedMap.ContainsKey(style)) {
                m_ExpandedMap.TryGetValue(style, out expanded);
            }

            expanded = EditorGUILayout.Foldout(expanded, name + "           " + fileName);
            m_ExpandedMap[style] = expanded;

            if (expanded) {
                EditorGUI.indentLevel++;
                // todo -- sort?
                for (int i = 0; i < style.PropertyCount; i++) {
                    DrawStyleProperty(style[i], false);
                }

                EditorGUI.indentLevel--;
            }
        }

        private static StyleProperty DrawStyleProperty(StyleProperty property, bool isEditable) {
            switch (property.propertyId) {
                case StylePropertyId.LayoutFitHorizontal:
                case StylePropertyId.LayoutFitVertical:
                    return DrawEnumWithValue<LayoutFit>(property, isEditable);

                case StylePropertyId.OverflowX:
                case StylePropertyId.OverflowY:
                    return DrawEnumWithValue<Overflow>(property, isEditable);

                case StylePropertyId.BackgroundColor:
                case StylePropertyId.BackgroundTint:
                case StylePropertyId.BorderColorTop:
                case StylePropertyId.BorderColorLeft:
                case StylePropertyId.BorderColorRight:
                case StylePropertyId.BorderColorBottom:
                    return DrawColor(property, isEditable);

                case StylePropertyId.BackgroundFit:
                    return DrawEnumWithValue<BackgroundFit>(property, isEditable);

                case StylePropertyId.Visibility:
                    return DrawEnumWithValue<Visibility>(property, isEditable);

                case StylePropertyId.Painter:
                    return DrawString(property, isEditable);

                case StylePropertyId.TextOutlineWidth:
                case StylePropertyId.TextGlowOffset:
                case StylePropertyId.TextGlowOuter:
                case StylePropertyId.TextGlowInner:
                case StylePropertyId.TextGlowPower:
                case StylePropertyId.TextUnderlayX:
                case StylePropertyId.TextUnderlayY:
                case StylePropertyId.TextUnderlayDilate:
                case StylePropertyId.TextUnderlaySoftness:
                case StylePropertyId.TextFaceDilate:
                case StylePropertyId.TextOutlineSoftness:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.MeshType:
                case StylePropertyId.MeshFillDirection:
                case StylePropertyId.MeshFillOrigin:
                    return DrawEnumWithValue<MeshType>(property, isEditable);

                case StylePropertyId.MeshFillAmount:
                    return DrawFloat(property, isEditable);
                
                case StylePropertyId.Layer:
                    return DrawInt(property, isEditable);

                case StylePropertyId.Material:
                    return DrawMaterial(property);

                case StylePropertyId.TextOutlineColor:
                case StylePropertyId.TextGlowColor:
                case StylePropertyId.TextUnderlayColor:
                case StylePropertyId.CaretColor:
                case StylePropertyId.SelectionBackgroundColor:
                case StylePropertyId.SelectionTextColor:
                    return DrawColor(property, isEditable);

                case StylePropertyId.TextUnderlayType:
                    return DrawEnumWithValue<UnderlayType>(property, isEditable);

                case StylePropertyId.RadialLayoutStartAngle:
                case StylePropertyId.RadialLayoutEndAngle:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.RadialLayoutRadius:
                case StylePropertyId.CornerBevelTopLeft:
                case StylePropertyId.CornerBevelTopRight:
                case StylePropertyId.CornerBevelBottomLeft:
                case StylePropertyId.CornerBevelBottomRight:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.AlignItemsHorizontal:
                case StylePropertyId.AlignItemsVertical:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.DistributeExtraSpaceHorizontal:
                case StylePropertyId.DistributeExtraSpaceVertical:
                    return DrawEnumWithValue<SpaceDistribution>(property, isEditable);

                case StylePropertyId.FitItemsHorizontal:
                case StylePropertyId.FitItemsVertical:
                    return DrawEnumWithValue<LayoutFit>(property, isEditable);

                case StylePropertyId.BackgroundImageOffsetX:
                case StylePropertyId.BackgroundImageOffsetY:
                case StylePropertyId.BackgroundImageScaleX:
                case StylePropertyId.BackgroundImageScaleY:
                case StylePropertyId.ShadowIntensity:
                case StylePropertyId.ShadowOpacity:
                case StylePropertyId.ShadowSizeX:
                case StylePropertyId.ShadowSizeY:
                case StylePropertyId.BackgroundImageRotation:
                case StylePropertyId.BackgroundImageTileX:
                case StylePropertyId.BackgroundImageTileY:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.BackgroundImage:
                    return DrawTextureAsset(property, isEditable);

                case StylePropertyId.Cursor:
                    return DrawCursor(property, isEditable);

                case StylePropertyId.Opacity:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.GridItemY:
                case StylePropertyId.GridItemHeight:
                case StylePropertyId.GridItemX:
                case StylePropertyId.GridItemWidth:
                    return DrawInt(property, isEditable);

                case StylePropertyId.GridLayoutDirection:
                    return DrawEnumWithValue<LayoutDirection>(property, isEditable);

                case StylePropertyId.GridLayoutDensity:
                    return DrawEnumWithValue<GridLayoutDensity>(property, isEditable);

                case StylePropertyId.GridLayoutColTemplate:
                case StylePropertyId.GridLayoutRowTemplate:
                    return DrawGridTemplate(property, isEditable);

                case StylePropertyId.GridLayoutColAutoSize:
                case StylePropertyId.GridLayoutRowAutoSize:
                    return DrawGridTemplate(property, isEditable);

                case StylePropertyId.GridLayoutColGap:
                case StylePropertyId.GridLayoutRowGap:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.GridLayoutColAlignment:
                case StylePropertyId.GridLayoutRowAlignment:
                    return DrawEnumWithValue<GridAxisAlignment>(property, isEditable);

                case StylePropertyId.FlexLayoutWrap:
                    return DrawEnumWithValue<LayoutWrap>(property, isEditable);

                case StylePropertyId.FlexLayoutDirection:
                    return DrawEnumWithValue<LayoutDirection>(property, isEditable);

                case StylePropertyId.FlexItemGrow:
                case StylePropertyId.FlexItemShrink:
                    return DrawInt(property, isEditable);

                case StylePropertyId.MarginTop:
                case StylePropertyId.MarginRight:
                case StylePropertyId.MarginBottom:
                case StylePropertyId.MarginLeft:
                    return DrawMeasurement(property, isEditable);

                case StylePropertyId.BorderTop:
                case StylePropertyId.BorderRight:
                case StylePropertyId.BorderBottom:
                case StylePropertyId.BorderLeft:
                case StylePropertyId.PaddingTop:
                case StylePropertyId.PaddingRight:
                case StylePropertyId.PaddingBottom:
                case StylePropertyId.PaddingLeft:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.BorderRadiusTopLeft:
                case StylePropertyId.BorderRadiusTopRight:
                case StylePropertyId.BorderRadiusBottomLeft:
                case StylePropertyId.BorderRadiusBottomRight:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.TransformPositionX:
                case StylePropertyId.TransformPositionY:
                case StylePropertyId.ShadowOffsetX:
                case StylePropertyId.ShadowOffsetY:
                    return DrawOffsetMeasurement(property, isEditable);

                case StylePropertyId.TransformScaleX:
                case StylePropertyId.TransformScaleY:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.TransformPivotX:
                case StylePropertyId.TransformPivotY:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.TransformRotation:
                    return DrawFloat(property, isEditable);

                case StylePropertyId.TextColor:
                case StylePropertyId.ShadowColor:
                case StylePropertyId.ShadowTint:
                    return DrawColor(property, isEditable);

                case StylePropertyId.TextFontAsset:
                    return DrawFontAsset(property, isEditable);

                case StylePropertyId.TextFontSize:
                    return DrawFixedLength(property, isEditable);

                case StylePropertyId.TextFontStyle:
                    // todo -- this needs to be an EnumFlags popup
                    return DrawEnumWithValue<FontStyle>(property, isEditable);
                //                    return DrawEnum<Text.FontStyle>(property, isEditable);

                case StylePropertyId.TextAlignment:
                    return DrawEnumWithValue<TextAlignment>(property, isEditable);

                case StylePropertyId.TextWhitespaceMode:
                    return DrawEnumWithValue<WhitespaceMode>(property, isEditable);

                //
                case StylePropertyId.TextTransform:
                    return DrawEnumWithValue<TextTransform>(property, isEditable);

                case StylePropertyId.AlignmentTargetX:
                case StylePropertyId.AlignmentTargetY:
                    return DrawEnumWithValue<AlignmentTarget>(property, isEditable);

                case StylePropertyId.AlignmentDirectionX:
                case StylePropertyId.AlignmentDirectionY:
                    return DrawEnumWithValue<AlignmentDirection>(property, isEditable);

                case StylePropertyId.AlignmentOffsetX:
                case StylePropertyId.AlignmentOffsetY:
                case StylePropertyId.AlignmentOriginX:
                case StylePropertyId.AlignmentOriginY:
                    return DrawOffsetMeasurement(property, isEditable);

                case StylePropertyId.MinWidth:
                case StylePropertyId.MaxWidth:
                case StylePropertyId.PreferredWidth:
                case StylePropertyId.MinHeight:
                case StylePropertyId.MaxHeight:
                case StylePropertyId.PreferredHeight:
                    return DrawMeasurement(property, isEditable);

                case StylePropertyId.LayoutType:
                    return DrawEnumWithValue<LayoutType>(property, isEditable);

                case StylePropertyId.LayoutBehavior:
                    return DrawEnumWithValue<LayoutBehavior>(property, isEditable);
                
                case StylePropertyId.ScrollBehaviorX:
                    return DrawEnumWithValue<ScrollBehavior>(property, isEditable);
                case StylePropertyId.ScrollBehaviorY:
                    return DrawEnumWithValue<ScrollBehavior>(property, isEditable);

                case StylePropertyId.ZIndex:
                case StylePropertyId.RenderLayerOffset:
                    return DrawInt(property, isEditable);

                case StylePropertyId.RenderLayer:
                    return DrawEnumWithValue<RenderLayer>(property, isEditable);

                case StylePropertyId.ClipBehavior:
                    return DrawEnumWithValue<ClipBehavior>(property, isEditable);

                case StylePropertyId.ClipBounds:
                    return DrawEnumWithValue<ClipBounds>(property, isEditable);

                case StylePropertyId.AlignmentBoundaryX:
                    return DrawEnumWithValue<AlignmentBoundary>(property, isEditable);

                case StylePropertyId.AlignmentBoundaryY:
                    return DrawEnumWithValue<AlignmentBoundary>(property, isEditable);

                case StylePropertyId.PointerEvents:
                    return DrawEnumWithValue<PointerEvents>(property, isEditable);

                default:
                    Debug.Log(property.propertyId.ToString() + " has no inspector");
                    return StyleProperty.Unset(property.propertyId);
            }
        }

        private static StyleProperty DrawMaterial(in StyleProperty property) {
            if (property.AsMaterialId.id == 0) {
                s_Content.text = "Material";
                GUI.enabled = false;
                EditorGUILayout.TextField(s_Content, "None");
                GUI.enabled = true;
                return property;
            }

            if (UIForiaHierarchyWindow.s_SelectedApplication.materialDatabase.TryGetMaterial(property.AsMaterialId, out MaterialInfo info)) { }

            return property;

        }

        private static StyleProperty DrawCursor(StyleProperty property, bool isEditable) {
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            Texture2D texture = property.AsCursorStyle?.texture;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(StyleUtil.GetPropertyName(property));
            Texture2D newTexture = (Texture2D) EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            // todo fix return value
            return property;
        }

        private static ValueTuple<int[], GUIContent[]> GetEnumValues<T>() {
            ValueTuple<int[], GUIContent[]> retn;
            if (!m_EnumValueMap.TryGetValue(typeof(T), out retn)) {
                T[] vals = (T[]) Enum.GetValues(typeof(T));
                int[] intValues = new int[vals.Length];
                GUIContent[] contentValues = new GUIContent[vals.Length];

                for (int i = 0; i < vals.Length; i++) {
                    intValues[i] = (int) (object) vals[i];
                    contentValues[i] = new GUIContent(vals[i].ToString());
                }

                retn = ValueTuple.Create(intValues, contentValues);
                m_EnumValueMap[typeof(T)] = retn;
            }

            return retn;
        }

        private static StyleProperty DrawEnumWithValue<T>(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            ValueTuple<int[], GUIContent[]> tuple = GetEnumValues<T>();

            int[] values = tuple.Item1;
            GUIContent[] displayOptions = tuple.Item2;
            int index = Array.IndexOf(values, property.int0);
            int output = EditorGUILayout.Popup(s_Content, index, displayOptions);
            // unclear if output is a value or an index, I suspect index
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, values[output]) : property;
        }

        private static StyleProperty DrawColor(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            Color value = EditorGUILayout.ColorField(s_Content, property.AsColor);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, value) : property;
        }

        private static StyleProperty DrawInt(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            float value = EditorGUILayout.IntField(s_Content, property.AsInt);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, value) : property;
        }

        private static StyleProperty DrawString(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            string value = EditorGUILayout.TextField(s_Content, property.AsString);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, value) : property;
        }

        private static StyleProperty DrawFloat(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            float value = EditorGUILayout.FloatField(s_Content, property.AsFloat);
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, value) : property;
        }

        private static StyleProperty DrawFixedLength(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUILayout.BeginHorizontal();
            GUI.enabled = isEditable;
            float value = EditorGUILayout.FloatField(s_Content, property.AsUIFixedLength.value);
            UIFixedUnit unit = (UIFixedUnit) EditorGUILayout.EnumPopup(property.AsUIFixedLength.unit);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, new UIFixedLength(value, unit)) : property;
        }

        private static StyleProperty DrawOffsetMeasurement(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUILayout.BeginHorizontal();
            GUI.enabled = isEditable;
            float value = EditorGUILayout.FloatField(s_Content, property.AsUIFixedLength.value);
            OffsetMeasurementUnit unit = (OffsetMeasurementUnit) EditorGUILayout.EnumPopup(property.AsUIFixedLength.unit);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, new OffsetMeasurement(value, unit)) : property;
        }

        private static StyleProperty DrawMeasurement(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            float value = EditorGUILayout.FloatField(s_Content, property.AsUIMeasurement.value);
            UIMeasurementUnit unit = (UIMeasurementUnit) EditorGUILayout.EnumPopup(property.AsUIMeasurement.unit);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, new UIMeasurement(value, unit)) : property;
        }

        private static StyleProperty DrawTextureAsset(StyleProperty property, bool isEditable) {
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            Texture2D texture = property.AsTexture;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(StyleUtil.GetPropertyName(property));
            Texture2D newTexture = (Texture2D) EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, newTexture) : property;
        }

        private static StyleProperty DrawFontAsset(StyleProperty property, bool isEditable) {
            GUI.enabled = isEditable;
            GUILayout.BeginHorizontal();
            FontAsset fontAsset = property.AsFont;
            
            TMP_FontAsset newFont = (TMP_FontAsset) EditorGUILayout.ObjectField(StyleUtil.GetPropertyName(property), fontAsset.textMeshProFont, typeof(TMP_FontAsset), false);

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            return isEditable ? new StyleProperty(property.propertyId, default(FontAsset)) : property;
        }

        private static StyleProperty DrawGridTemplate(StyleProperty property, bool isEditable) {
            s_Content.text = StyleUtil.GetPropertyName(property);
            GUI.enabled = isEditable;
            IReadOnlyList<GridTrackSize> template = property.AsGridTrackTemplate;
            EditorGUILayout.BeginHorizontal();
            if (template == null) {
                EditorGUILayout.LabelField("Undefined");
            }
            else {
                EditorGUILayout.LabelField(s_Content);
                for (int i = 0; i < template.Count; i++) {
                    float value = EditorGUILayout.FloatField(template[i].cell.baseSize.value);
                    GridTemplateUnit unit = (GridTemplateUnit) EditorGUILayout.EnumPopup(template[i].cell.baseSize.unit);
                }
            }

            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            return isEditable ? new StyleProperty(property.propertyId, default(IReadOnlyList<GridTrackSize>)) : property;
        }

    }

    public class StylePropertyIdComparer : IComparer<ValueTuple<string, StyleProperty>> {

        public int Compare(ValueTuple<string, StyleProperty> x, ValueTuple<string, StyleProperty> y) {
            return (int) x.Item2.propertyId > (int) y.Item2.propertyId ? 1 : -1;
        }

    }

}