using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems.Input {

    public struct KeyCodeState {

        public char character;
        public KeyCode keyCode;
        public KeyState keyState;

    }

    public class KeyboardInputState {

        private readonly StructList<KeyCodeState> m_KeyStates;

        public KeyboardModifiers modifiersThisFrame;

        public KeyboardInputState() {
            this.m_KeyStates = new StructList<KeyCodeState>();
        }

        public StructList<KeyCodeState> GetKeyCodeStates() {
            return m_KeyStates;
        }

        public KeyState GetKeyState(KeyCode keyCode) {
            for (int i = 0; i < m_KeyStates.size; i++) {
                KeyCodeState mKeyState = m_KeyStates[i];
                if (mKeyState.keyCode == keyCode) {
                    return mKeyState.keyState;
                }
            }

            return KeyState.Up;
        }

        public KeyState GetKeyState(char character) {
            for (int i = 0; i < m_KeyStates.size; i++) {
                KeyCodeState mKeyState = m_KeyStates[i];
                if (mKeyState.character == character) {
                    return mKeyState.keyState;
                }
            }

            return KeyState.Up;
        }

        public void SetKeyState(KeyCode keyCode, KeyState keyState) {
            for (int i = 0; i < m_KeyStates.size; i++) {
                KeyCodeState mKeyState = m_KeyStates[i];
                if (mKeyState.keyCode == keyCode) {
                    mKeyState.keyState = keyState;
                    return;
                }
            }

            m_KeyStates.Add(new KeyCodeState {keyCode = keyCode, keyState = keyState});
        }

        public void SetKeyState(char character, KeyState keyState) {
            for (int i = 0; i < m_KeyStates.size; i++) {
                KeyCodeState mKeyState = m_KeyStates[i];
                if (mKeyState.character == character) {
                    mKeyState.keyState = keyState;
                    return;
                }
            }

            m_KeyStates.Add(new KeyCodeState {character = character, keyState = keyState});
        }

        public void CleanupForThisFrame() {
            for (int i = m_KeyStates.size - 1; i >= 0; i--) {
                ref KeyCodeState mKeyState = ref m_KeyStates.array[i];
                if (mKeyState.keyState == KeyState.Up || (mKeyState.keyState & KeyState.UpThisFrame) != 0) {
                    m_KeyStates.RemoveAt(i);
                }
                else if ((mKeyState.keyState & KeyState.DownThisFrame) != 0) {
                    if (UnityEngine.Input.GetKey(mKeyState.keyCode)) {
                        mKeyState.keyState = KeyState.Down;
                    }
                    else {
                        mKeyState.keyState = KeyState.UpThisFrame;
                    }
                }
            }
        }

        public bool IsKeyDown(KeyCode keyCode) {
            return (GetKeyState(keyCode) & KeyState.Down) != 0;
        }

        public bool IsKeyDownThisFrame(KeyCode keyCode) {
            return (GetKeyState(keyCode) & KeyState.DownThisFrame) != 0;
        }

        public bool IsKeyUp(KeyCode keyCode) {
            KeyState state = GetKeyState(keyCode);
            return (state == KeyState.Up || (state & KeyState.UpThisFrame) != 0);
        }

        public bool IsKeyUpThisFrame(KeyCode keyCode) {
            return (GetKeyState(keyCode) & KeyState.UpThisFrame) != 0;
        }

        public bool IsKeyDown(char character) {
            return (GetKeyState(character) & KeyState.Down) != 0;
        }

        public bool IsKeyDownThisFrame(char character) {
            return (GetKeyState(character) & KeyState.DownThisFrame) != 0;
        }

        public bool IsKeyUp(char character) {
            KeyState state = GetKeyState(character);
            return (state == KeyState.Up || (state & KeyState.UpThisFrame) != 0);
        }

        public bool IsKeyUpThisFrame(char character) {
            return (GetKeyState(character) & KeyState.UpThisFrame) != 0;
        }

    }

}
