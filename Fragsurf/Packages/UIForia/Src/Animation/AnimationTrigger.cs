using System;

namespace UIForia.Animation {

    public struct AnimationTrigger {

        public readonly float time;
        public readonly Action<StyleAnimationEvent> fn;

        public AnimationTrigger(float time, Action<StyleAnimationEvent> fn) {
            this.time = time;
            this.fn = fn;
        }

    }

}