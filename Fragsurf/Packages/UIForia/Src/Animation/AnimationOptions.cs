using UIForia.Rendering;
using UIForia.Sound;

namespace UIForia.Animation {

    public enum AnimationPlaybackType {

        KeyFrame,
        Parallel,
        Sequential

    }
    
    public enum AnimationDirection {

        Forward,
        Reverse,

    }

    public enum AnimationLoopType {

        Constant,
        PingPong

    }

    public struct AnimationProgress {

        public float elapsedTime;
        public int iterationCount;
        public AnimationDirection currentDirection;

    }
    
    public struct AnimationOptions {

        public float? loopTime;
        public int? iterations;
        public UITimeMeasurement? delay;
        public UITimeMeasurement? duration;
        public int? forwardStartDelay;
        public int? reverseStartDelay;
        public AnimationDirection? direction;
        public AnimationLoopType? loopType;            
        public EasingFunction? timingFunction;      // todo add Easing.Custom?

        // options for spritesheet animations
        public int? fps;
        public int? startFrame;
        public int? endFrame;
        public string pathPrefix;

        public AnimationOptions(AnimationOptions copy) {
            this.duration = copy.duration;
            this.loopTime = copy.loopTime;
            this.iterations = copy.iterations;
            this.direction = copy.direction;
            this.delay = copy.delay;
            this.forwardStartDelay = copy.forwardStartDelay;
            this.reverseStartDelay = copy.reverseStartDelay;
            this.timingFunction = copy.timingFunction;
            this.loopType = loopType = copy.loopType;
            this.fps = copy.fps;
            this.startFrame = copy.startFrame;
            this.endFrame = copy.endFrame;
            this.pathPrefix = copy.pathPrefix;
        }
        
        public AnimationOptions(int duration, float loopTime = -1) {
            this.duration = new UITimeMeasurement(duration, UITimeMeasurementUnit.Milliseconds);
            this.loopTime = loopTime;
            this.iterations = 1;
            this.direction = AnimationDirection.Forward;
            this.delay = new UITimeMeasurement(0, UITimeMeasurementUnit.Milliseconds);
            this.forwardStartDelay = 0;
            this.reverseStartDelay = 0;
            this.timingFunction = EasingFunction.Linear;
            this.loopType = loopType = AnimationLoopType.PingPong;
            this.fps = 60;
            this.startFrame = 0;
            this.endFrame = 0;
            this.pathPrefix = null;
        }

        public AnimationOptions(int duration, EasingFunction easing) {
            this.duration = new UITimeMeasurement(duration, UITimeMeasurementUnit.Milliseconds);
            this.iterations = 1;
            this.loopTime = 0f;
            this.delay = new UITimeMeasurement(0, UITimeMeasurementUnit.Milliseconds);
            this.forwardStartDelay = 0;
            this.reverseStartDelay = 0;
            this.timingFunction = easing;
            this.direction = AnimationDirection.Forward;
            this.loopType = loopType = AnimationLoopType.PingPong;
            this.fps = 60;
            this.startFrame = 0;
            this.endFrame = 0;
            this.pathPrefix = null;
        }

        public const int InfiniteIterations = -1;

        public bool IterateInfinitely => iterations == InfiniteIterations;

        public bool Equals(AnimationOptions other) {
            return this == other;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AnimationOptions && Equals((AnimationOptions) obj);
        }

        public static bool operator ==(AnimationOptions a, AnimationOptions b) {
            return a.duration == b.duration
                   && a.iterations == b.iterations
                   && a.loopTime == b.loopTime
                   && a.delay == b.delay
                   && a.forwardStartDelay == b.forwardStartDelay
                   && a.reverseStartDelay == b.reverseStartDelay
                   && a.timingFunction == b.timingFunction
                   && a.direction == b.direction
                   && a.loopType == b.loopType
                   && a.fps == b.fps
                   && a.startFrame == b.startFrame
                   && a.endFrame == b.endFrame
                   && a.pathPrefix == b.pathPrefix;
        }

        public static bool operator !=(AnimationOptions a, AnimationOptions b) {
            return !(a == b);
        }
    }
}