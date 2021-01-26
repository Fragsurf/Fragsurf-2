using System;

namespace UIForia.Rendering {

    [Flags]
    public enum StyleState : byte {

        // todo -- reorganize by priority since this is a sort key
        Normal = 1 << 0,
        Hover = 1 << 1,
        Active = 1 << 2,
        Focused = 1 << 3,

    }

}