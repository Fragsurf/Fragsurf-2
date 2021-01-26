using System;

namespace UIForia.UIInput {

    [Flags]
    public enum InputEventType {

        MouseEnter = 1 << 0,
        MouseExit = 1 << 1,
        MouseUp = 1 << 2,
        MouseDown = 1 << 3,
        MouseHeldDown = 1 << 4,
        MouseMove = 1 << 5,
        MouseHover = 1 << 6,
        MouseContext = 1 << 7,
        MouseScroll = 1 << 8,
        MouseClick = 1 << 9,

        KeyDown = 1 << 10,
        KeyUp = 1 << 11,
        KeyHeldDown = 1 << 12,

        Focus = 1 << 13,
        Blur = 1 << 14,

        DragCreate = 1 << 15,
        DragMove = 1 << 16,
        DragHover = 1 << 17,
        DragEnter = 1 << 18,
        DragExit = 1 << 19,
        DragDrop = 1 << 20,
        DragCancel = 1 << 21,

        DragUpdate = DragMove | DragHover,
        MouseUpdate = MouseMove | MouseHover,



    }

}