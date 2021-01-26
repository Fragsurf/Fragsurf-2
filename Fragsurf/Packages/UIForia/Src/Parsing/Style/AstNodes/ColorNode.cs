using UIForia.Util;
using UnityEngine;

namespace UIForia.Parsing.Style.AstNodes {
    
    internal static partial class StyleASTNodeFactory {
        internal static readonly ObjectPool<ColorNode> s_ColorNodePool = new ObjectPool<ColorNode>();
        internal static ColorNode ColorNode(string colorHash) {
            ColorNode colorNode = s_ColorNodePool.Get();
            ColorUtility.TryParseHtmlString(colorHash, out Color color);
            colorNode.color = color;
            return colorNode;
        }
    }
    
    public class ColorNode : StyleASTNode {

        public Color color;

        public ColorNode() {
            type = StyleASTNodeType.Color;
        }
        
        public override void Release() {
            StyleASTNodeFactory.s_ColorNodePool.Release(this);
        }

        public override string ToString() {
            return $"{color}";
        }
    }
}