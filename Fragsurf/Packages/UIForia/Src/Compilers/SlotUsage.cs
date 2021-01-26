using UIForia.Elements;

namespace UIForia.Compilers {
    
    public readonly struct SlotUsage {

        public readonly string slotName;
        public readonly int slotId;
        public readonly UIElement context;
        
        public SlotUsage(string slotName, int slotId, UIElement context) {
            this.slotName = slotName;
            this.slotId = slotId;
            this.context = context;
        }

    }

}