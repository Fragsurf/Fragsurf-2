using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class ContainerNode : TemplateNode {

        public ContainerNode(TemplateRootNode root, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo) : base(root, parent, processedType, attributes, templateLineInfo) { }

    }

}