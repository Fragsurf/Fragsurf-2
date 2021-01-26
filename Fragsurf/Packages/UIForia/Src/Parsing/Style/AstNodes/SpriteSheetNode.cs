using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {
    
    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<SpriteSheetNode> s_SpriteSheetNodePool = new ObjectPool<SpriteSheetNode>();

        internal static SpriteSheetNode SpriteSheetNode(string identifier) {
            SpriteSheetNode node = s_SpriteSheetNodePool.Get();
            node.identifier = identifier;
            node.type = StyleASTNodeType.SpriteSheetDeclaration;
            return node;
        }

    }

    public class SpriteSheetNode : StyleNodeContainer {

    }
}
