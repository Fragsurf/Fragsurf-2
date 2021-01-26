using System;
using UIForia.UIInput;
using UnityEngine;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class KeyboardInputBindingAttribute : Attribute {

        public readonly char character;
        public readonly KeyCode key;
        public readonly KeyboardModifiers modifiers;
        public readonly InputEventType eventType;
        public readonly bool requiresFocus;
        public readonly EventPhase keyEventPhase;

        protected KeyboardInputBindingAttribute(KeyCode key, char character, KeyboardModifiers modifiers, InputEventType eventType, bool requiresFocusKeyEventPhase, EventPhase keyEventPhase) {
            this.key = key;
            this.character = character;
            this.modifiers = modifiers;
            this.eventType = eventType;
            this.requiresFocus = requiresFocusKeyEventPhase;
            this.keyEventPhase = keyEventPhase;
        }

    }
}