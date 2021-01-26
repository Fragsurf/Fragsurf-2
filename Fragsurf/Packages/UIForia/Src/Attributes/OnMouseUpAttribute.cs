using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnMouseUpAttribute : MouseEventHandlerAttribute {

        public OnMouseUpAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseUp, phase) { }

        public OnMouseUpAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseUp, phase) { }

    }
}