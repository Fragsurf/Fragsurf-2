using System;

namespace UIForia.UIInput {

    [Flags]
    public enum MouseEventType {

        Enter,
        Exit,
        Hover,
        DragStart,
        DragEnd,
        DragCancel,
        DragUpdate,
        Click,
        ButtonPress0,
        ButtonPress1,
        ButtonPress2,
        ScrollX,
        ScrollY

    }
}