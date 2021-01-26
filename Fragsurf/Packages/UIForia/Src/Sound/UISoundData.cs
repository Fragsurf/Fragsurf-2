namespace UIForia.Sound {
    public struct UISoundData {
        public string name;
        public string styleSheetFileName;

        public string asset;
        public float pitch;
        public float volume;
        public FloatRange pitchRange;
        public float tempo;
        public UITimeMeasurement duration;
        public int iterations;
        public string mixerGroup;
    }
}
