using System;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class OnKeyDownWithFocusAttribute : KeyboardInputBindingAttribute {

        public OnKeyDownWithFocusAttribute(KeyCode key = KeyCodeUtil.AnyKey, KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase keyEventPhase = EventPhase.Bubble)
            : base(key, '\0', modifiers, InputEventType.KeyDown, true, keyEventPhase) {
        }

        public OnKeyDownWithFocusAttribute(char character, KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase keyEventPhase = EventPhase.Bubble)
            : base(KeyCodeUtil.AnyKey, character, modifiers, InputEventType.KeyDown, true, keyEventPhase) {
        }

    }
}
