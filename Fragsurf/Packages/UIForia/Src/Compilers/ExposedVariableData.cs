using System;
using UIForia.Parsing.Expressions;

namespace UIForia.Compilers {

    public class ExposedVariableData {

        public Type rootType;
        public ContextVariableDefinition[] scopedVariables;
        public AttributeDefinition[] exposedAttrs;

    }

}