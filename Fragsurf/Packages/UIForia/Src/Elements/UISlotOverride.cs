namespace UIForia.Elements {

    public class UISlotOverride : UISlotBase {

        public override string GetDisplayName() {
            return "override:" + slotId;
        }
    }

}