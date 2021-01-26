namespace UIForia.Elements {

    public class UISlotBase : UIContainerElement {

        public string slotId;

    }

    public class UISlotDefinition : UISlotBase {

        public override string GetDisplayName() {
            return "define:" + slotId;
        }

    }

}