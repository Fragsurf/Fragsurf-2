using System;

namespace UIForia.Parsing.Expressions.AstNodes {
    
    [Flags]
    public enum OperatorType {
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
        And = 1 << 13,
        Or = 1 << 14,
        Not = 1 << 15,
        As = 1 << 16,
        Is = 1 << 17,
        BinaryNot = 1 << 18,
        BinaryOr = 1 << 19,
        BinaryXor = 1 << 20,
        BinaryAnd = 1 << 21,
        ShiftLeft = 1 << 22,
        ShiftRight = 1 << 23,
        Coalesce = 1 << 24,
        Elvis = 1 << 25,
        Assign = 1 << 26,

    }
}