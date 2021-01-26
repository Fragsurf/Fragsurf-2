using System.Diagnostics;
using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
        
        internal static readonly ObjectPool<StyleIdentifierNode> s_IdentifierPool = new ObjectPool<StyleIdentifierNode>();
        
        internal static StyleIdentifierNode IdentifierNode(string name) {
            StyleIdentifierNode idNode = s_IdentifierPool.Get();
            idNode.name = name;
            idNode.type = StyleASTNodeType.Identifier;
            return idNode;
        }
    }
    
    [DebuggerDisplay("StyleIdentifierNode[{name}]")]
    public class StyleIdentifierNode : StyleASTNode {

        public string name;

        public override void Release() {
            StyleASTNodeFactory.s_IdentifierPool.Release(this);
        }

        protected bool Equals(StyleIdentifierNode other) {
            return string.Equals(name, other.name);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StyleIdentifierNode) obj);
        }

        public override int GetHashCode() {
            return (name != null ? name.GetHashCode() : 0);
        }

        public override string ToString() {
            return name;
        }
    }

}