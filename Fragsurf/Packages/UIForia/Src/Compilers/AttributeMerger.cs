using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    public static class AttributeMerger {

        public static StructList<AttributeDefinition> MergeSlotAttributes(StructList<AttributeDefinition> innerAttributes, SlotAttributeData slotAttributeData, StructList<AttributeDefinition> outerAttributes) {
            StructList<AttributeDefinition> retn = new StructList<AttributeDefinition>();

            if (innerAttributes == null && outerAttributes == null) {
                retn = new StructList<AttributeDefinition>();
            }

            if (innerAttributes != null) {
                retn.AddRange(innerAttributes);
            }

            if (outerAttributes != null) {
                for (int i = 0; i < outerAttributes.size; i++) {
                    AttributeDefinition attrCopy = outerAttributes[i];
                    attrCopy.slotAttributeData = slotAttributeData;
                    retn.Add(attrCopy);
                }
            }

            return retn;
        }
        
        public static StructList<AttributeDefinition> MergeModifySlotAttributes(StructList<AttributeDefinition> innerAttributes, StructList<AttributeDefinition> outerAttributes) {
            StructList<AttributeDefinition> retn = new StructList<AttributeDefinition>();

            if (innerAttributes == null && outerAttributes == null) {
                retn = new StructList<AttributeDefinition>();
            }

            if (innerAttributes != null) {
                retn.AddRange(innerAttributes);
            }

            if (outerAttributes != null) {
                for (int i = 0; i < outerAttributes.size; i++) {
                    AttributeDefinition attrCopy = outerAttributes[i];
                    retn.Add(attrCopy);
                }
            }

            return retn;
        }

        // Template Root attribute rules
        // - No if binding allowed
        // - No property bindings allowed 
        // - Dynamic attr bindings are ok 
        // - Dynamic style bindings are ok
        // - Style bindings are ok
        // - Event subscriptions are ok
        // - Input handler declarations are ok
        // - Context variables are ok

        public static StructList<AttributeDefinition> MergeExpandedAttributes(StructList<AttributeDefinition> innerAttributes, StructList<AttributeDefinition> outerAttributes) {
            StructList<AttributeDefinition> output = null;

            if (innerAttributes == null) {
                if (outerAttributes == null) {
                    return null;
                }

                output = new StructList<AttributeDefinition>(outerAttributes.size);
                for (int i = 0; i < outerAttributes.size; i++) {
                    AttributeDefinition attr = outerAttributes.array[i];
                    output.AddUnsafe(attr);
                }

                return output;
            }

            if (outerAttributes == null) {
                output = new StructList<AttributeDefinition>(innerAttributes.size);
                for (int i = 0; i < innerAttributes.size; i++) {
                    AttributeDefinition attr = innerAttributes.array[i];
                    attr.flags |= AttributeFlags.InnerContext;
                    output.AddUnsafe(attr);
                }

                return output;
            }

            output = new StructList<AttributeDefinition>(innerAttributes.size + outerAttributes.size);

            for (int i = 0; i < innerAttributes.size; i++) {
                AttributeDefinition attr = innerAttributes.array[i];
                attr.flags |= AttributeFlags.InnerContext;
                output.AddUnsafe(attr);
            }

            const AttributeType replacedType = AttributeType.Attribute | AttributeType.InstanceStyle;

            for (int i = 0; i < outerAttributes.size; i++) {
                ref AttributeDefinition attr = ref outerAttributes.array[i];

                int idx = ContainsAttr(attr, output);

                if (idx == -1) {
                    output.AddUnsafe(attr);
                    continue;
                }

                if ((attr.type & replacedType) != 0) {
                    output.array[idx] = attr;
                }
                else {
                    output.AddUnsafe(attr);
                }
            }

            return output;
        }

        private static int ContainsAttr(in AttributeDefinition a, StructList<AttributeDefinition> list) {
            for (int i = 0; i < list.size; i++) {
                ref AttributeDefinition b = ref list.array[i];
                if (a.type == b.type && a.key == b.key) {
                    return i;
                }
            }

            return -1;
        }

    }

}