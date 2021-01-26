using System;

namespace UIForia.Animation {

    public struct AnimationTriggerState {

        public Action<StyleAnimationEvent> fn;
        public float time;
        public int fireCount;

        public AnimationTriggerState(AnimationTrigger trigger) {
            this.fireCount = 0;
            this.fn = trigger.fn;
            this.time = trigger.time;
        }

    }

}