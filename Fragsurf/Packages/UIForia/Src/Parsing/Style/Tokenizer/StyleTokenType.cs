namespace UIForia.Parsing.Style.Tokenizer {

    public enum StyleTokenType {

        Invalid,

        Import,
        From,
        At,
        Use,
        Export,
        ExportAs,
        Const,
        EqualSign,
        Value,
        
        AttributeSpecifier,

        EndStatement,
        GreaterThan,
        LessThan,
        Colon,

        As,
        Dollar,

        // accessors
        Dot,
        Comma,
        // [
        BracesOpen,
        // ]
        BracesClose,
        BracketOpen,
        BracketClose,
        ParenOpen,
        ParenClose,

        // identifiers
        Identifier,
        VariableType,
        
        HashColor,
        Rgba,
        Rgb,
        Url,

        // constants
        String,
        Boolean,
        Number,

        // booleans
        And,
        Not,
        
        // top level identifier block names
        Style,
        Animation,
        SpriteSheet,
        Texture,
        Sound,
        Cursor,

        BooleanAnd,
        BooleanOr,
        BooleanNot,
        
        // operators
        Plus,
        Minus,
        Times,
        Divide,
        Mod,
        // Comparators
        Equals,
        NotEquals,

        GreaterThanEqualTo,
        LessThanEqualTo,
        QuestionMark,

        Exit,
        Enter,
        Run,
        Resume,
        Pause,
        Stop,

        EnterExit

    }
}
