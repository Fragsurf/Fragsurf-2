using UIForia.Rendering;

namespace UIForia.Compilers.Style {

    // todo -- convert to struct
    public class UIStyleGroupContainer {

        public int id;
        public string name;
        public StyleType styleType;
        public UIStyleGroup[] groups; // can this become an int[]?
        public readonly bool hasAttributeStyles;
        public StyleSheet styleSheet;

        public UIStyleGroupContainer(int id, string name, StyleType styleType, UIStyleGroup[] groups) {
            this.id = id;
            this.name = name;
            this.styleType = styleType;
            this.groups = groups;
            for (int i = 0; i < groups.Length; i++) {
                if (groups[i].HasAttributeRule) {
                    hasAttributeStyles = true;
                    break;
                }
            }
        }

    }

}