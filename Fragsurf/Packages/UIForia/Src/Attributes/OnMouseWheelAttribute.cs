using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnMouseWheelAttribute : MouseEventHandlerAttribute {

        public OnMouseWheelAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseScroll, phase) { }

        public OnMouseWheelAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseScroll, phase) { }

    }
}
