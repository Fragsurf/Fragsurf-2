using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnMouseMoveAttribute : MouseEventHandlerAttribute {

        public OnMouseMoveAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseMove, phase) { }

        public OnMouseMoveAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseMove, phase) { }

    }
}