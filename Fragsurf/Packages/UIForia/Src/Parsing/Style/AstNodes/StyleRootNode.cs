using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {
    
    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<StyleRootNode> s_StyleRootNodePool = new ObjectPool<StyleRootNode>();

        internal static StyleRootNode StyleRootNode(string identifier, string tagName) {
            StyleRootNode rootNode = s_StyleRootNodePool.Get();
            rootNode.identifier = identifier;
            rootNode.tagName = tagName;
            return rootNode;
        }

    }

    /// <summary>
    /// Container for all the things inside a style node: 'style xy { children... }'
    /// </summary>
    public class StyleRootNode : StyleNodeContainer {

        public string tagName;

        public StyleRootNode() {
            type = StyleASTNodeType.StyleGroup;
        }

        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_StyleRootNodePool.Release(this);
        }

        public override string ToString() {
            return $"style {identifier ?? $"<{tagName}>"} {{ {children} }}";
        }
    }
}
