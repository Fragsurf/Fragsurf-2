using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnMouseContextAttribute : MouseEventHandlerAttribute {

        public OnMouseContextAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseContext, phase) { }

        public OnMouseContextAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseContext, phase) { }

    }
}