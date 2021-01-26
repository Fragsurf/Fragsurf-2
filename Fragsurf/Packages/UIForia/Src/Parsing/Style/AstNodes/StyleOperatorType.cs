using System;

namespace UIForia.Parsing.Style.AstNodes {
    
    [Flags]
    public enum StyleOperatorType {
        Plus = 1 << 0,
        Minus = 1 << 1,
        Mod = 1 << 2,
        Times = 1 << 3,
        Divide = 1 << 4,
        TernaryCondition = 1 << 5,
        TernarySelection = 1 << 6,
        Equals = 1 << 7,
        NotEquals = 1 << 8,
        GreaterThan = 1 << 9,
        GreaterThanEqualTo = 1 << 10,
        LessThan = 1 << 11,
        LessThanEqualTo = 1 << 12,
        BooleanAnd = 1 << 13,
        BooleanOr = 1 << 14,
        BooleanNot = 1 << 15,
        As = 1 << 16,
        Is = 1 << 17,
        And = 1 << 18,
        Not = 1 << 19,
        
        GroupOperator = And | Not,
        Boolean = BooleanAnd | BooleanOr | BooleanNot,
        Arithmetic = Plus | Minus | Times | Divide | Mod,
        Comparator = Equals | NotEquals | GreaterThan | GreaterThanEqualTo | LessThan | LessThanEqualTo,

    }
}