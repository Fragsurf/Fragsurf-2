using System;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.UIInput {

    public abstract class DragEvent {

        public readonly Type type;
        internal EventPropagator source;
        public UIElement origin { get; internal set; }
        public UIElement element { get; internal set; }
        public bool lockCursor;
        public CursorStyle cursor;

        protected DragEvent() {
            this.type = GetType();
        }

        public Vector2 MousePosition { get; internal set; }
        public Vector2 MousePositionInvertY => new Vector2(MousePosition.x, element.application.Height - MousePosition.y);
        public Vector2 DragStartPosition { get; internal set; }
        public KeyboardModifiers Modifiers { get; internal set; }
        public InputEventType CurrentEventType { get; internal set; }

        public float StartTime { get; internal set; }
        public bool IsCanceled { get; protected set; }
        public bool IsDropped { get; }

        public bool IsConsumed => source.isConsumed;

        public Vector2 DragDelta => source.mouseState.MouseDelta;

        public bool Alt => (Modifiers & KeyboardModifiers.Alt) != 0;

        public bool Shift => (Modifiers & KeyboardModifiers.Shift) != 0;

        public bool Ctrl => (Modifiers & KeyboardModifiers.Control) != 0;

        public bool OnlyControl => Ctrl && !Alt && !Shift;

        public bool Command => (Modifiers & KeyboardModifiers.Command) != 0;

        public bool NumLock => (Modifiers & KeyboardModifiers.NumLock) != 0;

        public bool CapsLock => (Modifiers & KeyboardModifiers.CapsLock) != 0;

        public bool Windows => (Modifiers & KeyboardModifiers.Windows) != 0;

        public bool IsMouseLeftDown => source.mouseState.isLeftMouseDown;
        public bool IsMouseLeftDownThisFrame => source.mouseState.isLeftMouseDownThisFrame;
        public bool IsMouseLeftUpThisFrame => source.mouseState.isLeftMouseUpThisFrame;

        public bool IsMouseRightDown => source.mouseState.isRightMouseDown;
        public bool IsMouseRightDownThisFrame => source.mouseState.isRightMouseDownThisFrame;
        public bool IsMouseRightUpThisFrame => source.mouseState.isRightMouseUpThisFrame;

        public bool IsMouseMiddleDown => source.mouseState.isMiddleMouseDown;
        public bool IsMouseMiddleDownThisFrame => source.mouseState.isMiddleMouseDownThisFrame;
        public bool IsMouseMiddleUpThisFrame => source.mouseState.isMiddleMouseUpThisFrame;


        public void StopPropagation() {
            if (source != null) {
                source.shouldStopPropagation = true;
            }
        }

        public void Consume() {
            if (source != null) {
                source.isConsumed = true;
            }
        }

        public virtual void Begin() { }

        public virtual void Update() { }

        public virtual void Drop(bool success) { }

        public virtual void Cancel() { }

        public virtual void OnComplete() { }

    }

}