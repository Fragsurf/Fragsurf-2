using System;

namespace UIForia.Parsing.Style.AstNodes {

    public class VariableDefinitionNode : StyleASTNode {

        public Type variableType;

        public string name;

        public StyleASTNode value;

        public override void Release() {
            value.Release();
        }

    }

}