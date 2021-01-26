using System.Diagnostics;

namespace UIForia.Parsing.Style.Tokenizer {

    [DebuggerDisplay("{styleTokenType} -> {value}")]
    public struct StyleToken {

        public readonly StyleTokenType styleTokenType;
        public readonly string value;
        public readonly int line;
        public readonly int column;

        public StyleToken(StyleTokenType styleTokenType, string value, int line, int column) {
            this.styleTokenType = styleTokenType;
            this.value = value;
            this.line = line;
            this.column = column;
        }

        public StyleToken(StyleTokenType styleTokenType, int line, int column) : 
            this(styleTokenType, string.Empty, line, column) {
        }

        [DebuggerStepThrough]
        public static implicit operator StyleTokenType(StyleToken token) {
            return token.styleTokenType;
        }
        
        [DebuggerStepThrough]
        public static implicit operator string(StyleToken token) {
            return token.value;
        }
        
        public bool IsArithmeticOperator =>
            styleTokenType == StyleTokenType.Plus ||
            styleTokenType == StyleTokenType.Minus ||
            styleTokenType == StyleTokenType.Times ||
            styleTokenType == StyleTokenType.Divide ||
            styleTokenType == StyleTokenType.Mod;
        
        public bool IsComparator => 
            styleTokenType == StyleTokenType.Equals ||
            styleTokenType == StyleTokenType.NotEquals ||
            styleTokenType == StyleTokenType.GreaterThan || 
            styleTokenType == StyleTokenType.GreaterThanEqualTo || 
            styleTokenType == StyleTokenType.LessThan || 
            styleTokenType == StyleTokenType.LessThanEqualTo;

        public bool IsBooleanTest =>
            styleTokenType == StyleTokenType.BooleanAnd ||
            styleTokenType == StyleTokenType.BooleanOr ||
            styleTokenType == StyleTokenType.BooleanNot;
        
        public bool IsOperator =>
            IsArithmeticOperator ||
            IsComparator ||
            IsBooleanTest || 
            styleTokenType == StyleTokenType.As ||
            styleTokenType == StyleTokenType.QuestionMark ||
            styleTokenType == StyleTokenType.Colon;

        public bool IsUnaryOperator =>
            styleTokenType == StyleTokenType.Plus ||
            styleTokenType == StyleTokenType.Minus ||
            styleTokenType == StyleTokenType.Not;

        public bool UnaryRequiresCheck =>
            styleTokenType == StyleTokenType.Comma ||
            styleTokenType == StyleTokenType.Colon ||
            styleTokenType == StyleTokenType.QuestionMark ||
            styleTokenType == StyleTokenType.ParenOpen ||
            IsArithmeticOperator ||
            IsComparator;
        public static StyleToken Invalid => new StyleToken(StyleTokenType.Invalid, string.Empty, 0, 0);

        public override string ToString() {
            return $"{styleTokenType} -> {value}";
        }
    }

}