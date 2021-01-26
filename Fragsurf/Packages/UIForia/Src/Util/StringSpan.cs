namespace UIForia.Util {

    public struct StringSpan {

        public readonly string str;
        public readonly int start;
        public readonly int length;
        
        public StringSpan(string str, int start, int length) {
            this.str = str;
            this.start = start;
            this.length = length;
        }

        public static int Split(string input, char c, int start, out StringSpan span) {
            for (int i = start; i < input.Length; i++) {
                if (input[i] == c) {
                    span = new StringSpan(input, start, i - start);
                }
            }

            span = new StringSpan(null, -1, -1);
            return input.Length;
        }
        
    }

}