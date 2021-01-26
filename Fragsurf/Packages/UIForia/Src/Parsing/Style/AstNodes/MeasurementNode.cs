using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<MeasurementNode> s_MeasurementNodePool = new ObjectPool<MeasurementNode>();

        internal static MeasurementNode MeasurementNode(StyleLiteralNode value, UnitNode unit) {
            MeasurementNode measurementNode = s_MeasurementNodePool.Get();
            measurementNode.value = value;
            measurementNode.unit = unit;
            return measurementNode;
        }
    }

    public class MeasurementNode : StyleASTNode {

        public StyleLiteralNode value;

        public UnitNode unit;

        public MeasurementNode() {
            type = StyleASTNodeType.Measurement;
        }

        public override void Release() {
            StyleASTNodeFactory.s_MeasurementNodePool.Release(this);
        }

        public override string ToString() {
            return $"{value}{unit}";
        }
    }
}
