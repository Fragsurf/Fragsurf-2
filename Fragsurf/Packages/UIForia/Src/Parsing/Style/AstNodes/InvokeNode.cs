using System.Collections.Generic;
using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<InvokeNode> s_InvokeNodePool = new ObjectPool<InvokeNode>();

        internal static InvokeNode InvokeNode(List<StyleASTNode> parameters) {
            InvokeNode invokeNode = s_InvokeNodePool.Get();
            invokeNode.parameters = parameters;
            return invokeNode;
        }
    }

    public class InvokeNode : StyleASTNode {
        public List<StyleASTNode> parameters;

        public override void Release() {
            for (int i = 0; i < parameters.Count; i++) {
                parameters[i].Release();
            }

            ListPool<StyleASTNode>.Release(ref parameters);
            StyleASTNodeFactory.s_InvokeNodePool.Release(this);
        }

    }
}
