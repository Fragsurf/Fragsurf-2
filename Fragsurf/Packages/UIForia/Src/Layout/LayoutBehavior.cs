using System;

namespace UIForia.Layout {

    [Flags]
    public enum LayoutBehavior {

        Unset = 0,
        Normal = 1 << 0,
        Ignored = 1 << 1,
        TranscludeChildren = 1 << 2

    }

}