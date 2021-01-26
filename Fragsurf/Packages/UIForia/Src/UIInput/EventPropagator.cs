using UIForia.Elements;
using UIForia.Systems.Input;

namespace UIForia.UIInput {

    public class EventPropagator {

        public bool isConsumed;
        public bool shouldStopPropagation;
        public UIElement origin;

        public MouseState mouseState;

        public void Reset(MouseState mouseState) {
            this.mouseState = mouseState;
            shouldStopPropagation = false;
            isConsumed = false;
            origin = null;
        }

    }

}