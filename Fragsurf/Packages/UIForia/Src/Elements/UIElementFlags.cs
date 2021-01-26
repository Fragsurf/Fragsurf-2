using System;

namespace UIForia.Elements {

    [Flags]
    internal enum UIElementFlags {

        // Element Flags
        ImplicitElement = 1,
        Enabled = 1 << 1,
        AncestorEnabled = 1 << 2,
        Alive = 1 << 3,
        HasBeenEnabled = 1 << 4,
        Primitive = 1 << 5,
        Created = 1 << 6, // can maybe get rid fo this when revisiting 
        TemplateRoot = 1 << 7,
        IndexChanged = 1 << 8,
    
        InTagIndex = 1 << 9,
        
        NeedsUpdate = 1 << 10,
        
        SelfAndAncestorEnabled = Alive | Enabled | AncestorEnabled,

        // Layout Flags
        DebugLayout = 1 << 16,
        LayoutHierarchyDirty = 1 << 19,
        LayoutTransformNotIdentity = 1 << 20,
        LayoutFitWidthDirty = 1 << 24,
        LayoutFitHeightDirty = 1 << 25,
        LayoutTypeOrBehaviorDirty = 1 << 26,

        EnabledFlagSet = Alive | Enabled | AncestorEnabled,
        EnabledFlagSetWithUpdate = EnabledFlagSet | NeedsUpdate,

    }

}