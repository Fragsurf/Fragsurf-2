using System;
using UIForia.Layout;

namespace UIForia.Sound {
    [Flags]
    public enum UITimeMeasurementUnit {
        Unset = UnitConstants.Unset,
        Percentage = UnitConstants.Percent,
        Seconds = UnitConstants.Seconds,
        Milliseconds = UnitConstants.Milliseconds,
    }
}
