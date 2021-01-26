namespace UIForia.Parsing.Style.AstNodes {

    public enum StyleASTNodeType {

        None,
        NullLiteral,
        BooleanLiteral,
        NumericLiteral,
        DefaultLiteral,
        StringLiteral,
        Operator,
        TypeOf,
        Identifier,
        Invalid,
        DotAccess,
        AccessExpression,
        IndexExpression,
        UnaryNot,
        UnaryMinus,
        Paren,
        Unit,
        Rgba,
        Rgb,
        Function,
        Url,
        Property,
        AttributeGroup,
        StateGroup,
        ExpressionGroup,
        Color,
        Measurement,
        Reference,
        
        // the integers define the sort order. see the style compiler for more details
        Import = 100,
        Const = 110,
        Export = 120,
        AnimationDeclaration = 130,
        SpriteSheetDeclaration = 131,
        SoundDeclaration = 135,
        StyleGroup = 140,
        MaterialProperty

    }

}
