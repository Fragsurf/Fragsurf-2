using System;

namespace UIForia.Layout {

    [Flags]
    public enum UIMeasurementUnit {

        Unset = UnitConstants.Unset,
        Pixel = UnitConstants.Pixel,
        Content = UnitConstants.Content,
        BlockSize = UnitConstants.ParentSize,
        ViewportWidth = UnitConstants.ViewportWidth,
        ViewportHeight = UnitConstants.ViewportHeight,
        ParentContentArea = UnitConstants.ParentContentArea,
        Em = UnitConstants.Em,
        Percentage = UnitConstants.Percent,
        IntrinsicMinimum = UnitConstants.MinContent,
        IntrinsicPreferred = UnitConstants.MaxContent,
        FitContent = UnitConstants.FitContent,
        BackgroundWidth = UnitConstants.BackgroundWidth,
        BackgroundHeight = UnitConstants.BackgroundHeight,

        Auto = 0

    }

}