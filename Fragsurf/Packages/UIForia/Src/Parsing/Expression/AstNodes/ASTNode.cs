using UIForia.Parsing.Expressions.Tokenizer;
using UIForia.Util;

namespace UIForia.Parsing.Expressions.AstNodes {

    public abstract class ASTNode {

        protected static readonly ObjectPool<LiteralNode> s_LiteralPool = new ObjectPool<LiteralNode>();
        protected static readonly ObjectPool<OperatorNode> s_OperatorPool = new ObjectPool<OperatorNode>();
        protected static readonly ObjectPool<IdentifierNode> s_IdentifierPool = new ObjectPool<IdentifierNode>();
        protected static readonly ObjectPool<TypeNode> s_TypeNodePool = new ObjectPool<TypeNode>();
        protected static readonly ObjectPool<ParenNode> s_ParenPool = new ObjectPool<ParenNode>();
        protected static readonly ObjectPool<DotAccessNode> s_DotAccessPool = new ObjectPool<DotAccessNode>();
        protected static readonly ObjectPool<MemberAccessExpressionNode> s_MemberAccessExpressionPool = new ObjectPool<MemberAccessExpressionNode>();
        protected static readonly ObjectPool<IndexNode> s_IndexExpressionPool = new ObjectPool<IndexNode>();
        protected static readonly ObjectPool<InvokeNode> s_InvokeNodePool = new ObjectPool<InvokeNode>();
        protected static readonly ObjectPool<NewExpressionNode> s_NewExpressionNodePool = new ObjectPool<NewExpressionNode>();
        protected static readonly ObjectPool<UnaryExpressionNode> s_UnaryNodePool = new ObjectPool<UnaryExpressionNode>();
        protected static readonly ObjectPool<ListInitializerNode> s_ListInitializerPool = new ObjectPool<ListInitializerNode>();
        protected static readonly ObjectPool<GenericTypePathNode> s_GenericTypePathNode = new ObjectPool<GenericTypePathNode>();
        protected static readonly ObjectPool<LambdaExpressionNode> s_LambdaExpressionPool = new ObjectPool<LambdaExpressionNode>();

        public static readonly ASTNode[] EmptyArray = { };

        public ASTNodeType type;

        public bool IsCompound {
            get {
                if (type == ASTNodeType.Operator) {
                    return true;
                }

                return false;
            }
        }


        public int line;
        public int column;

        public ASTNode WithLocation(ExpressionToken token) {
            this.line = token.line;
            this.column = token.column;
            return this;
        }


        public abstract void Release();

        public static LiteralNode NullLiteralNode(string value) {
            LiteralNode retn = s_LiteralPool.Get();
            retn.type = ASTNodeType.NullLiteral;
            retn.rawValue = value;
            return retn;
        }

        public static LiteralNode StringLiteralNode(string value) {
            LiteralNode retn = s_LiteralPool.Get();
            retn.type = ASTNodeType.StringLiteral;
            retn.rawValue = value;
            return retn;
        }

        public static LiteralNode BooleanLiteralNode(string value) {
            LiteralNode retn = s_LiteralPool.Get();
            retn.type = ASTNodeType.BooleanLiteral;
            retn.rawValue = value;
            return retn;
        }

        public static LiteralNode NumericLiteralNode(string value) {
            LiteralNode retn = s_LiteralPool.Get();
            retn.type = ASTNodeType.NumericLiteral;
            retn.rawValue = value;
            return retn;
        }

        public static LiteralNode DefaultLiteralNode(string value) {
            LiteralNode retn = s_LiteralPool.Get();
            retn.type = ASTNodeType.DefaultLiteral;
            retn.rawValue = value;
            return retn;
        }

        public static OperatorNode OperatorNode(OperatorType operatorType) {
            OperatorNode operatorNode = s_OperatorPool.Get();
            operatorNode.type = ASTNodeType.Operator;
            operatorNode.operatorType = operatorType;
            return operatorNode;
        }

        public static IdentifierNode IdentifierNode(string name) {
            IdentifierNode idNode = s_IdentifierPool.Get();
            idNode.name = name;
            idNode.type = ASTNodeType.Identifier;
            return idNode;
        }

        public static TypeNode TypeOfNode(TypeLookup typeLookup) {
            TypeNode typeOfNode = s_TypeNodePool.Get();
            typeOfNode.typeLookup = typeLookup;
            typeOfNode.type = ASTNodeType.TypeOf;
            return typeOfNode;
        }

        public static NewExpressionNode NewExpressionNode(TypeLookup typeLookup, LightList<ASTNode> parameters) {
            NewExpressionNode retn = s_NewExpressionNodePool.Get();
            retn.typeLookup = typeLookup;
            retn.parameters = parameters;
            retn.type = ASTNodeType.New;
            return retn;
        }

        public static ParenNode ParenNode(ASTNode expression) {
            ParenNode parenNode = s_ParenPool.Get();
            parenNode.expression = expression;
            return parenNode;
        }

        public static DotAccessNode DotAccessNode(string propertyName, bool isElvisAccess = false) {
            DotAccessNode dotAccessNode = s_DotAccessPool.Get();
            dotAccessNode.propertyName = propertyName;
            dotAccessNode.isNullableAccess = isElvisAccess;
            return dotAccessNode;
        }

        public static InvokeNode InvokeNode(LightList<ASTNode> parameters) {
            InvokeNode invokeNode = s_InvokeNodePool.Get();
            invokeNode.parameters = parameters;
            return invokeNode;
        }

        public static MemberAccessExpressionNode MemberAccessExpressionNode(string identifier, LightList<ASTNode> parts) {
            MemberAccessExpressionNode accessExpressionNode = s_MemberAccessExpressionPool.Get();
            accessExpressionNode.identifier = identifier;
            accessExpressionNode.parts = parts;
            return accessExpressionNode;
        }

        public static ListInitializerNode ListInitializerNode(LightList<ASTNode> list) {
            ListInitializerNode listInitializerNode = s_ListInitializerPool.Get();
            listInitializerNode.list = list;
            return listInitializerNode;
        }

        // todo -- support multiple indexer arguments
        public static IndexNode IndexExpressionNode(ASTNode expression, bool isElvisAccess) {
            IndexNode indexNode = s_IndexExpressionPool.Get();
            LightList<ASTNode> list = LightList<ASTNode>.GetMinSize(4);
            list.Add(expression);
            indexNode.arguments = list;
            indexNode.isNullableAccess = isElvisAccess;
            return indexNode;
        }

        public static UnaryExpressionNode UnaryExpressionNode(ASTNodeType nodeType, ASTNode expr) {
            UnaryExpressionNode unaryNode = s_UnaryNodePool.Get();
            unaryNode.type = nodeType;
            unaryNode.expression = expr;
            return unaryNode;
        }

        public static UnaryExpressionNode DirectCastNode(TypeLookup typeLookup, ASTNode expression) {
            UnaryExpressionNode unaryNode = s_UnaryNodePool.Get();
            unaryNode.type = ASTNodeType.DirectCast;
            unaryNode.typeLookup = typeLookup;
            unaryNode.expression = expression;
            return unaryNode;
        }

        public static ASTNode GenericTypePath(TypeLookup genericPath) {
            GenericTypePathNode node = s_GenericTypePathNode.Get();
            node.genericPath = genericPath;
            return node;
        }

        public static LambdaExpressionNode LambdaExpressionNode(StructList<LambdaArgument> arguments, ASTNode body) {
            LambdaExpressionNode node = s_LambdaExpressionPool.Get();
            node.signature = arguments;
            node.body = body;
            return node;
        }
        
    }

    public class GenericTypePathNode : ASTNode {

        public TypeLookup genericPath;

        public GenericTypePathNode() {
            this.type = ASTNodeType.GenericTypePath;
        }

        public override void Release() {
            genericPath.Release();
            s_GenericTypePathNode.Release(this);
        }

    }

    public class UnaryExpressionNode : ASTNode {

        public ASTNode expression;
        public TypeLookup typeLookup;

        public override void Release() {
            typeLookup.Release();
            expression?.Release();
            s_UnaryNodePool.Release(this);
        }

    }

    public class MemberAccessExpressionNode : ASTNode {

        public string identifier;
        public LightList<ASTNode> parts;

        public MemberAccessExpressionNode() {
            type = ASTNodeType.AccessExpression;
        }

        public override void Release() {
            s_MemberAccessExpressionPool.Release(this);
            for (int i = 0; i < parts.Count; i++) {
                parts[i].Release();
            }

            LightList<ASTNode>.Release(ref parts);
        }

        public override string ToString() {
            return $"{identifier} with parts: {string.Join(".", parts)}";
        }

    }

    public class ParenNode : ASTNode {

        public ASTNode expression;
        public MemberAccessExpressionNode accessExpression;

        public ParenNode() {
            type = ASTNodeType.Paren;
        }

        public override void Release() {
            expression?.Release();
            accessExpression?.Release();
            s_ParenPool.Release(this);
        }

    }

    public class TypeNode : ASTNode {

        public TypeLookup typeLookup;

        public override void Release() {
            s_TypeNodePool.Release(this);
            typeLookup.Release();
        }

    }

    public class InvokeNode : ASTNode {

        public LightList<ASTNode> parameters;

        public override void Release() {
            for (int i = 0; i < parameters.Count; i++) {
                parameters[i].Release();
            }

            LightList<ASTNode>.Release(ref parameters);
            s_InvokeNodePool.Release(this);
        }

    }

    public class NewExpressionNode : ASTNode {

        public TypeLookup typeLookup;
        public LightList<ASTNode> parameters;

        public NewExpressionNode() {
            type = ASTNodeType.New;
        }

        public override void Release() {
            if (parameters != null) {
                for (int i = 0; i < parameters.Count; i++) {
                    parameters[i].Release();
                }
            }

            typeLookup.Release();
            LightList<ASTNode>.Release(ref parameters);
            s_NewExpressionNodePool.Release(this);
        }

    }

    public class ListInitializerNode : ASTNode {

        public LightList<ASTNode> list;

        public ListInitializerNode() {
            type = ASTNodeType.ListInitializer;
        }

        public override void Release() {
            for (int i = 0; i < list.Count; i++) {
                list[i].Release();
            }

            LightList<ASTNode>.Release(ref list);
            s_ListInitializerPool.Release(this);
        }

    }

    public class IndexNode : ASTNode {

        public LightList<ASTNode> arguments;
        public bool isNullableAccess;

        public IndexNode() {
            type = ASTNodeType.IndexExpression;
        }

        public override void Release() {
            for (int i = 0; i < arguments.Count; i++) {
                arguments[i].Release();
            }
            LightList<ASTNode>.Release(ref arguments);
            s_IndexExpressionPool.Release(this);
        }

        public override string ToString() {
            return arguments.ToString();
        }

    }

    public class DotAccessNode : ASTNode {

        public string propertyName;
        public bool isNullableAccess;

        public DotAccessNode() {
            type = ASTNodeType.DotAccess;
        }

        public override void Release() {
            s_DotAccessPool.Release(this);
        }

        public override string ToString() {
            return propertyName;
        }

    }

    public struct LambdaArgument {

        public TypeLookup? type;
        public string identifier;

    }
    
    public class LambdaExpressionNode : ASTNode {

        public StructList<LambdaArgument> signature;
        public ASTNode body;

        public LambdaExpressionNode() {
            type = ASTNodeType.LambdaExpression;
        }

        public override void Release() {
            body.Release();
            StructList<LambdaArgument>.Release(ref signature);
            body = null;
            s_LambdaExpressionPool.Release(this);
        }


    }

}