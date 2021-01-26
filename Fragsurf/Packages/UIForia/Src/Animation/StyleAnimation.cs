using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Animation {

    // animation -> runner + context + state
    // data -> prototype for an animation 
    // state -> struct containing status for given animation

    public abstract class StyleAnimation : AnimationTask {

        public UIElement target;
        public AnimationState status;

        protected static readonly StyleKeyFrameSorter s_StyleKeyFrameSorter = new StyleKeyFrameSorter();
        protected static readonly MaterialKeyFrameSorter s_MaterialKeyFrameSorter = new MaterialKeyFrameSorter();

        protected StyleAnimation(UIElement target, AnimationData data) : base(data, data.triggers) {
            this.target = target;
            ResetTriggers();
        }

        public void ResetTriggers() {
            if (triggerStates == null) return;
            for (int i = 0; i < triggerStates.Count; i++) {
                AnimationTriggerState triggerState = triggerStates[i];
                triggerState.fireCount = 0;
                triggerStates[i] = triggerState;
            }
        }

        public void RunTriggers() {
            if (triggerStates == null) {
                return;
            }

            StyleAnimationEvent evt = new StyleAnimationEvent(AnimationEventType.Trigger, target, status, animationData.options);

            float progress = status.iterationProgress;
            for (int i = 0; i < triggerStates.Count; i++) {
                AnimationTriggerState triggerState = triggerStates[i];
                if (progress >= triggerState.time && triggerState.fireCount == 0) {
                    triggerState.fireCount++;
                    triggerState.fn?.Invoke(evt);
                    triggerStates[i] = triggerState;
                }
            }
        }

        protected static float ResolveFixedWidth(UIElement element, Rect viewport, UIFixedLength width) {
            switch (width.unit) {
                case UIFixedUnit.Pixel:
                    return width.value;

                case UIFixedUnit.Percent:
                    return element.layoutResult.AllocatedWidth * width.value;

                case UIFixedUnit.ViewportHeight:
                    return viewport.height * width.value;

                case UIFixedUnit.ViewportWidth:
                    return viewport.width * width.value;

                case UIFixedUnit.Em:
                    return element.style.TextFontAsset.faceInfo.pointSize * width.value;

                default:
                    return 0;
            }
        }

        protected static float ResolveFixedHeight(UIElement element, Rect viewport, UIFixedLength height) {
            switch (height.unit) {
                case UIFixedUnit.Pixel:
                    return height.value;

                case UIFixedUnit.Percent:
                    return element.layoutResult.AllocatedHeight * height.value;

                case UIFixedUnit.ViewportHeight:
                    return viewport.height * height.value;

                case UIFixedUnit.ViewportWidth:
                    return viewport.width * height.value;

                case UIFixedUnit.Em:
                    return element.style.TextFontAsset.faceInfo.pointSize * height.value;

                default:
                    return 0;
            }
        }

        protected static float ResolveWidthMeasurement(UIElement element, Rect viewport, UIMeasurement measurement) {
            switch (measurement.unit) {
                case UIMeasurementUnit.Unset:
                    return 0;

                case UIMeasurementUnit.Pixel:
                    return measurement.value;

                case UIMeasurementUnit.Content:
                    return element.layoutResult.actualSize.width * measurement.value;

                case UIMeasurementUnit.BlockSize:
                    if (element.parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, element.parent.layoutResult.AllocatedWidth * measurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, viewport.width * measurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, viewport.height * measurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    UIStyleSet parentStyle = element.parent.style;
                    if (parentStyle.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, (element.parent.layoutResult.AllocatedWidth
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingLeft)
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingRight)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderRight)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderLeft)));
                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, element.style.TextFontAsset.faceInfo.pointSize * measurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected static float ResolveHeightMeasurement(UIElement element, Rect viewport, UIMeasurement measurement) {
            switch (measurement.unit) {
                case UIMeasurementUnit.Unset:
                    return 0;

                case UIMeasurementUnit.Pixel:
                    return measurement.value;

                case UIMeasurementUnit.Content:
                    return element.layoutResult.actualSize.height * measurement.value;

                case UIMeasurementUnit.BlockSize:
                    if (element.parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, element.parent.layoutResult.AllocatedHeight * measurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, viewport.width * measurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, viewport.height * measurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    UIStyleSet parentStyle = element.parent.style;
                    if (parentStyle.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, (element.parent.layoutResult.AllocatedHeight
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingTop)
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingBottom)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderBottom)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderTop)));
                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, element.style.TextFontAsset.faceInfo.pointSize * measurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}