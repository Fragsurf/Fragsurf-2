using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class ChildrenNode : SlotNode {

        public ChildrenNode(TemplateRootNode root, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo, SlotType slotType) 
            : base(root, parent, processedType, attributes, templateLineInfo, "Children", slotType) { }

    }

}