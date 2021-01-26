using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnMouseEnterAttribute : MouseEventHandlerAttribute {

        public OnMouseEnterAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseEnter, phase) { }

        public OnMouseEnterAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseEnter, phase) { }

    }
}