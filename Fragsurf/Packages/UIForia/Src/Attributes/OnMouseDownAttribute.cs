using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnMouseDownAttribute : MouseEventHandlerAttribute {

        public OnMouseDownAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseDown, phase) { }


        public OnMouseDownAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseDown, phase) { }

    }
}