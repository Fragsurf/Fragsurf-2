using UnityEngine;

namespace UIForia.Text {

    public struct LineInfo {

        // screen coords
        public float width;
        public float height;
        public float x;
        public float y;
        
        public Rect LineRect => new Rect(x, y, width, height);

        // indices 
        // word index into the first span
        public int wordStart;
        // ... into the last span
        public int wordEnd;

        // index into the span array of this line
        public int spanStart;
        public int spanEnd;

        // total word count across all spans in this line
        public int wordCount;
        public int globalCharacterStartIndex;
        public int globalCharacterEndIndex;

        public int LastWordIndex => wordEnd - 1;

        public LineInfo(int spanStart, int wordStart, float width = 0) {
            this.wordStart = wordStart;
            this.wordEnd = 0;
            this.spanStart = spanStart;
            this.spanEnd = 0;
            this.x = 0;
            this.y = 0;
            this.width = width;
            this.height = 0;
            this.wordCount = 0;
            this.globalCharacterStartIndex = 0;
            this.globalCharacterEndIndex = 0;
        }

    }

}
