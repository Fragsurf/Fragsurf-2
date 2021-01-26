using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<UnitNode> s_UnitNodePool = new ObjectPool<UnitNode>();

        public static UnitNode UnitNode(string value) {
            UnitNode retn = s_UnitNodePool.Get();
            retn.value = value;
            return retn;
        }
    }

    public class UnitNode : StyleASTNode {
        public string value;

        public UnitNode() {
            type = StyleASTNodeType.Unit;
        }

        public override void Release() {
            StyleASTNodeFactory.s_UnitNodePool.Release(this);
        }

        public override string ToString() {
            return value;
        }
    }
}
