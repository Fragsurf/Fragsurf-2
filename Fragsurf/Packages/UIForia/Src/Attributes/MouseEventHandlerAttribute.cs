using System;
using UIForia.UIInput;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class MouseEventHandlerAttribute : Attribute {

        public readonly KeyboardModifiers modifiers;
        public readonly InputEventType eventType;
        public readonly EventPhase phase;

        protected MouseEventHandlerAttribute(KeyboardModifiers modifiers, InputEventType eventType, EventPhase phase) {
            this.modifiers = modifiers;
            this.eventType = eventType;
            this.phase = phase;
        }

    }
}