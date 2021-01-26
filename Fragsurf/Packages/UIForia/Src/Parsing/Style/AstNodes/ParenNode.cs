using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<ParenNode> s_ParenPool = new ObjectPool<ParenNode>();

        internal static ParenNode ParenNode(StyleASTNode expression) {
            ParenNode parenNode = s_ParenPool.Get();
            parenNode.expression = expression;
            return parenNode;
        }
    }
    
    public class ParenNode : StyleASTNode {

        public StyleASTNode expression;

        public ParenNode() {
            type = StyleASTNodeType.Paren;
        }

        public override void Release() {
            expression?.Release();
            StyleASTNodeFactory.s_ParenPool.Release(this);
        }

    }
}