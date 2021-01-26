using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<ExportNode> s_ExportNodePool = new ObjectPool<ExportNode>();

        internal static ExportNode ExportNode(ConstNode constNode) {
            ExportNode exportNode = s_ExportNodePool.Get();
            exportNode.constNode = constNode;
            return exportNode;
        }
    }

    public class ExportNode : StyleASTNode {

        public ConstNode constNode;

        public ExportNode() {
            type = StyleASTNodeType.Export;
        }

        public override void Release() {
            StyleASTNodeFactory.s_ExportNodePool.Release(this);
        }

        public override string ToString() {
            return $"export {constNode}";
        }
    }
}
