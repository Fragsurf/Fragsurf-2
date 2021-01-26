using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnMouseHeldDownAttribute : MouseEventHandlerAttribute {

        public OnMouseHeldDownAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(modifiers, InputEventType.MouseHeldDown, phase) { }


        public OnMouseHeldDownAttribute(EventPhase phase)
            : base(KeyboardModifiers.None, InputEventType.MouseHeldDown, phase) { }

    }
}