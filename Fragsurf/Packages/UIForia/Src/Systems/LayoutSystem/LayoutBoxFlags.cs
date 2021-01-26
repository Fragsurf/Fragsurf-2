using System;

namespace UIForia.Systems {

    [Flags]
    public enum LayoutBoxFlags {

        None = 0,
        RequireLayoutHorizontal = 1,
        RequireLayoutVertical = 1 << 1,
        Ignored = 1 << 2,
        AlwaysUpdate = 1 << 3,
            

        WidthBlockProvider = 1 << 4,
        HeightBlockProvider = 1 << 5,

        RequireAlignmentHorizontal = 1 << 6,
        RequireAlignmentVertical = 1 << 7,

        RequiresMatrixUpdate = 1 << 8,

        RecomputeClipping = 1 << 9,

        Clipper = 1 << 10,

        GatherChildren = 1 << 11,

        TypeOrBehaviorChanged = 1 << 12,

        TransformDirty = 1 << 13,

        ContentAreaWidthChanged = 1 << 14,
        ContentAreaHeightChanged = 1 << 15,

        IdentityTransform = 1 << 16

    }

}