using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
       
        internal static readonly ObjectPool<UnaryExpressionNode> s_UnaryNodePool = new ObjectPool<UnaryExpressionNode>();

        public static UnaryExpressionNode UnaryExpressionNode(StyleASTNodeType nodeType, StyleASTNode expr) {
            UnaryExpressionNode unaryNode = s_UnaryNodePool.Get();
            unaryNode.type = nodeType;
            unaryNode.expression = expr;
            return unaryNode;
        } 
        
    }
    

    public class UnaryExpressionNode : StyleASTNode {

        public StyleASTNode expression;
        public TypePath typePath;

        public override void Release() {
            typePath.Release();
            expression?.Release();
            StyleASTNodeFactory.s_UnaryNodePool.Release(this);
        }

    }
}