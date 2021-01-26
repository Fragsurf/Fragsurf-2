using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    public class StyleSystem2 { }


    public enum StyleHook { }

    public struct Style { }

    // [StructLayout(LayoutKind.Explicit)]
    public struct StyleGroup {

        public StyleType type;
        public StyleState state;

    }

    public class UIStyleSet2 {

        public UIElement element { get; internal set; }
        internal StyleState styleState;
        internal StructList<StyleGroup> groups;

        public void SetProperty(StyleProperty property, StyleState styleState = StyleState.Normal) {
            if (groups == null) {
                groups = new StructList<StyleGroup>(2);
                groups[0] = new StyleGroup() { };
            }

            for (int i = 0; i < groups.size; i++) {
                ref StyleGroup styleGroup = ref groups.array[i];
                if (styleGroup.type == StyleType.Instance && styleGroup.state == styleState) { }
            }
        }

    }

}