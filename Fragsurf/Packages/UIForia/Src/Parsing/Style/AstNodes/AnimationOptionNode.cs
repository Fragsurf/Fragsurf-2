using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
            
        internal static readonly ObjectPool<AnimationOptionNode> s_AnimationOptionNodePool = new ObjectPool<AnimationOptionNode>();
        
        internal static AnimationOptionNode AnimationOptionNode(string optionName, StyleASTNode value) {
            AnimationOptionNode retn = s_AnimationOptionNodePool.Get();
            retn.optionName = optionName;
            retn.value = value;
            return retn;
        }
    }

    public class AnimationOptionNode : StyleASTNode {

        public string optionName;
        public StyleASTNode value;

        public override void Release() {
            value.Release();
            StyleASTNodeFactory.s_AnimationOptionNodePool.Release(this);
        }
    }
}