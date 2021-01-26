using System.Linq.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class LHSStatementChain {

        public string outputVarName;
        public bool isSimpleAssignment;
        public Expression targetExpression;
        public StructList<LHSAssignment> assignments;
        public Expression OutputExpression => null;

        public void AddAssignment(Expression left, Expression right) {
            assignments = assignments ?? new StructList<LHSAssignment>();
            assignments.Add(new LHSAssignment() {
                left = left,
                right = right
            });
        }

        public void AddIndexAssignment(Expression left, Expression right, IndexExpression index) { }

    }

}