namespace UIForia.Parsing {

    public struct TemplateLineInfo {

        public int line;
        public int column;

        public TemplateLineInfo(int line, int column) {
            this.line = line;
            this.column = column;
        }

        public override string ToString() {
            return line + ":" + column;
        }

    }

}