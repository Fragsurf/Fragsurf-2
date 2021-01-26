using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<ConstNode> s_ConstNodePool = new ObjectPool<ConstNode>();
        
        internal static ConstNode ConstNode(string name, StyleASTNode value) {
            ConstNode constNode = s_ConstNodePool.Get();
            constNode.constName = name;
            constNode.value = value;
            return constNode;
        }
    }

    public class ConstNode : StyleASTNode {

        public string constName;

        public StyleASTNode value;

        public ConstNode() {
            type = StyleASTNodeType.Const;
        }

        public override void Release() {
            StyleASTNodeFactory.s_ConstNodePool.Release(this);
        }

        public override string ToString() {
            return $"{constName} = {value}";
        }
    }
}
