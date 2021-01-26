using System.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public class CompiledTemplate {

        public string guid;
        public int templateId;
        public string filePath;
        public string templateName;
        public LambdaExpression templateFn;
        public TemplateMetaData templateMetaData;
        internal ProcessedType elementType;
        public LightList<CompiledBinding> bindings;
        public LightList<CompiledSlot> slots;
        public LightList<string> namespaces;

        public void AddBinding(CompiledBinding binding) {
            bindings = bindings ?? new LightList<CompiledBinding>();
            bindings.Add(binding);
        }

        public void AddSlot(CompiledSlot slot) {
            slots = slots ?? new LightList<CompiledSlot>();
            slots.Add(slot);
        }

        public CompiledSlot GetCompiledSlot(string slotName) {
            if (slots == null) return null;
            for (int i = 0; i < slots.size; i++) {
                CompiledSlot slot = slots.array[i];
                if ((slot.slotType == SlotType.Forward || slot.slotType == SlotType.Define) && slot.slotName == slotName) {
                    return slot;
                }
            }

            return null;
        }

    }

}