namespace UIForia.UIInput {

    public class InputEvent {

        public readonly InputEventType type;
        private bool m_ShouldStopPropagation;
        private bool m_ShouldStopLateralPropagation;

        public InputEvent(InputEventType type) {
            this.type = type;
        }

    }

}