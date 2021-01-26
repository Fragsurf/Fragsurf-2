using UIForia.Elements;
using UIForia.Systems;

namespace UIForia.Animation {

    public struct AnimationState {

        public readonly UIElement target;

        public float elapsedTotalTime;
        public float elapsedIterationTime;
        public int iterationCount;
        public int frameCount;
        public float totalProgress;
        public float iterationProgress;
        public UITaskState status;
        public int currentIteration;
        
        internal AnimationState(UIElement target) {
            this.target = target;
            elapsedTotalTime = 0;
            elapsedIterationTime = 0;
            iterationCount = 0;
            frameCount = 0;
            totalProgress = 0;
            iterationProgress = 0;
            status = 0;
            currentIteration = 0;
        }

    }

}