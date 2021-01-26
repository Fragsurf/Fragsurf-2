using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {
    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<SoundPropertyNode> s_SoundPropertyNodePool = new ObjectPool<SoundPropertyNode>();

        internal static SoundPropertyNode SoundPropertyNode(string name, StyleASTNode value) {
            SoundPropertyNode retn = s_SoundPropertyNodePool.Get();
            retn.name = name;
            retn.value = value;
            retn.type = StyleASTNodeType.Property;
            return retn;
        }
    }

    public class SoundPropertyNode : StyleASTNode {

        public string name;
        public StyleASTNode value;

        public override void Release() {
            value.Release();
            StyleASTNodeFactory.s_SoundPropertyNodePool.Release(this);
        }
    }
}
