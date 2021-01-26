using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<SoundCommandNode> s_SoundCommandNodePool = new ObjectPool<SoundCommandNode>();

        internal static SoundCommandNode SoundCommandNode(StyleASTNode name, RunCommandType cmdType, RunAction runAction) {
            SoundCommandNode soundCommandNode = s_SoundCommandNodePool.Get();
            soundCommandNode.name = name;
            soundCommandNode.cmdType = cmdType;
            soundCommandNode.runAction = runAction;
            return soundCommandNode;
        }
    }

    public class SoundCommandNode : CommandNode {

        public StyleASTNode name;
        public RunAction runAction;
        public RunCommandType cmdType;
        
        public override void Release() {
            name.Release();
            StyleASTNodeFactory.s_SoundCommandNodePool.Release(this);
        }
    }
}
