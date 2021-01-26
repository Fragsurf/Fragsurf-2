
namespace UIForia.UIInput {

    public struct DragCreatorGroup {

//        public readonly DragEventCreator[] creators;
//
//        public DragCreatorGroup(DragEventCreator[] creators) {
//            this.creators = creators;
//        }

        public DragEvent TryCreateEvent(object target, MouseInputEvent mouseEvent) {
//            for (int i = 0; i < creators.Length; i++) {
//                DragEvent evt = creators[i].Invoke(target, mouseEvent);
//                if (evt != null) {
//                    return evt;
//                }
//            }

            return null;
        }
    }
}
