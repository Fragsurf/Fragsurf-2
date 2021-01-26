using UIForia.Elements;
using UnityEngine;

namespace UIForia.UIInput {

    public interface IInputProvider {

        KeyState GetKeyState(KeyCode keyCode);

        bool IsKeyDown(KeyCode keyCode);
        bool IsKeyDownThisFrame(KeyCode keyCode);
        bool IsKeyUp(KeyCode keyCode);
        bool IsKeyUpThisFrame(KeyCode keyCode);

        bool IsMouseLeftDown { get; }
        bool IsMouseLeftDownThisFrame { get; }
        bool IsMouseLeftUpThisFrame { get; }

        bool IsMouseRightDown { get; }
        bool IsMouseRightDownThisFrame { get; }
        bool IsMouseRightUpThisFrame { get; }

        bool IsMouseMiddleDown { get; }
        bool IsMouseMiddleDownThisFrame { get; }
        bool IsMouseMiddleUpThisFrame { get; }

        Vector2 ScrollDelta { get; }
        Vector2 MousePosition { get; }

        Vector2 MouseDownPosition { get; }
        
        bool IsDragging { get; }

        IFocusable GetFocusedElement();
        bool RequestFocus(IFocusable target);
        void ReleaseFocus(IFocusable target);

        void RegisterFocusable(IFocusable focusable);
        void UnRegisterFocusable(IFocusable focusable);
        void FocusNext();
        void FocusPrevious();

        void DelayEvent(UIElement origin, UIEvent evt);

    }

}