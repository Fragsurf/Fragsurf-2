using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<DotAccessNode> s_DotAccessPool = new ObjectPool<DotAccessNode>();

        internal static DotAccessNode DotAccessNode(string propertyName) {
            DotAccessNode dotAccessNode = s_DotAccessPool.Get();
            dotAccessNode.propertyName = propertyName;
            return dotAccessNode;
        }
    }

    public class DotAccessNode : StyleASTNode {
        public string propertyName;

        public DotAccessNode() {
            type = StyleASTNodeType.DotAccess;
        }

        public override void Release() {
            StyleASTNodeFactory.s_DotAccessPool.Release(this);
        }

        public override string ToString() {
            return $".{propertyName}";
        }
    }
}
