using System.Linq.Expressions;

namespace UIForia.Compilers {

    public struct LHSAssignment {

        public Expression left;
        public Expression right;
        public Expression index;

    }

}