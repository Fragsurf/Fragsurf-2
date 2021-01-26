using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class TextNode : TemplateNode {

        public readonly string rawTextContent;
        public readonly StructList<TextExpression> textExpressionList;

        public TextNode(TemplateRootNode root, TemplateNode parent, string content, ProcessedType processedType, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo)
            : base(root, parent, processedType, attributes, templateLineInfo) {
            this.textExpressionList = new StructList<TextExpression>(3);
            this.rawTextContent = content;
            this.attributes = attributes;
            this.processedType = processedType;
        }

        public bool IsTextConstant() {
            if (textExpressionList == null || textExpressionList.size == 0) return false;

            for (int i = 0; i < textExpressionList.Count; i++) {
                if (textExpressionList.array[i].isExpression) {
                    return false;
                }
            }

            return true;
        }

        public string GetStringContent() {
            string retn = "";
            if (textExpressionList == null) return retn;

            for (int i = 0; i < textExpressionList.Count; i++) {
                retn += textExpressionList[i].text;
            }

            return retn;
        }

    }

}