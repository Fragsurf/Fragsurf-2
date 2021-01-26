using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {
    
    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<StyleFunctionNode> s_StyleFunctionNodePool = new ObjectPool<StyleFunctionNode>();

        internal static StyleFunctionNode StyleFunctionNode(string functionName) {
            StyleFunctionNode functionNode = s_StyleFunctionNodePool.Get();
            functionNode.identifier = functionName;
            functionNode.type = StyleASTNodeType.Function;
            return functionNode;
        }

    }
    
    public class StyleFunctionNode : StyleNodeContainer {
        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_StyleFunctionNodePool.Release(this);
        }
    }


}