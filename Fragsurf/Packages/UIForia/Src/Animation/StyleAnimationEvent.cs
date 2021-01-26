using UIForia.Elements;

namespace UIForia.Animation {

    public struct StyleAnimationEvent {

        public readonly UIElement target;
        public readonly AnimationEventType evtType;
        public readonly AnimationState state;
        public readonly AnimationOptions options;

        internal StyleAnimationEvent(AnimationEventType type, UIElement target, AnimationState state, AnimationOptions options) {
            this.evtType = type;
            this.target = target;
            this.state = state;
            this.options = options;
        }

    }

}