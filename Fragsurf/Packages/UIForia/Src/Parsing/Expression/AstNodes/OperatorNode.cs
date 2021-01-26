using System.Diagnostics;
using UIForia.Exceptions;

namespace UIForia.Parsing.Expressions.AstNodes {

    public class OperatorNodeDebugView {

        public OperatorType operatorType;
        public ASTNode left;
        public ASTNode right;
        
        public OperatorNodeDebugView(OperatorNode node) {
            operatorType = node.operatorType;
            left = node.left;
            right = node.right;
        }

    }
    
    [DebuggerTypeProxy(typeof(OperatorNodeDebugView))]
    public class OperatorNode : ASTNode {

        public ASTNode left;
        public ASTNode right;
        public OperatorType operatorType;

        public int priority {
            get {
                switch (operatorType) {
                    case OperatorType.Plus:
                        return 1;
                    
                    case OperatorType.Minus:
                        return 1;
                    
                    case OperatorType.Mod:
                        return 2;
                    
                    case OperatorType.Times:
                        return 2;
                    
                    case OperatorType.Divide:
                        return 2;
                    
                    case OperatorType.TernaryCondition:
                        return -2;
                    
                    case OperatorType.TernarySelection:
                        return -1;
                    
                    case OperatorType.Equals:
                        return -1;
                    
                    case OperatorType.NotEquals:
                        return -1;
                    
                    case OperatorType.GreaterThan:
                        return -1;
                    
                    case OperatorType.GreaterThanEqualTo:
                        return -1;
                    
                    case OperatorType.LessThan:
                        return -1;
                    
                    case OperatorType.LessThanEqualTo:
                        return -1;
                    
                    case OperatorType.ShiftLeft:
                        return 1;
                    
                    case OperatorType.ShiftRight:
                        return 1;
                    
                    case OperatorType.BinaryOr:
                        return 1;
                    
                    case OperatorType.BinaryAnd:
                        return 1;
                    
                    case OperatorType.BinaryXor:
                        return 1;
                    
                    case OperatorType.Coalesce:
                        return 1; // todo -- not sure about this
                    
                    case OperatorType.Or:
                        return 2; // todo -- not sure about this
                    
                    case OperatorType.And:
                        return 2; // todo -- not sure about this 
                    
                    case OperatorType.Assign:
                        return 2; // todo -- not sure about this 
                    
                    default:
                        throw new TemplateParseException($"Operator {operatorType} could not be parsed because it is not supported right now.", left);
                }
            }
        }

        public override void Release() {
            left.Release();
            right.Release();
            s_OperatorPool.Release(this);
        }

    }

}