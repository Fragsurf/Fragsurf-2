using System;
using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {

        internal static readonly ObjectPool<StyleOperatorNode> s_OperatorPool = new ObjectPool<StyleOperatorNode>();

        internal static StyleOperatorNode OperatorNode(StyleOperatorType operatorType) {
            StyleOperatorNode operatorNode = s_OperatorPool.Get();
            operatorNode.type = StyleASTNodeType.Operator;
            operatorNode.operatorType = operatorType;
            return operatorNode;
        }

    }

    public class StyleOperatorNode : StyleASTNode {

        public StyleASTNode left;
        public StyleASTNode right;
        public StyleOperatorType operatorType;

        public int priority {
            get {
                switch (operatorType) {
                    case StyleOperatorType.Plus:
                        return 1;

                    case StyleOperatorType.Minus:
                        return 1;

                    case StyleOperatorType.Mod:
                        return 1;

                    case StyleOperatorType.Times:
                        return 2;

                    case StyleOperatorType.Divide:
                        return 2;

                    case StyleOperatorType.TernaryCondition:
                        return -2;

                    case StyleOperatorType.TernarySelection:
                        return -1;

                    case StyleOperatorType.Equals:
                        return -1;

                    case StyleOperatorType.NotEquals:
                        return -1;

                    case StyleOperatorType.GreaterThan:
                        return -1;

                    case StyleOperatorType.GreaterThanEqualTo:
                        return -1;

                    case StyleOperatorType.LessThan:
                        return -1;

                    case StyleOperatorType.LessThanEqualTo:
                        return -1;
                    case StyleOperatorType.And:
                        return -1;
                    case StyleOperatorType.Not:
                        return -1;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void Release() {
            left.Release();
            right.Release();
            StyleASTNodeFactory.s_OperatorPool.Release(this);
        }

    }

}
