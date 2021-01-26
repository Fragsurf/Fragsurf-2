namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
    }

    public abstract class ChainableNodeContainer : StyleNodeContainer {
        public string value;
        public bool invert;
        public ChainableNodeContainer next;

        public string GetGroupName() {
            if (next != null) return $"{(invert ? "!" : "")}{identifier}+{next.GetGroupName()}";
            return identifier;
        }
    }
}
