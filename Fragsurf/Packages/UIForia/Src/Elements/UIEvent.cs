using UIForia.UIInput;

namespace UIForia.Elements {
    public class UIEvent {

        public UIElement origin { get; internal set; }

        public readonly string eventType;
        
        private bool propagating;

        public readonly KeyboardInputEvent keyboardInputEvent;

        protected UIEvent(string eventType) {
            this.eventType = eventType;
            propagating = true;
        }
        
        protected UIEvent(string eventType, KeyboardInputEvent keyboardInputEvent) {
            this.eventType = eventType;
            this.keyboardInputEvent = keyboardInputEvent;
            propagating = true;
        }

        public void StopPropagation() {
            propagating = false;
        }

        public bool IsPropagating() {
            return propagating;
        }
    }
    
}
