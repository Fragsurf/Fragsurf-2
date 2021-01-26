using System;
using System.Text;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Sound;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIForia.Animation {

    public class SpriteSheetAnimation : StyleAnimation {

        private float lastFrameTime;

        private LightList<Texture2D> frames;

        public SpriteSheetAnimation(UIElement target, AnimationData animationData) : base(target, animationData) {
            frames = new LightList<Texture2D>();
            if (!animationData.options.startFrame.HasValue) {
                throw new Exception($"SpriteSheetAnimations must define a startFrame. File: {animationData.fileName}>{animationData.name}");
            }

            if (animationData.options.pathPrefix.IndexOf('{') > -1) {
                for (int i = animationData.options.startFrame.Value; i <= animationData.options.endFrame; i++) {
                    frames.Add(target.application.ResourceManager.GetTexture(string.Format(animationData.options.pathPrefix, i)));
                }
            }
            else {
                for (int i = animationData.options.startFrame.Value; i <= animationData.options.endFrame; i++) {
                    frames.Add(target.application.ResourceManager.GetTexture(animationData.options.pathPrefix + i));
                }
            }
        }

        public override UITaskResult Run(float deltaTime) {
            status.elapsedTotalTime += deltaTime;
            status.elapsedIterationTime += deltaTime;
            
            if (state == UITaskState.Cancelled) {
                return UITaskResult.Cancelled;
            }

            if (frames.size == 0) {
                return UITaskResult.Completed;
            }

            AnimationOptions options = animationData.options;
            float delay = options.delay?.AsSeconds ?? 0; 
            if (delay > status.elapsedIterationTime) {
                return UITaskResult.Running;
            }

            bool isReversed = options.direction.HasValue && options.direction.Value == AnimationDirection.Reverse;
            if (options.duration != new UITimeMeasurement(1, UITimeMeasurementUnit.Percentage)) {
                float duration = options.duration?.AsSeconds ?? 1;
                if (duration - delay <= status.elapsedTotalTime) {
                    target.style.SetBackgroundImage(frames[isReversed ? frames.size - 1 : 0], StyleState.Normal);
                    return UITaskResult.Completed;
                }
            }

            target.style.SetBackgroundImage(frames[isReversed ? frames.size - 1 - status.frameCount : status.frameCount], StyleState.Normal);

            if (lastFrameTime == 0 || status.elapsedTotalTime - lastFrameTime >= (1f / options.fps)) {
                lastFrameTime = status.elapsedTotalTime;

                if (options.endFrame - options.startFrame <= status.frameCount) {
                    if (status.iterationCount + 1 < options.iterations || options.IterateInfinitely) {
                        status.frameCount = 0;
                        status.iterationCount++;
                        status.elapsedIterationTime = 0;
                    }
                    else {
                        return UITaskResult.Completed;
                    }
                }
                else {
                    status.frameCount++;
                    status.iterationProgress = status.frameCount;
                }
            }

            return UITaskResult.Running;
        }
    }
}
