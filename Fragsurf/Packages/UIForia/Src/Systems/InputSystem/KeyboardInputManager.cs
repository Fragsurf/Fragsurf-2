using UIForia.UIInput;
using UnityEngine;

namespace UIForia.Systems.Input {

    public class KeyboardInputManager {

        private readonly KeyboardInputState keyboardInputState;

        private static readonly Event s_Event = new Event();

        public KeyboardInputManager() {
            keyboardInputState = new KeyboardInputState();
        }

        public virtual KeyboardInputState UpdateKeyboardInputState() {
            keyboardInputState.CleanupForThisFrame();

            HandleShiftKey(KeyCode.LeftShift);
            HandleShiftKey(KeyCode.RightShift);

            UpdateModifier(keyboardInputState.IsKeyDown(KeyCode.LeftShift) || keyboardInputState.IsKeyDown(KeyCode.RightShift), KeyboardModifiers.Shift);
            UpdateModifier(keyboardInputState.IsKeyDown(KeyCode.LeftAlt) || keyboardInputState.IsKeyDown(KeyCode.RightAlt), KeyboardModifiers.Alt);
            UpdateModifier(keyboardInputState.IsKeyDown(KeyCode.LeftCommand) || keyboardInputState.IsKeyDown(KeyCode.RightCommand) || keyboardInputState.IsKeyDown(KeyCode.LeftApple) || keyboardInputState.IsKeyDown(KeyCode.RightApple), KeyboardModifiers.Command);

            UpdateModifier(keyboardInputState.IsKeyDown(KeyCode.LeftControl) || keyboardInputState.IsKeyDown(KeyCode.RightControl), KeyboardModifiers.Control);
            UpdateModifier(keyboardInputState.IsKeyDown(KeyCode.CapsLock), KeyboardModifiers.CapsLock);
            UpdateModifier(keyboardInputState.IsKeyDown(KeyCode.Numlock), KeyboardModifiers.NumLock);

            while (Event.PopEvent(s_Event)) {
                if (!s_Event.isKey) {
                    continue;
                }

                KeyCode keyCode = s_Event.keyCode;
                char character = s_Event.character;

                // need to check this on osx, according to stackoverflow OSX and Windows might handle
                // sending key events differently

                if (s_Event.rawType == EventType.ExecuteCommand || s_Event.rawType == EventType.ValidateCommand) {
                    switch (s_Event.commandName) {
                        case "SelectAll":
                            ProcessEventType(EventType.KeyDown, KeyCode.A, 'a');
                            continue;
                        case "Copy":
                            ProcessEventType(EventType.KeyDown, KeyCode.C, 'c');
                            continue;
                        case "Cut":
                            ProcessEventType(EventType.KeyDown, KeyCode.X, 'x');
                            continue;
                        case "Paste":
                            ProcessEventType(EventType.KeyDown, KeyCode.V, 'v');
                            continue;
                        case "SoftDelete":
                            Debug.Log("Delete");
                            continue;
                        case "Duplicate":
                            Debug.Log("Duplicate");
                            continue;
                        case "Find":
                            continue;
                        // "Copy", "Cut", "Paste", "Delete", "SoftDelete", "Duplicate", "FrameSelected", "FrameSelectedWithLock", "SelectAll", "Find"
                    }
                }

                if (keyCode == KeyCode.None && character != '\0') {
                    if (s_Event.rawType == EventType.KeyDown) {
                        keyboardInputState.SetKeyState(character, keyboardInputState.IsKeyDown(character) ? KeyState.Down : KeyState.DownThisFrame);
                        HandleModifierDown(keyCode);
                        continue;
                    }

                    if (s_Event.rawType == EventType.KeyUp) {
                        keyboardInputState.SetKeyState(character, KeyState.UpThisFrame);
                        HandleModifierUp(keyCode);
                        continue;
                    }
                }

                ProcessEventType(s_Event.rawType, s_Event.keyCode, s_Event.character);
            }

            return keyboardInputState;
        }

        private void UpdateModifier(bool isDown, KeyboardModifiers modifier) {
            if (isDown) {
                keyboardInputState.modifiersThisFrame |= modifier;
            }
            else {
                keyboardInputState.modifiersThisFrame &= ~modifier;
            }
        }

        private void HandleShiftKey(KeyCode code) {
            bool wasDown = keyboardInputState.IsKeyDown(code);
            bool isDown = UnityEngine.Input.GetKey(code);
            if ((wasDown && !isDown) || UnityEngine.Input.GetKeyUp(code)) {
                keyboardInputState.SetKeyState(code, KeyState.UpThisFrame);
            }
            else if (UnityEngine.Input.GetKeyDown(code)) {
                keyboardInputState.SetKeyState(code, KeyState.DownThisFrame);
            }
            else if (isDown) {
                keyboardInputState.SetKeyState(code, KeyState.Down);
            }
        }

        private void HandleModifierDown(KeyCode keyCode) {
            switch (keyCode) {
                case KeyCode.LeftAlt:
                case KeyCode.RightAlt:
                    keyboardInputState.modifiersThisFrame |= KeyboardModifiers.Alt;
                    break;
                case KeyCode.LeftControl:
                case KeyCode.RightControl:
                    keyboardInputState.modifiersThisFrame |= KeyboardModifiers.Control;
                    break;
                case KeyCode.LeftCommand:
                case KeyCode.RightCommand:
                    keyboardInputState.modifiersThisFrame |= KeyboardModifiers.Command;
                    break;
                case KeyCode.LeftWindows:
                case KeyCode.RightWindows:
                    keyboardInputState.modifiersThisFrame |= KeyboardModifiers.Windows;
                    break;
                case KeyCode.LeftShift:
                case KeyCode.RightShift:
                    keyboardInputState.modifiersThisFrame |= KeyboardModifiers.Shift;
                    break;
                case KeyCode.Numlock:
                    keyboardInputState.modifiersThisFrame |= KeyboardModifiers.NumLock;
                    break;
                case KeyCode.CapsLock:
                    keyboardInputState.modifiersThisFrame |= KeyboardModifiers.CapsLock;
                    break;
            }
        }

        private void ProcessEventType(EventType evtType, KeyCode keyCode, char character) {
            switch (evtType) {
                case EventType.KeyDown:
                    KeyState keyState = keyboardInputState.GetKeyState(keyCode);
                    if (keyState != KeyState.DownThisFrame) {
                        keyboardInputState.SetKeyState(keyCode, KeyState.DownThisFrame);
                    }

                    HandleModifierDown(keyCode);
                    break;

                case EventType.KeyUp:
                    keyboardInputState.SetKeyState(keyCode, KeyState.UpThisFrame);
                    HandleModifierUp(keyCode);
                    break;
            }
        }

        private void HandleModifierUp(KeyCode keyCode) {
            switch (keyCode) {
                case KeyCode.LeftAlt:
                    if (!UnityEngine.Input.GetKey(KeyCode.RightAlt)) {
                        keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.Alt;
                    }

                    break;
                case KeyCode.RightAlt:
                    if (!UnityEngine.Input.GetKey(KeyCode.LeftAlt)) {
                        keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.Alt;
                    }

                    break;
                case KeyCode.LeftControl:
                    if (!UnityEngine.Input.GetKey(KeyCode.RightControl)) {
                        keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.Control;
                    }

                    break;
                case KeyCode.RightControl:
                    if (!UnityEngine.Input.GetKey(KeyCode.LeftControl)) {
                        keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.Control;
                    }

                    break;
                case KeyCode.LeftCommand:
                    if (!UnityEngine.Input.GetKey(KeyCode.RightCommand)) {
                        keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.Command;
                    }

                    break;
                case KeyCode.RightCommand:
                    if (!UnityEngine.Input.GetKey(KeyCode.LeftCommand)) {
                        keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.Command;
                    }

                    break;
                case KeyCode.LeftWindows:
                    if (!UnityEngine.Input.GetKey(KeyCode.RightWindows)) {
                        keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.Windows;
                    }

                    break;
                case KeyCode.RightWindows:
                    if (!UnityEngine.Input.GetKey(KeyCode.LeftWindows)) {
                        keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.Windows;
                    }

                    break;
                case KeyCode.LeftShift:
                    if (!UnityEngine.Input.GetKey(KeyCode.RightShift)) {
                        keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.Shift;
                    }

                    break;
                case KeyCode.RightShift:
                    if (!UnityEngine.Input.GetKey(KeyCode.LeftShift)) {
                        keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.Shift;
                    }

                    break;
                case KeyCode.Numlock:
                    keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.NumLock;
                    break;
                case KeyCode.CapsLock:
                    keyboardInputState.modifiersThisFrame &= ~KeyboardModifiers.CapsLock;
                    break;
            }
        }

    }
}
