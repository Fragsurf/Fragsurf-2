namespace UIForia.Parsing.Expressions.Tokenizer {

    public enum ExpressionTokenType {
        
        Invalid,
        
        // operators
        Plus,
        Minus,
        Times,
        Divide,
        Mod,
        BinaryOr,
        BinaryAnd,
        BinaryXor,
        BinaryNot,
        
        Is,
        As,
        Dollar, 
        New, 
        Null,
        Default,
        TypeOf,
        
        // accessors
        Dot,
        Comma,
        ExpressionOpen,
        ExpressionClose,
        ArrayAccessOpen,
        ArrayAccessClose,
        ParenOpen,
        ParenClose,

        // identifiers
        Identifier,
//        Alias,
        At,

        // constants
        String, 
        Boolean,
        Number,

        // booleans
        AndAlso,
        OrElse,
        Not,

        // Comparators
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanEqualTo,
        LessThanEqualTo,
        QuestionMark,
        Colon, 
     
        Assign,
        AddAssign,
        SubtractAssign,
        MultiplyAssign,
        DivideAssign,
        ModAssign,
        AndAssign,
        OrAssign,
        XorAssign,
        LeftShiftAssign,
        RightShiftAssign,
        
        Increment,
        Decrement,
        
        LambdaArrow,

        Coalesce,

        Elvis,

        If,
        Else,
        ElseIf,
        SemiColon,

        Var,

        While,

        For,

        Return

    }

}