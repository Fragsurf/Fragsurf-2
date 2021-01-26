using UIForia.Elements;
using UnityEditor.IMGUI.Controls;

public class ElementTreeItem : TreeViewItem {

    public readonly UIElement element;

    public ElementTreeItem(UIElement element) : base(element.id, element.hierarchyDepth) {
        this.element = element;
    }

}