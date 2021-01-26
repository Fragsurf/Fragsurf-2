namespace UIForia.Parsing.Expressions {

    public enum ASTNodeType {

        NullLiteral,
        BooleanLiteral,
        NumericLiteral,
        DefaultLiteral,
        StringLiteral,
        Operator,
        TypeOf,
        Identifier,
        DotAccess,
        AccessExpression,
        IndexExpression,
        UnaryNot,
        UnaryMinus,
        UnaryBitwiseNot,
        DirectCast,
        ListInitializer,
        New,
        Paren,
        GenericTypePath,
        LambdaExpression,
        UnaryPreIncrement,
        UnaryPreDecrement,
        UnaryPostIncrement,
        UnaryPostDecrement,
        Block,
        Return,
        VariableDeclaration,
        IfStatement,
        Method,
        Field,

        ElseIf

    }

}