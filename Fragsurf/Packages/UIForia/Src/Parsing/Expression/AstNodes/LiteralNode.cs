namespace UIForia.Parsing.Expressions.AstNodes {

    public class LiteralNode : ASTNode {

        public string rawValue;

        public override void Release() {
            rawValue = null;
            type = 0;
            s_LiteralPool.Release(this);
        }
    }

}