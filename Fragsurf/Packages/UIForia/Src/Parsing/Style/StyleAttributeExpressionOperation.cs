namespace UIForia.Parsing.Style {
    public interface StyleAttributeExpressionOperation {
        int Execute(int left, int right);
    }

    public class AddOperation : StyleAttributeExpressionOperation {
        public int Execute(int left, int right) {
            return left + right;
        }
    }

    public class SubtractionOperation : StyleAttributeExpressionOperation {
        public int Execute(int left, int right) {
            return left - right;
        }
    }

    public class MultiplicationOperation : StyleAttributeExpressionOperation {
        public int Execute(int left, int right) {
            return left * right;
        }
    }

    public class DivisionOperation : StyleAttributeExpressionOperation {
        public int Execute(int left, int right) {
            return left / right;
        }
    }

    public class ModOperation : StyleAttributeExpressionOperation {
        public int Execute(int left, int right) {
            return left % right;
        }
    }

    public class GreaterThanOperation : StyleAttributeExpressionOperation {
        public int Execute(int left, int right) {
            return left > right ? 0 : -1;
        }
    }

    public class GreaterThanEqualOperation : StyleAttributeExpressionOperation {
        public int Execute(int left, int right) {
            return left >= right ? 0 : -1;
        }
    }

    public class LessThanOperation : StyleAttributeExpressionOperation {
        public int Execute(int left, int right) {
            return left < right ? 0 : -1;
        }
    }

    public class LessThanEqualOperation : StyleAttributeExpressionOperation {
        public int Execute(int left, int right) {
            return left <= right ? 0 : -1;
        }
    }
}
