using System;

namespace UIForia.Layout {

    [Flags]
    public enum LayoutRenderFlag {

        NeedsLayout = 1 << 0,
        Ignored = 1 << 1,
        Transclude = 1 << 2,
        ClipWidth = 1 << 3,
        ClipHeight = 1 << 4,
        IgnoreClip = 1 << 5,
        Clip = 1 << 6

    }

}