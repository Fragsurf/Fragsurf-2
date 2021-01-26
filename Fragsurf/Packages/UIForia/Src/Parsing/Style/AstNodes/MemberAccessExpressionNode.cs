using System.Collections.Generic;
using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
        
        internal static readonly ObjectPool<MemberAccessExpressionNode> s_MemberAccessExpressionPool = new ObjectPool<MemberAccessExpressionNode>();

        internal static MemberAccessExpressionNode MemberAccessExpressionNode(string identifier, List<StyleASTNode> parts) {
            MemberAccessExpressionNode accessExpressionNode = s_MemberAccessExpressionPool.Get();
            accessExpressionNode.identifier = identifier;
            accessExpressionNode.parts = parts;
            return accessExpressionNode;
        }
    }
    
    public class MemberAccessExpressionNode : StyleASTNode {

        public string identifier;
        public List<StyleASTNode> parts;

        public MemberAccessExpressionNode() {
            type = StyleASTNodeType.AccessExpression;
        }

        public override void Release() {
            StyleASTNodeFactory.s_MemberAccessExpressionPool.Release(this);
            for (int i = 0; i < parts.Count; i++) {
                parts[i].Release();
            }

            ListPool<StyleASTNode>.Release(ref parts);
        }
    }
}