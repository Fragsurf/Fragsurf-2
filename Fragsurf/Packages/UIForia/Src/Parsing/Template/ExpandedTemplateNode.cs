using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class ExpandedTemplateNode : TemplateNode {

        public LightList<SlotNode> slotOverrideNodes;

        public ExpandedTemplateNode(TemplateRootNode root, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo)
            : base(root, parent, processedType, attributes, templateLineInfo) { }

        public override void AddChild(TemplateNode child) {
            if (child is SlotNode slotNode) {
                AddSlotOverride(slotNode);
            }
            else {
                SlotNode childrenSlot = FindOrCreateChildrenSlotOverride();
                childrenSlot.AddChild(child);
            }
        }

        private SlotNode FindOrCreateChildrenSlotOverride() {
            if (slotOverrideNodes == null) {
                slotOverrideNodes = new LightList<SlotNode>(1);
            }

            for (int i = 0; i < slotOverrideNodes.size; i++) {
                if (slotOverrideNodes.array[i].slotName == "Children") {
                    return slotOverrideNodes.array[i];
                }
            }

            SlotNode slot = new SlotNode(root, null, TypeProcessor.GetProcessedType(typeof(UIChildrenElement)), null, lineInfo, "Children", SlotType.Override);

            slotOverrideNodes.Add(slot);
            return slot;
        }

        public void AddSlotOverride(SlotNode node) {
            slotOverrideNodes = slotOverrideNodes ?? new LightList<SlotNode>(2);
            for (int i = 0; i < slotOverrideNodes.size; i++) {
                if (slotOverrideNodes.array[i].slotName == node.slotName) {
                    throw ParseException.MultipleSlotOverrides(node.slotName);
                }
            }

            slotOverrideNodes.Add(node);
        }

    }

}