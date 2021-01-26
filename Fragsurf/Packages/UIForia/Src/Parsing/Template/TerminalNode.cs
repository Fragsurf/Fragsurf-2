using UIForia.Exceptions;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class TerminalNode : TemplateNode {

        public TerminalNode(TemplateRootNode root, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo) : base(root, parent, processedType, attributes, templateLineInfo) { }

        public override void AddChild(TemplateNode child) {
            throw new InvalidArgumentException($"{root.root} -> Terminal element {processedType.rawType} cannot accept children. {lineInfo.line}:{lineInfo.column}");
        }

    }

}