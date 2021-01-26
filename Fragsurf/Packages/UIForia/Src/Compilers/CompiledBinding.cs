using System.Linq.Expressions;

namespace UIForia.Compilers {

    public class CompiledBinding {

        public int line;
        public int column;
        public string filePath;
        public string elementTag;
        public int bindingId;
        public string guid;
        public LambdaExpression bindingFn;
        public CompiledBindingType bindingType;
        public string templateName;

    }

}