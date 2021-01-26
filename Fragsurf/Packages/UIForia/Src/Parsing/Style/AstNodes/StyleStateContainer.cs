using UIForia.Parsing.Style.Tokenizer;
using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<StyleStateContainer> s_StyleContainerNodePool = new ObjectPool<StyleStateContainer>();

        internal static StyleStateContainer StateGroupRootNode(StyleToken token) {
            StyleStateContainer rootNode = s_StyleContainerNodePool.Get();
            rootNode.type = StyleASTNodeType.StateGroup;
            rootNode.identifier = token.value;
            rootNode.WithLocation(token);
            return rootNode;
        }
    }

    public class StyleStateContainer : StyleNodeContainer {
        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_StyleContainerNodePool.Release(this);
        }
    }
}
