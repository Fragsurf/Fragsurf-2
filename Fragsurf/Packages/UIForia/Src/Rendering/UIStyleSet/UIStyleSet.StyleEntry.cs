using UIForia.Util;

namespace UIForia.Rendering {

    public partial class UIStyleSet {

        internal struct StyleEntry {

            public readonly UIStyleRunCommand styleRunCommand;
            public readonly StyleState state;
            public readonly StyleType type;
            public readonly int priority;
            public readonly UIStyleGroup sourceGroup;

            //style number is used to prioritize shared styles, higher numbers are less important
            public StyleEntry(UIStyleGroup sourceGroup, UIStyleRunCommand styleRunCommand, StyleType type, StyleState state, int styleNumber, int attributeCount) {
                this.sourceGroup = sourceGroup;
                this.styleRunCommand = styleRunCommand;
                this.type = type;
                this.state = state;
                this.priority = (int)BitUtil.SetBytes(styleNumber, attributeCount, (int) type, (int) state);

            }

        }

    }

}