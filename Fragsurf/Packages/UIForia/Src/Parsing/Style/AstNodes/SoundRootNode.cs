using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {
    
    internal static partial class StyleASTNodeFactory {
            
        internal static readonly ObjectPool<SoundRootNode> s_SoundRootNodePool = new ObjectPool<SoundRootNode>();
        
        internal static SoundRootNode SoundRootNode(string name) {
            SoundRootNode retn = s_SoundRootNodePool.Get();
            retn.identifier = name;
            retn.type = StyleASTNodeType.SoundDeclaration;
            return retn;
        }
    }

    public class SoundRootNode : StyleNodeContainer {

        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_SoundRootNodePool.Release(this);
        }
    }
}
