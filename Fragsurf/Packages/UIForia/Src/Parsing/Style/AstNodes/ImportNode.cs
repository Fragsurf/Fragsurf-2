using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<ImportNode> s_ImportNodePool = new ObjectPool<ImportNode>();

        internal static ImportNode ImportNode(string alias, string source) {
            ImportNode importNode = s_ImportNodePool.Get();
            importNode.alias = alias;
            importNode.source = source;
            return importNode;
        }
    }

    public class ImportNode : StyleASTNode {

        public string alias;
        public string source;

        public ImportNode() {
            type = StyleASTNodeType.Import;
        }

        public override void Release() {
            StyleASTNodeFactory.s_ImportNodePool.Release(this);
        }
    }
}
