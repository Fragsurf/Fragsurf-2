using System.Diagnostics;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct TemplateScope {

        public Application application;
        public StructList<SlotUsage> slotInputs;
        public StructList<SlotUsage> parentInputs;
        public UIElement innerSlotContext;

        [DebuggerStepThrough]
        public TemplateScope(Application application) {
            this.application = application;
            this.slotInputs = null;
            this.parentInputs = null;
            this.innerSlotContext = null;
        }

        public void AddSlotOverride(string slotName, UIElement context, int slotId) {
            slotInputs = slotInputs ?? StructList<SlotUsage>.Get();
            slotInputs.Add(new SlotUsage(slotName, slotId, context));
        }

        public void AddSlotForward(TemplateScope parentScope, string slotName, UIElement context, int slotId) {
            slotInputs = slotInputs ?? StructList<SlotUsage>.Get();
            if (parentScope.slotInputs == null) {
                slotInputs.Add(new SlotUsage(slotName, slotId, context));
                return;
            }

            for (int i = 0; i < parentScope.slotInputs.size; i++) {
                if (parentScope.slotInputs.array[i].slotName == slotName) {
                    slotInputs.Add(parentScope.slotInputs.array[i]);
                }
            }

            slotInputs.Add(new SlotUsage(slotName, slotId, context));
        }

        public void SetParentScope(TemplateScope parentScope) {
            this.parentInputs = parentScope.slotInputs;
        }

        public TemplateScope GetOverrideScope() {
            return new TemplateScope() {
                slotInputs = parentInputs,
                application = application,
                innerSlotContext = innerSlotContext,
                parentInputs = null
            };
        }

        public TemplateScope Clone() {
             return new TemplateScope() {
                slotInputs = slotInputs?.Clone(),
                application = application,
                innerSlotContext = innerSlotContext,
                parentInputs = parentInputs?.Clone()
            };
        }
        
        public void Release() {
            slotInputs?.Release();
            application = null;
            innerSlotContext = null;
        }

    }

}