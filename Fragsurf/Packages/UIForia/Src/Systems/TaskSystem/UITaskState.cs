using System;

namespace UIForia.Systems {

    [Flags]
    public enum UITaskState {

        Uninitialized = 0,
        Pending = 1 << 0,
        Restarting = 1 << 1,
        Running = 1 << 2,
        Completed = 1 << 3,
        Failed = 1 << 5,
        Cancelled = 1 << 6,
        Paused = 1 << 7,

        ReRunnable = Uninitialized | Completed | Failed | Cancelled,
        Pausable = Pending | Restarting | Running,
        Stoppable = Pausable | Paused,
    }

}