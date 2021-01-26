using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnMouseClickAttribute : MouseEventHandlerAttribute {

        public OnMouseClickAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseClick, phase) { }

        public OnMouseClickAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseClick, phase) { }

    }
}