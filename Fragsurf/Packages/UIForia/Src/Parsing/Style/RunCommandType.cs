using System;

namespace UIForia.Parsing.Style {

    [Flags]
    public enum RunCommandType {

        Enter = 1 << 1, 
        Exit =  1 << 2,
        EnterExit = Enter | Exit

    }

}