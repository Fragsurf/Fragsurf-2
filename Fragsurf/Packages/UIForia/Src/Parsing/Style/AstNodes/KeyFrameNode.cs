using System.Collections.Generic;
using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<KeyFrameNode> s_KeyFrameNodePool = new ObjectPool<KeyFrameNode>();

        internal static KeyFrameNode KeyFrameNode(int keyframe) {
            KeyFrameNode node = s_KeyFrameNodePool.Get();
            node.keyframes = ListPool<int>.Get();
            node.keyframes.Add(keyframe);
            return node;
        }
    }

    public class KeyFrameNode : StyleNodeContainer {
        public List<int> keyframes;

        public override void Release() {
            ListPool<int>.Release(ref keyframes);
            StyleASTNodeFactory.s_KeyFrameNodePool.Release(this);
        }

    }

}