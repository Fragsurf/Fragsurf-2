using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
            
        internal static readonly ObjectPool<RgbNode> s_RgbNodePool = new ObjectPool<RgbNode>();
        
        internal static RgbNode RgbNode(StyleASTNode red, StyleASTNode green, StyleASTNode blue) {
            RgbNode retn = s_RgbNodePool.Get();
            retn.red = red;
            retn.green = green;
            retn.blue = blue;
            return retn;
        }
    }

    public class RgbNode : StyleASTNode {

        public StyleASTNode red;
        public StyleASTNode green;
        public StyleASTNode blue;

        public RgbNode() {
            type = StyleASTNodeType.Rgb;
        }

        public override void Release() {
            StyleASTNodeFactory.s_RgbNodePool.Release(this);
        }

        public override string ToString() {
            return $"rgb({red}, {green}, {blue})";
        }
    }
}
