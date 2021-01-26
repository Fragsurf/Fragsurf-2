using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<ConstReferenceNode> s_ReferenceNodePool = new ObjectPool<ConstReferenceNode>();

        internal static ConstReferenceNode ConstReferenceNode(string value) {
            ConstReferenceNode constReferenceNode = s_ReferenceNodePool.Get();
            constReferenceNode.identifier = value;
            return constReferenceNode;
        }
    }

    public class ConstReferenceNode : StyleNodeContainer {

        //public string referenceName; // todo -- remove reference name because we use identifier from base class

        public ConstReferenceNode() {
            type = StyleASTNodeType.Reference;
        }

        public override void Release() {
            base.Release();
            StyleASTNodeFactory.s_ReferenceNodePool.Release(this);
        }

        protected bool Equals(ConstReferenceNode other) {
            return string.Equals(identifier, other.identifier);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConstReferenceNode) obj);
        }

        public override int GetHashCode() {
            return (identifier != null ? identifier.GetHashCode() : 0);
        }

        public override string ToString() {
            return $"@{identifier}";
        }
    }
}
