using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
    
        internal static readonly ObjectPool<RgbaNode> s_RgbaNodePool = new ObjectPool<RgbaNode>();

        internal static RgbaNode RgbaNode(StyleASTNode red, StyleASTNode green, StyleASTNode blue, StyleASTNode alpha) {
            RgbaNode retn = s_RgbaNodePool.Get();
            retn.red = red;
            retn.green = green;
            retn.blue = blue;
            retn.alpha = alpha;
            return retn;
        }
    }

    public class RgbaNode : StyleASTNode {

        public StyleASTNode red;
        public StyleASTNode green;
        public StyleASTNode blue;
        public StyleASTNode alpha;

        public RgbaNode() {
            type = StyleASTNodeType.Rgba;
        }

        public override void Release() {
            StyleASTNodeFactory.s_RgbaNodePool.Release(this);
        }
        
        public override string ToString() {
            return $"rgba({red}, {green}, {blue}, {alpha})";
        }
    }
}