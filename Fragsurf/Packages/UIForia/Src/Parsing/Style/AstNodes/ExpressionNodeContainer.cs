using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {
    
    internal static partial class StyleASTNodeFactory {
    
        internal static readonly ObjectPool<ExpressionNodeContainer> s_ExpressionGroupContainerNodePool = new ObjectPool<ExpressionNodeContainer>();

        internal static ExpressionNodeContainer ExpressionGroupRootNode(string identifier, bool invert, AttributeNodeContainer next) {
            ExpressionNodeContainer rootNode = s_ExpressionGroupContainerNodePool.Get();
            rootNode.type = StyleASTNodeType.ExpressionGroup;
            rootNode.identifier = identifier;
            return rootNode;
        }
    }
    
    public class ExpressionNodeContainer : ChainableNodeContainer {

        // TODO add expression node
        
        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_ExpressionGroupContainerNodePool.Release(this);
        }
    }
}