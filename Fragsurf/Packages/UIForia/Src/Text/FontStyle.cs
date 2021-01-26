using System;

namespace UIForia.Text {

    [Flags]
    public enum FontStyle {

        Unset = 0,
        Normal = 1 << 1,
        Bold = 1 << 2,
        Italic = 1 << 3,
        Underline = 1 << 4,
        StrikeThrough = 1 << 5

    }

}