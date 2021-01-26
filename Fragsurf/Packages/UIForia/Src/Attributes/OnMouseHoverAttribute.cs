using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnMouseHoverAttribute : MouseEventHandlerAttribute {

        public OnMouseHoverAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseHover, phase) { }

        public OnMouseHoverAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseHover, phase) { }

    }
}