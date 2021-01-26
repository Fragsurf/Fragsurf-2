using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class TemplateRootNode : TemplateNode {

        public readonly string templateName;
        public readonly TemplateShell templateShell;
        
        public LightList<SlotNode> slotDefinitionNodes;

        public TemplateRootNode(string templateName, TemplateShell templateShell, ProcessedType processedType, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo) : base(null, null, processedType, attributes, in templateLineInfo) {
            this.templateName = templateName;
            this.templateShell = templateShell;
        }

        public void AddSlot(SlotNode slotNode) {
            slotDefinitionNodes = slotDefinitionNodes ?? new LightList<SlotNode>(4);
            slotDefinitionNodes.Add(slotNode);
        }

        public bool DefinesSlot(string slotName, out SlotNode slotNode) {
            if (slotDefinitionNodes == null || slotDefinitionNodes.size == 0) {
                slotNode = null;
                return false;
            }

            for (int i = 0; i < slotDefinitionNodes.size; i++) {
                if (slotDefinitionNodes.array[i].slotName == slotName) {
                    slotNode = slotDefinitionNodes.array[i];
                    return true;
                }
            }

            slotNode = null;
            return false;
        }

        public TemplateRootNode Clone(ProcessedType overrideType) {
            TemplateRootNode rootNode = new TemplateRootNode(templateName, templateShell, overrideType, attributes, lineInfo);
            rootNode.children = children;
            rootNode.slotDefinitionNodes = slotDefinitionNodes;
            return rootNode;
        }

    }

}