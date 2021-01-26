using System;

namespace UIForia.Rendering {

    [Flags]
    public enum StyleType : byte {

        /// Styles applied to the element type, overrides the built-in default styles
        Implicit = 1 << 1,

        /// Regular style applied to one or more elements, overrides implicit styles
        Shared = 1 << 2,

        /// Set only on one element, like inline styles, overrides all other styles
        Instance = 1 << 3,
        
        Default = 1 << 4,
        
        Important = 1 << 5

    }

}