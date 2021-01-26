using System;
using UIForia.Animation;
using UIForia.Elements;

namespace UIForia {

    public struct ElementAnimator {

        public readonly UIElement element;
        private readonly AnimationSystem animationSystem;

        internal ElementAnimator(AnimationSystem animationSystem, UIElement element) {
            this.element = element;
            this.animationSystem = animationSystem;
        }

        public bool TryGetAnimationData(string animationName, out AnimationData animationData) {
            return element.templateMetaData.TryResolveAnimationByName(animationName, out animationData);
        }
        
        public AnimationTask PlayAnimation(in AnimationData animation) {
            return animationSystem.Animate(element, animation);
        }

        public void PauseAnimation(in AnimationData animationData) {
            animationSystem.PauseAnimation(element, animationData);
        }

        public void ResumeAnimation(AnimationData animationData) {
            animationSystem.ResumeAnimation(element, animationData);
        }

        public void StopAnimation(AnimationData animationData) {
            animationSystem.StopAnimation(element, animationData);
        }

        public bool TryGetActiveAnimation(string animationName, out AnimationTask animationTask) {
            animationTask = animationSystem.GetActiveAnimationByName(element, animationName);
            return animationTask != null;
        }

    }

}