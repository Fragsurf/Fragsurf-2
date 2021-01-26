using System;

namespace UIForia.UIInput {

    public class FocusEvent : InputEvent {

        public FocusEvent() : base(InputEventType.Focus) { }

    }
    
    public class BlurEvent : InputEvent {

        public BlurEvent() : base(InputEventType.Blur) { }

    }

    public interface IFocusable {
      
        bool Focus();
        void Blur();

    }

    public interface IFocusableEvented : IFocusable {

        event Action<FocusEvent> onFocus;
        event Action<BlurEvent> onBlur;

    }

}