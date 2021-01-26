namespace UIForia.Parsing.Style.AstNodes {

    public class VariableReferenceNode : StyleNodeContainer {

        public VariableReferenceNode(string identifier) {
            this.identifier = identifier;
        }
        
        public override void Release() {
            throw new System.NotImplementedException();
        }

    }

}