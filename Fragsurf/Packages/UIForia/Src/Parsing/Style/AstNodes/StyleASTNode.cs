using UIForia.Parsing.Style.Tokenizer;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
    }

    public abstract class StyleASTNode {

        public StyleASTNodeType type = StyleASTNodeType.None;

        public int line;
        public int column;

        public StyleASTNode WithLocation(StyleToken token) {
            this.line = token.line;
            this.column = token.column;
            return this;
        }

        public bool IsCompound {
            get {
                if (type == StyleASTNodeType.Operator) {
                    return true;
                }

                return false;
            }
        }

        public abstract void Release();
    }
}
