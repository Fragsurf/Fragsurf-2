using UIForia.Parsing.Style.AstNodes;

namespace UIForia.Compilers.Style {
    
    public struct StyleConstant {

        public string name;
        public bool exported;
        public StyleASTNode value;
        public ConstReferenceNode constReferenceNode;
        
    }
    
}
