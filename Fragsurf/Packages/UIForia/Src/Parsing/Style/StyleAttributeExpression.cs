namespace UIForia.Parsing.Style {
    public class StyleAttributeExpression {
      
        private int left;
        private int right;
        
        private StyleAttributeExpressionOperation op;

        public StyleAttributeExpression(int left, int right, StyleAttributeExpressionOperation op) {
            this.left = left;
            this.right = right;
            this.op = op;
        }

        public int Execute() {
            return op.Execute(left, right);
        }
    }
}
