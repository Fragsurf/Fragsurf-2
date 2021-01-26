using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<AttributeNodeContainer> s_AttributeGroupContainerNodePool = new ObjectPool<AttributeNodeContainer>();

        internal static AttributeNodeContainer AttributeGroupRootNode(string identifier, string value, bool invert, AttributeNodeContainer next) {
            AttributeNodeContainer rootNode = s_AttributeGroupContainerNodePool.Get();
            rootNode.type = StyleASTNodeType.AttributeGroup;
            rootNode.identifier = identifier;
            rootNode.value = value;
            rootNode.invert = invert;
            rootNode.next = next;
            return rootNode;
        }
    }

    public class SelectNode : StyleNodeContainer { }

    public class AttributeNodeContainer : ChainableNodeContainer {

        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_AttributeGroupContainerNodePool.Release(this);
        }
    }
}
