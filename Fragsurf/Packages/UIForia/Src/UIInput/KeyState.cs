using System;

namespace UIForia.UIInput {

    [Flags]
    public enum KeyState {

        Up = 0,
        Down = 1 << 0,
        DownThisFrame = Down | (1 << 2),
        UpThisFrame = Up | (1 << 3),
        
    }

}