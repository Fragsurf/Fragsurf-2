using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public enum SlotType {

        Define,
        Forward,
        Override,
        Template

    }

    public class SlotNode : TemplateNode {

        public readonly string slotName;
        public readonly SlotType slotType;
        public StructList<AttributeDefinition> injectedAttributes;

        public SlotNode(TemplateRootNode root, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo, string slotName, SlotType slotType)
            : base(root, parent, processedType, attributes, templateLineInfo) {
            this.slotName = slotName;
            this.slotType = slotType;
            this.tagName = slotName;
        }

        public AttributeDefinition[] GetAttributes(AttributeType expose) {
            if (attributes == null) {
                return null;
            }

            int cnt = 0;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == expose) {
                    cnt++;
                }
            }

            if (cnt == 0) return null;
            int idx = 0;
            AttributeDefinition[] retn = new AttributeDefinition[cnt];
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == expose) {
                    retn[idx++] = attributes.array[i];
                }
            }

            return retn;
        }

    }

}