using System.Diagnostics;

namespace UIForia.Parsing.Expressions.Tokenizer {

    [DebuggerDisplay("{value} --- {expressionTokenType}")]
    public readonly struct ExpressionToken {

        public readonly ExpressionTokenType expressionTokenType;
        public readonly string value;
        public readonly int line;
        public readonly int column;


        public ExpressionToken(ExpressionTokenType expressionTokenType, int line, int column) :
            this(expressionTokenType, string.Empty, line, column) { }

        public ExpressionToken(ExpressionTokenType expressionTokenType, string value, int line, int column) {
            this.expressionTokenType = expressionTokenType;
            this.value = value;
            this.line = line;
            this.column = column;
        }

        [DebuggerStepThrough]
        public static implicit operator ExpressionTokenType(ExpressionToken token) {
            return token.expressionTokenType;
        }

        [DebuggerStepThrough]
        public static implicit operator string(ExpressionToken token) {
            return token.value;
        }

        public bool IsArithmeticOperator =>
            expressionTokenType == ExpressionTokenType.Plus ||
            expressionTokenType == ExpressionTokenType.Minus ||
            expressionTokenType == ExpressionTokenType.Times ||
            expressionTokenType == ExpressionTokenType.Divide ||
            expressionTokenType == ExpressionTokenType.Mod ||
            expressionTokenType == ExpressionTokenType.BinaryAnd ||
            expressionTokenType == ExpressionTokenType.BinaryOr ||
            expressionTokenType == ExpressionTokenType.BinaryXor;

        public bool IsComparator =>
            expressionTokenType == ExpressionTokenType.Equals ||
            expressionTokenType == ExpressionTokenType.NotEquals ||
            expressionTokenType == ExpressionTokenType.GreaterThan ||
            expressionTokenType == ExpressionTokenType.GreaterThanEqualTo ||
            expressionTokenType == ExpressionTokenType.LessThan ||
            expressionTokenType == ExpressionTokenType.LessThanEqualTo;

        public bool IsBooleanTest =>
            expressionTokenType == ExpressionTokenType.AndAlso ||
            expressionTokenType == ExpressionTokenType.OrElse ||
            expressionTokenType == ExpressionTokenType.Not;

        public bool IsOperator =>
            IsArithmeticOperator ||
            IsComparator ||
            IsBooleanTest ||
            expressionTokenType == ExpressionTokenType.AddAssign ||
            expressionTokenType == ExpressionTokenType.SubtractAssign ||
            expressionTokenType == ExpressionTokenType.ModAssign ||
            expressionTokenType == ExpressionTokenType.MultiplyAssign ||
            expressionTokenType == ExpressionTokenType.DivideAssign ||
            expressionTokenType == ExpressionTokenType.LeftShiftAssign ||
            expressionTokenType == ExpressionTokenType.RightShiftAssign ||
            expressionTokenType == ExpressionTokenType.XorAssign ||
            expressionTokenType == ExpressionTokenType.AndAssign ||
            expressionTokenType == ExpressionTokenType.OrAssign ||
            
            expressionTokenType == ExpressionTokenType.Assign || 
            expressionTokenType == ExpressionTokenType.Coalesce ||
            expressionTokenType == ExpressionTokenType.Elvis ||
            expressionTokenType == ExpressionTokenType.As ||
            expressionTokenType == ExpressionTokenType.Is ||
            expressionTokenType == ExpressionTokenType.QuestionMark ||
            expressionTokenType == ExpressionTokenType.Colon;

        public bool IsUnaryOperator =>
            expressionTokenType == ExpressionTokenType.Plus ||
            expressionTokenType == ExpressionTokenType.Minus ||
            expressionTokenType == ExpressionTokenType.BinaryNot ||
            expressionTokenType == ExpressionTokenType.Increment ||
            expressionTokenType == ExpressionTokenType.Decrement ||
            expressionTokenType == ExpressionTokenType.Not;

        public bool UnaryRequiresCheck =>
            expressionTokenType == ExpressionTokenType.Comma ||
            expressionTokenType == ExpressionTokenType.Colon ||
            expressionTokenType == ExpressionTokenType.QuestionMark ||
            expressionTokenType == ExpressionTokenType.ParenOpen ||
            expressionTokenType == ExpressionTokenType.ArrayAccessOpen ||
            IsArithmeticOperator ||
            IsComparator;

        public static ExpressionToken Invalid => new ExpressionToken(ExpressionTokenType.Invalid, string.Empty, -1, -1);

    }

}