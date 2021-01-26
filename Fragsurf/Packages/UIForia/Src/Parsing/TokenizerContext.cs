namespace UIForia.Parsing {
    internal class TokenizerContext {
        public int line;
        public int column;
        public int ptr;
        public readonly string input;

        private int savedLine;
        private int savedColumn;
        private int savedPtr;

        public TokenizerContext(string input) {
            this.line = 1;
            this.column = 1;
            this.ptr = 0;
            this.input = input;
        }

        public TokenizerContext Save() {
            savedLine = line;
            savedColumn = column;
            savedPtr = ptr;
            return this;
        }

        public TokenizerContext Restore() {
            line = savedLine;
            column = savedColumn;
            ptr = savedPtr;
            return this;
        }

        public TokenizerContext Advance() {
            if (ptr >= input.Length) return this;
            if (input[ptr] == '\n') {
                line++;
                column = 1;
            }
            else {
                column++;
            }

            ptr++;

            return this;
        }

        public TokenizerContext Advance(int characters) {
            while (HasMore() && characters > 0) {
                Advance();
                characters--;
            }

            return this;
        }

        public bool IsConsumed() {
            return ptr >= input.Length;
        }

        public bool HasMore() {
            return ptr < input.Length;
        }

        public bool HasMuchMore(int howMuchMore) {
            return ptr + howMuchMore < input.Length;
        }
    }
}