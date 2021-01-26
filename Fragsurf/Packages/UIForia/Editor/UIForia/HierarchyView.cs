using System;
using System.Collections.Generic;
using UIForia.Editor;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class HierarchyView : TreeView {

    private struct ViewState {

        public bool showTemplateContents;

    }

    public UIView[] views;

    private readonly IntMap<ViewState> m_ViewState;

    public bool needsReload;
    public event Action<UIElement> onSelectionChanged;

    private static readonly GUIStyle s_ElementNameStyle;
    private static readonly GUIStyle s_ElementTemplateRootStyle;
    private static readonly GUIContent s_Content = new GUIContent();

    public bool showChildrenAndId = false;
    public bool showDisabled = false;
    public bool selectMode = false;
    public bool showLayoutStats = false;

    static HierarchyView() {
        s_ElementNameStyle = new GUIStyle();
        GUIStyleState elementNameNormal = new GUIStyleState();
        GUIStyleState elementStyleNormal = new GUIStyleState();
        elementNameNormal.textColor = UIForiaEditorTheme.elementNameNormal;
        elementStyleNormal.textColor = UIForiaEditorTheme.elementStyleNormal;
        s_ElementNameStyle.normal = elementNameNormal;
    }

    public HierarchyView(UIView[] views, TreeViewState state) : base(state) {
        this.views = views;
        m_ViewState = new IntMap<ViewState>();
        needsReload = true;
    }

    public void Destroy() {
        onSelectionChanged = null;
    }

    protected override TreeViewItem BuildRoot() {
        Stack<ElementTreeItem> stack = StackPool<ElementTreeItem>.Get();

        // todo -- maybe pool tree items

        TreeViewItem root = new TreeViewItem(-9999, -1);

        foreach (UIView uiView in views) {
            if (uiView.RootElement == null) continue;
            if (uiView.RootElement.isDisabled && !showDisabled) continue;

            ElementTreeItem firstChild = new ElementTreeItem(uiView.RootElement);
            firstChild.displayName = uiView.RootElement.ToString();
            stack.Push(firstChild);

            while (stack.Count > 0) {
                ElementTreeItem current = stack.Pop();
                if (current.element.isDisabled && !showDisabled) {
                    continue;
                }

                UIElement element = current.element;

                List<UIElement> ownChildren = element.GetChildren();

                if (ownChildren.Count == 0) {
                    ListPool<UIElement>.Release(ref ownChildren);
                    continue;
                }

                for (int i = 0; i < ownChildren.Count; i++) {
                    ElementTreeItem childItem = new ElementTreeItem(ownChildren[i]);
                    if (childItem.element.isDisabled && !showDisabled) {
                        continue;
                    }
                    childItem.displayName = ownChildren[i].ToString();
                    current.AddChild(childItem);
                    stack.Push(childItem);
                }
            }

            root.AddChild(firstChild);
        }

        root.displayName = "ROOT";
        SetupDepthsFromParentsAndChildren(root);
        needsReload = false;
        if (root.children == null) {
            root.children = new List<TreeViewItem>();
        }
        return root;
    }


    public bool RunGUI() {
        OnGUI(GUILayoutUtility.GetRect(0, 10000, 0, 10000));
        return needsReload;
    }

    private static string GetCullText(CullResult result) {
        switch (result) {
            case CullResult.NotCulled:
                return string.Empty;

            case CullResult.ClipRectIsZero:
                return "[Culled - Fully Clipped]";

            case CullResult.ActualSizeZero:
                return "[Culled - Size is zero]";

            case CullResult.OpacityZero:
                return "[Culled - Opacity is zero]";

            case CullResult.VisibilityHidden:
                return "[Culled - Visibility Hidden]";

            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }

    protected override void RowGUI(RowGUIArgs args) {
        ElementTreeItem item = (ElementTreeItem) args.item;
        GUIStyleState textStyle = s_ElementNameStyle.normal;

        bool isTemplateRoot = (item.element.flags & UIElementFlags.TemplateRoot) != 0;
        
        Color mainColor = isTemplateRoot
            ? UIForiaEditorTheme.mainColorTemplateRoot 
            : UIForiaEditorTheme.mainColorRegularChild;

        if (item.element.style.LayoutBehavior == LayoutBehavior.TranscludeChildren) {
            mainColor = UIForiaEditorTheme.mainColorChildrenElement;
        }

        textStyle.textColor = AdjustColor(mainColor, item.element);

        float indent = GetContentIndent(args.item);
        float rowWidth = args.rowRect.width;
        args.rowRect.x += indent;
        args.rowRect.width -= indent;
        s_Content.text = item.element.GetDisplayName();

        if (showLayoutStats) {
            s_Content.text += $"w: {item.element.layoutBox.cacheHit}, {item.element.layoutBox.cacheMiss}";
        }
        
        if (item.element.isEnabled && item.element.renderBox != null) {
            if (item.element.renderBox.overflowX != Overflow.Visible || item.element.renderBox.overflowY != Overflow.Visible) {
                s_Content.text += " [Clipper]";
            }    
        }
        
        if((item.element.flags & UIElementFlags.DebugLayout) != 0) {
            s_Content.text = "{Debug Layout} " + s_Content.text;
        }
        
        Vector2 v = s_ElementNameStyle.CalcSize(s_Content);
        Rect r = new Rect(args.rowRect);
        GUI.Label(args.rowRect, s_Content, s_ElementNameStyle);
        r.x += v.x + 5f;
        r.width -= v.x + 5f;

        List<string> names = ListPool<string>.Get();

        item.element.style.GetStyleNameList(names);
        string styleName = string.Empty;

        for (int i = 0; i < names.Count; i++) {
            styleName += names[i] + " ";
        }

        ListPool<string>.Release(ref names);

        if (styleName.Length > 0) {
            styleName = '[' + styleName.TrimEnd() + "] ";
        }

        s_Content.text = styleName; // + "(children: " + box.children.Count + ", id: " + item.element.id + ")";

        if (showChildrenAndId) {
            s_Content.text += "(id: " + item.element?.id + ")";
        }

        textStyle.textColor = AdjustColor(UIForiaEditorTheme.elementStyleNormal, item.element);

        GUI.Label(r, s_Content, s_ElementNameStyle);

        v = s_ElementNameStyle.CalcSize(s_Content);
        r.x += v.x + 5f;
        r.width -= v.x + 5f;

        r = DrawAdditionalInfo(item.element, r);

        if (!isTemplateRoot) {
            return;
        }

        ViewState viewState;
        m_ViewState.TryGetValue(item.element.id, out viewState);

        r.x = rowWidth - 16;
        r.width = 16;
        s_Content.text = viewState.showTemplateContents ? "+" : "-";
        GUI.Label(r, s_Content);

        if (Event.current.type == EventType.MouseDown) {
            if (r.Contains(Event.current.mousePosition)) {
                viewState.showTemplateContents = !viewState.showTemplateContents;
                m_ViewState[item.element.id] = viewState;
                needsReload = true;
            }
        }
    }

    private static Rect DrawAdditionalInfo(UIElement element, Rect rect) {
        if (element is UITextElement textElement) {
            if (!string.IsNullOrEmpty(textElement.text)) {
                if (textElement.text.Length <= 20) {
                    s_Content.text = '"' + textElement.text.Trim() + '"';
                }
                else {
                    s_Content.text = '"' + textElement.text.Substring(0, 20).Trim() + "...\"";
                }

                s_ElementNameStyle.normal.textColor = AdjustColor(UIForiaEditorTheme.elementNameNormal, element);
                GUI.Label(rect, s_Content, s_ElementNameStyle);
                Vector2 size = s_ElementNameStyle.CalcSize(s_Content);
                rect.x += size.x;
                rect.width -= size.x;
            }
        }
        
        return rect;
    }

    private static Color AdjustColor(Color color, UIElement element) {
        return element.isEnabled ? color : new Color(color.r, color.g, color.b, 0.25f);
    }

    protected override void SelectionChanged(IList<int> selectedIds) {
        if (selectedIds.Count == 0) {
            onSelectionChanged?.Invoke(null);
            return;
        }

        int id = selectedIds[0];
        UIElement element = UIForiaHierarchyWindow.s_SelectedApplication.GetElement(id);
        onSelectionChanged?.Invoke(element);
    }

}