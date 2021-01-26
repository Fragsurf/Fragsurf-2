using UnityEngine;

namespace UIForia.UIInput {

    public struct KeyboardInputEvent  {

        public readonly bool isFocused;
        public readonly KeyCode keyCode;
        public readonly KeyboardModifiers modifiers;
        public readonly char character;
        public InputEventType eventType;
        private readonly EventPropagator source;

        public KeyboardInputEvent(EventPropagator source, InputEventType type, KeyCode keyCode, char character, KeyboardModifiers modifiers, bool isFocused) {
            this.source = source;
            this.eventType = type;
            this.keyCode = keyCode;
            this.modifiers = modifiers;
            this.character = character;
            this.isFocused = isFocused;
        }

        public void Consume() {
            source.isConsumed = true;
        }

        public void StopPropagation() {
            source.shouldStopPropagation = true;
        }
        
        public bool IsConsumed => source.isConsumed;

        public bool alt => (modifiers & KeyboardModifiers.Alt) != 0;

        public bool shift => (modifiers & KeyboardModifiers.Shift) != 0;

        public bool ctrl => (modifiers & KeyboardModifiers.Control) != 0;

        public bool onlyControl => ctrl && !alt && !shift;

        public bool command => (modifiers & KeyboardModifiers.Command) != 0;

        public bool numLock => (modifiers & KeyboardModifiers.NumLock) != 0;

        public bool capsLock => (modifiers & KeyboardModifiers.CapsLock) != 0;

        public bool windows => (modifiers & KeyboardModifiers.Windows) != 0;

    }

}