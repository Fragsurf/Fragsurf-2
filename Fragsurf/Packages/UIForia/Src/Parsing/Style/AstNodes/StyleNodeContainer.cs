using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    public abstract class StyleNodeContainer : StyleASTNode {

        public string identifier;
        public LightList<StyleASTNode> children;

        public StyleNodeContainer() {
            this.children = new LightList<StyleASTNode>(2);
        }

        public void AddChildNode(StyleASTNode child) {
            children.Add(child);
        }

        public override void Release() {
            for (int index = 0; index < children.Count; index++) {
                children[index].Release();
            }

            children.Clear();
        }
        
        public override string ToString() {

            string childrenToString = children.Count > 0 ? children[0].ToString() : string.Empty;
            for (int index = 1; index < children.Count; index++) {
                var child = children[index];
                childrenToString += ", " + child;
            }

            return $"{identifier} = {childrenToString}";
        }

    }

}