namespace UIForia.Text {

    public enum WordType {
        Whitespace,
        NewLine,
        Normal,
        SoftHyphen,
    }

    public struct WordInfo {
        public WordType type;
        public int charStart;
        public int charEnd;
        public float width;
        public float height;
        public float yOffset;
        public int LastCharacterIndex => charEnd - 1;
    }
}
