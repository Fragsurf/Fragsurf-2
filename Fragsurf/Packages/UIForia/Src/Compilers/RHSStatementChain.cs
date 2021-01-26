using System.Linq.Expressions;

namespace UIForia.Compilers {

    public struct RHSStatementChain {

        public Expression OutputExpression;

        public static implicit operator Expression(RHSStatementChain chain) {
            return chain.OutputExpression;
        }

    }

}