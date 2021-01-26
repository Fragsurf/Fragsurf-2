namespace UIForia.Systems {

    public enum UITaskResult {

        Running = 1 << 0,
        Completed = 1 << 1,
        Restarted = 1 << 2, // no effect on completion
        Failed = 1 << 3,
        Cancelled = Completed | (1 << 4), // like failing but counts as completed
        Paused = 1 << 5,
    }

}