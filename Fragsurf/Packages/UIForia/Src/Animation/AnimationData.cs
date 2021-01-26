using System;
using System.Collections.Generic;

namespace UIForia.Animation {

    public struct AnimationData {

        public AnimationType animationType;
        public string name;
        public string fileName;
        public AnimationOptions options;
        public IList<StyleAnimationKeyFrame> frames;
        public IList<MaterialAnimationKeyFrame> materialFrames;
        public IList<AnimationTrigger> triggers;

        public Action<StyleAnimationEvent> onStart;
        public Action<StyleAnimationEvent> onEnd;
        public Action<StyleAnimationEvent> onCanceled;
        public Action<StyleAnimationEvent> onCompleted;
        public Action<StyleAnimationEvent> onTick;

        public AnimationData(AnimationOptions options, IList<StyleAnimationKeyFrame> frames = null, IList<AnimationTrigger> triggers = null, AnimationType animationType = AnimationType.KeyFrame) {
            this.animationType = animationType;
            this.options = options;
            this.triggers = null;
            this.onStart = null;
            this.onEnd = null;
            this.onCanceled = null;
            this.onCompleted = null;
            this.onTick = null;
            this.frames = frames;
            this.triggers = triggers;
            this.name = null;
            this.fileName = null;
            this.materialFrames = null;
        }

    }

}