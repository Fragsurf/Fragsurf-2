using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<RunNode> s_RunNodePool = new ObjectPool<RunNode>();

        internal static RunNode RunNode(StyleASTNode command) {
            RunNode result = s_RunNodePool.Get();
            result.command = command;
            return result;
        }
    }

    public class RunNode : StyleASTNode {

        public StyleASTNode command;

        public override void Release() {
            command.Release();
            StyleASTNodeFactory.s_RunNodePool.Release(this);
        }

    }

}