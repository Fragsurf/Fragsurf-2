using UIForia.UIInput;

namespace UIForia.Elements {
    public sealed class TabNavigationEvent : UIEvent {
        
        public TabNavigationEvent() : base("tabnavigation") {}
        
        public TabNavigationEvent(KeyboardInputEvent keyboardInputEvent) 
            : base("tabnavigation", keyboardInputEvent) {}
    }
}
