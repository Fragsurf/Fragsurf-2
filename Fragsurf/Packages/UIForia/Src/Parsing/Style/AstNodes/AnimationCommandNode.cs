using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<AnimationCommandNode> s_AnimationCommandNodePool = new ObjectPool<AnimationCommandNode>();

        internal static AnimationCommandNode AnimationCommandNode(StyleASTNode animationName, RunCommandType cmdType, RunAction runAction) {
            AnimationCommandNode animationCommandNode = s_AnimationCommandNodePool.Get();
            animationCommandNode.animationName = animationName;
            animationCommandNode.cmdType = cmdType;
            animationCommandNode.runAction = runAction;
            return animationCommandNode;
        }
    }

    public class AnimationCommandNode : CommandNode {

        public StyleASTNode animationName;
        public RunCommandType cmdType;
        public RunAction runAction;

        public override void Release() {
            animationName.Release();
            StyleASTNodeFactory.s_AnimationCommandNodePool.Release(this);
        }
    }
}
