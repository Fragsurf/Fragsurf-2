using System;

namespace UIForia.Rendering {

    [Flags]
    public enum CullResult {

        NotCulled,
        ClipRectIsZero,
        ActualSizeZero,
        OpacityZero,
        VisibilityHidden

    }

}