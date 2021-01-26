using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Animation {

    public class StyleKeyFrameAnimation : StyleAnimation {

        private LightList<ProcessedStyleKeyFrameGroup> processedStyleFrameGroups;
        private LightList<ProcessedMaterialKeyFrameGroup> processedMaterialFrameGroups;

        public StyleKeyFrameAnimation(UIElement target, AnimationData data) : base(target, data) {
            ProcessStyleKeyFrames(data.frames);
            ProcessMaterialKeyFrames(data.materialFrames);
        }

        private void ProcessMaterialKeyFrames(IList<MaterialAnimationKeyFrame> frames) {
            if (frames == null || frames.Count == 0) {
                return;
            }

            processedMaterialFrameGroups = new LightList<ProcessedMaterialKeyFrameGroup>();
            // todo -- ensure we release lists where we need to
            // todo -- dont use a list each processed frame group, use a single list sorted sensibly
            for (int i = 0; i < frames.Count; i++) {
                MaterialAnimationKeyFrame f = frames[i];
                MaterialKeyFrameValue[] properties = f.properties;

                for (int j = 0; j < f.properties.Length; j++) {
                    AddMaterialKeyFrame(f.key, properties[j]);
                }

            }

            for (int i = 0; i < processedMaterialFrameGroups.Count; i++) {

                MaterialPropertyId property = processedMaterialFrameGroups[i].materialIdPair;

                LightList<ProcessedMaterialKeyFrame> processedKeyFrames = processedMaterialFrameGroups[i].frames;

                processedKeyFrames.Sort(s_MaterialKeyFrameSorter);

                if (processedKeyFrames[0].time != 0f) {
                    processedKeyFrames.Insert(0, new ProcessedMaterialKeyFrame(0f, default));
                }
            }
        }

        public void ProcessStyleKeyFrames(IList<StyleAnimationKeyFrame> frames) {
            if (frames == null || frames.Count == 0) {
                return;
            }

            processedStyleFrameGroups = new LightList<ProcessedStyleKeyFrameGroup>();

            for (int i = 0; i < frames.Count; i++) {
                StyleAnimationKeyFrame f = frames[i];
                StyleKeyFrameValue[] properties = f.properties.Array;
                for (int j = 0; j < f.properties.Count; j++) {
                    AddKeyFrame(f.key, properties[j]);
                }
            }

            for (int i = 0; i < processedStyleFrameGroups.Count; i++) {
                StylePropertyId property = processedStyleFrameGroups[i].propertyId;
                LightList<ProcessedStyleKeyFrame> processedKeyFrames = processedStyleFrameGroups[i].frames;
                processedKeyFrames.Sort(s_StyleKeyFrameSorter);
                if (processedKeyFrames[0].time != 0f) {
                    processedKeyFrames.Insert(0, new ProcessedStyleKeyFrame(0f, target.style.GetComputedStyleProperty(property)));
                }
            }
        }

        private void AddMaterialKeyFrame(float time, MaterialKeyFrameValue property) {

            for (int i = 0; i < processedStyleFrameGroups.Count; i++) {

                ref ProcessedMaterialKeyFrameGroup p = ref processedMaterialFrameGroups.array[i];

                if (p.materialIdPair.materialId.baseId == property.materialId.baseId && p.materialIdPair.propertyId == property.propertyId) {
                   // p.frames.Add(new ProcessedMaterialKeyFrame(time));
                    return;
                }
            }

            LightList<ProcessedMaterialKeyFrame> list = LightList<ProcessedMaterialKeyFrame>.Get();
           // list.Add(new ProcessedMaterialKeyFrame(time));
            processedMaterialFrameGroups.Add(new ProcessedMaterialKeyFrameGroup(new MaterialPropertyId(property.materialId, property.propertyId), list));
        }

        private void AddKeyFrame(float time, StyleKeyFrameValue property) {

            for (int i = 0; i < processedStyleFrameGroups.Count; i++) {
                if (processedStyleFrameGroups[i].propertyId == property.propertyId) {
                    processedStyleFrameGroups[i].frames.Add(new ProcessedStyleKeyFrame(time, property));
                    return;
                }
            }

            LightList<ProcessedStyleKeyFrame> list = LightList<ProcessedStyleKeyFrame>.Get();
            list.Add(new ProcessedStyleKeyFrame(time, property));
            processedStyleFrameGroups.Add(new ProcessedStyleKeyFrameGroup(property.propertyId, list));
        }

        public override UITaskResult Run(float deltaTime) {
            status.elapsedTotalTime += deltaTime;
            status.elapsedIterationTime += deltaTime;

            if (state == UITaskState.Cancelled) {
                return UITaskResult.Cancelled;
            }

            AnimationOptions options = animationData.options;
            float delay = options.delay?.AsSeconds ?? 0;
            if (delay > status.elapsedTotalTime) {
                return UITaskResult.Running;
            }

            float duration = options.duration?.AsSeconds ?? 1;
            float elapsedIterationTime = status.elapsedIterationTime - delay;
            float progress = Mathf.Clamp01(elapsedIterationTime / duration);

            status.iterationProgress = progress;
            status.frameCount++;

            float targetProgress = progress;
            bool isReversed = options.direction.HasValue && options.direction.Value == AnimationDirection.Reverse;

            if (isReversed) {
                targetProgress = 1 - targetProgress;
            }

            Rect viewport = target.View.Viewport;

            if (processedMaterialFrameGroups != null) {

                ProcessedMaterialKeyFrameGroup[] materialGroups = processedMaterialFrameGroups.array;

                for (int i = 0; i < processedMaterialFrameGroups.size; i++) {

                    ProcessedMaterialKeyFrame[] frames = materialGroups[i].frames.array;
                    int frameCount = materialGroups[i].frames.size;

                    ProcessedMaterialKeyFrame prev = frames[0];
                    ProcessedMaterialKeyFrame nextMaterialFrame = frames[frameCount - 1];

                    for (int j = 1; j < frameCount; j++) {
                        if (frames[j].time > targetProgress) {
                            prev = frames[j - 1];
                            nextMaterialFrame = frames[j];
                            break;
                        }
                    }

                    if (isReversed) {
                        ProcessedMaterialKeyFrame tmp = prev;
                        prev = nextMaterialFrame;
                        nextMaterialFrame = tmp;
                    }

                    float t = Mathf.Clamp01(Easing.Interpolate(MathUtil.PercentOfRange(targetProgress, prev.time, nextMaterialFrame.time), options.timingFunction.Value));

                    switch (prev.materialProperty.propertyType) {

                        case MaterialPropertyType.Color:
                            Color c0 = prev.materialProperty.value.colorValue;
                            Color c1 = nextMaterialFrame.materialProperty.value.colorValue;
                            // target.Material.SetColor(
                            //     materialGroups[i].materialIdPair.materialId,
                            //     materialGroups[i].materialIdPair.propertyId,
                            //     progress >= 1 ? c1 : Color.Lerp(c0, c1, t)
                            // );
                            break;

                        case MaterialPropertyType.Float:
                            float f0 = prev.materialProperty.value.floatValue;
                            float f1 = nextMaterialFrame.materialProperty.value.floatValue;
                            // target.Material.SetFloat(
                            //     materialGroups[i].materialIdPair.materialId,
                            //     materialGroups[i].materialIdPair.propertyId,
                            //     progress >= 1 ? f1 : Mathf.Lerp(f0, f1, t)
                            // );
                            break;

                        case MaterialPropertyType.Vector:
                            break;

                        case MaterialPropertyType.Range:
                            break;

                        case MaterialPropertyType.Texture:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                }

            }

            if (processedStyleFrameGroups != null) {

                ProcessedStyleKeyFrameGroup[] styleGroups = processedStyleFrameGroups.array;

                for (int i = 0; i < processedStyleFrameGroups.Count; i++) {

                    StylePropertyId propertyId = styleGroups[i].propertyId;
                    ProcessedStyleKeyFrame[] frames = styleGroups[i].frames.array;
                    int frameCount = styleGroups[i].frames.size;

                    ProcessedStyleKeyFrame prev = frames[0];
                    ProcessedStyleKeyFrame nextStyleFrame = frames[frameCount - 1];

                    for (int j = 1; j < frameCount; j++) {
                        if (frames[j].time > targetProgress) {
                            prev = frames[j - 1];
                            nextStyleFrame = frames[j];
                            break;
                        }
                    }

                    if (isReversed) {
                        ProcessedStyleKeyFrame tmp = prev;
                        prev = nextStyleFrame;
                        nextStyleFrame = tmp;
                    }

                    float t = Mathf.Clamp01(Easing.Interpolate(MathUtil.PercentOfRange(targetProgress, prev.time, nextStyleFrame.time), options.timingFunction.Value));

                    switch (propertyId) {
                        case StylePropertyId.TransformPivotX:
                        case StylePropertyId.PaddingLeft:
                        case StylePropertyId.PaddingRight:
                        case StylePropertyId.BorderLeft:
                        case StylePropertyId.BorderRight:
                        case StylePropertyId.MarginLeft:
                        case StylePropertyId.MarginRight: {
                            float v0 = ResolveFixedWidth(target, viewport, prev.value.styleProperty.AsUIFixedLength);

                            float v1 = ResolveFixedWidth(target, viewport, nextStyleFrame.value.styleProperty.AsUIFixedLength);
                            target.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIFixedLength(Mathf.Lerp(v0, v1, t))));
                            break;
                        }

                        case StylePropertyId.TransformPivotY:
                        case StylePropertyId.PaddingTop:
                        case StylePropertyId.PaddingBottom:
                        case StylePropertyId.BorderTop:
                        case StylePropertyId.BorderBottom:
                        case StylePropertyId.MarginTop:
                        case StylePropertyId.TextFontSize:
                        case StylePropertyId.MarginBottom: {
                            float v0 = ResolveFixedHeight(target, viewport, prev.value.styleProperty.AsUIFixedLength);

                            float v1 = ResolveFixedHeight(target, viewport, nextStyleFrame.value.styleProperty.AsUIFixedLength);
                            target.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIFixedLength(Mathf.Lerp(v0, v1, t))));
                            break;
                        }

                        case StylePropertyId.PreferredWidth:
                        case StylePropertyId.MinWidth:
                        case StylePropertyId.MaxWidth: {

                            float v0 = ResolveWidthMeasurement(target, viewport, prev.value.styleProperty.AsUIMeasurement);

                            float v1 = ResolveWidthMeasurement(target, viewport, nextStyleFrame.value.styleProperty.AsUIMeasurement);

                            //target.style.SetAnimatedMeasurementProperty(propertyId, prev.value.styleProperty, next.value.styleProperty, t);
                            target.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIMeasurement(Mathf.Lerp(v0, v1, t))));
                            break;
                        }

                        case StylePropertyId.PreferredHeight:
                        case StylePropertyId.MinHeight:
                        case StylePropertyId.MaxHeight: {
                            float v0 = ResolveHeightMeasurement(target, viewport, prev.value.styleProperty.AsUIMeasurement);

                            float v1 = ResolveHeightMeasurement(target, viewport, nextStyleFrame.value.styleProperty.AsUIMeasurement);

                            target.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIMeasurement(Mathf.Lerp(v0, v1, t))));
                            break;
                        }

                        case StylePropertyId.Opacity:
                        case StylePropertyId.ShadowOpacity:
                        case StylePropertyId.TextUnderlayDilate:
                        case StylePropertyId.TextUnderlaySoftness:
                        case StylePropertyId.TextUnderlayX:
                        case StylePropertyId.TextUnderlayY:
                        case StylePropertyId.TextFaceDilate:
                        case StylePropertyId.TextGlowInner:
                        case StylePropertyId.TextGlowOuter:
                        case StylePropertyId.TextGlowOffset:
                        case StylePropertyId.TextGlowPower:
                        case StylePropertyId.TextOutlineSoftness:
                        case StylePropertyId.TextOutlineWidth:

                        case StylePropertyId.ShadowIntensity:
                        case StylePropertyId.ShadowSizeX:
                        case StylePropertyId.ShadowSizeY:
                        case StylePropertyId.TransformScaleX:
                        case StylePropertyId.TransformScaleY:
                        case StylePropertyId.TransformRotation:
                        case StylePropertyId.GridLayoutColGap:
                        case StylePropertyId.GridLayoutRowGap: {
                            float v0 = prev.value.styleProperty.AsFloat;

                            float v1 = nextStyleFrame.value.styleProperty.AsFloat;

                            target.style.SetAnimatedProperty(new StyleProperty(propertyId, Mathf.Lerp(v0, v1, t)));
                            break;
                        }

                        case StylePropertyId.ShadowOffsetX:
                        case StylePropertyId.TransformPositionX:
                        case StylePropertyId.AlignmentOffsetX: {
                            Rect viewRect = target.View.Viewport;
                            if (target.layoutBox != null) {
                                float v0 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, prev.value.styleProperty.AsOffsetMeasurement, target.layoutResult.actualSize.width);
                                float v1 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, nextStyleFrame.value.styleProperty.AsOffsetMeasurement, target.layoutResult.actualSize.width);

                                target.style.SetAnimatedProperty(new StyleProperty(propertyId, new OffsetMeasurement(Mathf.Lerp(v0, v1, t))));
                            }

                            break;
                        }

                        case StylePropertyId.ShadowOffsetY:
                        case StylePropertyId.TransformPositionY:
                        case StylePropertyId.AlignmentOffsetY: {
                            Rect viewRect = target.View.Viewport;
                            if (target.layoutBox != null) {
                                float v0 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, prev.value.styleProperty.AsOffsetMeasurement, target.layoutResult.actualSize.height);
                                float v1 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, nextStyleFrame.value.styleProperty.AsOffsetMeasurement, target.layoutResult.actualSize.height);
                                target.style.SetAnimatedProperty(new StyleProperty(propertyId, new OffsetMeasurement(Mathf.Lerp(v0, v1, t))));
                            }

                            break;
                        }

                        case StylePropertyId.AlignmentOriginX: {
                            Rect viewRect = target.View.Viewport;

                            if (target.layoutBox != null) {
                                float originSize = MeasurementUtil.ResolveOffsetOriginSizeX(target.layoutResult, viewRect.width, target.style.AlignmentTargetX);
                                float v0 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, prev.value.styleProperty.AsOffsetMeasurement, originSize);
                                float v1 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, nextStyleFrame.value.styleProperty.AsOffsetMeasurement, originSize);
                                target.style.SetAnimatedProperty(new StyleProperty(propertyId, new OffsetMeasurement(Mathf.Lerp(v0, v1, t))));
                            }

                            break;
                        }

                        case StylePropertyId.AlignmentOriginY: {
                            Rect viewRect = target.View.Viewport;

                            if (target.layoutBox != null) {
                                float originSize = MeasurementUtil.ResolveOffsetOriginSizeY(target.layoutResult, viewRect.width, target.style.AlignmentTargetY);
                                float v0 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, prev.value.styleProperty.AsOffsetMeasurement, originSize);
                                float v1 = MeasurementUtil.ResolveOffsetMeasurement(target, viewRect.width, viewRect.height, nextStyleFrame.value.styleProperty.AsOffsetMeasurement, originSize);
                                target.style.SetAnimatedProperty(new StyleProperty(propertyId, new OffsetMeasurement(Mathf.Lerp(v0, v1, t))));
                            }

                            break;
                        }

                        case StylePropertyId.TextUnderlayColor:
                        case StylePropertyId.TextGlowColor:
                        case StylePropertyId.TextOutlineColor:
                        case StylePropertyId.BorderColorTop:
                        case StylePropertyId.BorderColorRight:
                        case StylePropertyId.BorderColorBottom:
                        case StylePropertyId.BorderColorLeft:
                        case StylePropertyId.BackgroundColor:
                        case StylePropertyId.BackgroundTint:
                        case StylePropertyId.ShadowColor:
                        case StylePropertyId.ShadowTint:
                        case StylePropertyId.TextColor: {
                            Color c0 = prev.value.styleProperty.AsColor;
                            Color c1 = nextStyleFrame.value.styleProperty.AsColor;
                            target.style.SetAnimatedProperty(new StyleProperty(propertyId, Color.Lerp(c0, c1, t)));
                            break;
                        }

                        default:
                            if (StyleUtil.CanAnimate(propertyId)) {
                                throw new NotImplementedException(propertyId + " can be animated but is not implemented");
                            }

                            target.style.SetProperty(prev.value.styleProperty, StyleState.Normal);
                            break;
                    }

                    if (progress >= 1f) {
                        target.style.SetProperty(nextStyleFrame.value.styleProperty, StyleState.Normal);
                    }

                }
            }

            return progress >= 1f ? UITaskResult.Completed : UITaskResult.Running;

        }

    }

}