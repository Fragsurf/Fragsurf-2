using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<StyleLiteralNode> s_LiteralPool = new ObjectPool<StyleLiteralNode>();

        internal static StyleLiteralNode StringLiteralNode(string value) {
            StyleLiteralNode retn = s_LiteralPool.Get();
            retn.type = StyleASTNodeType.StringLiteral;
            retn.rawValue = value;
            return retn;
        }

        internal static StyleLiteralNode BooleanLiteralNode(string value) {
            StyleLiteralNode retn = s_LiteralPool.Get();
            retn.type = StyleASTNodeType.BooleanLiteral;
            retn.rawValue = value;
            return retn;
        }

        internal static StyleLiteralNode NumericLiteralNode(string value) {
            StyleLiteralNode retn = s_LiteralPool.Get();
            retn.type = StyleASTNodeType.NumericLiteral;
            retn.rawValue = value;
            return retn;
        }
    }

    public class StyleLiteralNode : StyleASTNode {

        public string rawValue;

        public override void Release() {
            rawValue = null;
            type = StyleASTNodeType.Invalid;
            StyleASTNodeFactory.s_LiteralPool.Release(this);
        }

        protected bool Equals(StyleLiteralNode other) {
            return string.Equals(rawValue, other.rawValue);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StyleLiteralNode) obj);
        }

        public override int GetHashCode() {
            return (rawValue != null ? rawValue.GetHashCode() : 0);
        }

        public override string ToString() {
            return type == StyleASTNodeType.StringLiteral ? $@"""{rawValue}""" : rawValue;
        }
    }

}
