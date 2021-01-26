using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIForia.Systems {

    public class AwesomeLayoutSystem : ILayoutSystem {

        private Application application;
        private LightList<AwesomeLayoutRunner> runners;
        internal int traversalIndex;

        internal LightList<UIElement> horizontalScrollFixed;
        internal LightList<UIElement> verticalScrollFixed;

        public AwesomeLayoutSystem(Application application) {
            this.application = application;
            this.runners = new LightList<AwesomeLayoutRunner>();
            this.horizontalScrollFixed = new LightList<UIElement>();
            this.verticalScrollFixed = new LightList<UIElement>();

            // for (int i = 0; i < application.views.Count; i++) {
            //     runners.Add(new AwesomeLayoutRunner(this, application.views[i].dummyRoot));
            // }

            application.onViewsSorted += uiViews => {
                runners.Sort((a, b) =>
                    Array.IndexOf(uiViews, b.rootElement.View) - Array.IndexOf(uiViews, a.rootElement.View));
            };

            application.StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
            // application.StyleSystem.onStylePropertyAnimated += HandleStylepropertyAnimated;
        }

        internal void CreateLayoutBox(UIElement currentElement) {
            if (currentElement is UITextElement) {
                currentElement.layoutBox = new AwesomeTextLayoutBox();
            }
            else if (currentElement is ScrollView) {
                currentElement.layoutBox = new AwesomeScrollViewLayoutBox();
            }
            else if (currentElement is UIImageElement) {
                currentElement.layoutBox = new AwesomeImageLayoutBox();
            }
            else {
                switch (currentElement.style.LayoutType) {
                    default:
                    case LayoutType.Unset:
                    case LayoutType.Flex:
                        currentElement.layoutBox = new AwesomeFlexLayoutBox();
                        break;
                    case LayoutType.Grid:
                        currentElement.layoutBox = new AwesomeGridLayoutBox();
                        break;
                    case LayoutType.Radial:
                        throw new NotImplementedException();
                    case LayoutType.Stack:
                        currentElement.layoutBox = new AwesomeStackLayoutBox();
                        break;
                }
            }

            if (currentElement.style.ScrollBehaviorX == ScrollBehavior.Fixed) {
                horizontalScrollFixed.Add(currentElement);
            } 
            if (currentElement.style.ScrollBehaviorY == ScrollBehavior.Fixed) {
                verticalScrollFixed.Add(currentElement);
            }
            
            currentElement.layoutBox.Initialize(currentElement, application.frameId);
        }

        internal void ChangeLayoutBox(UIElement currentElement, LayoutType layoutType) {
            Assert.IsNotNull(currentElement.layoutBox);

            if (currentElement is UITextElement || currentElement is ScrollView || currentElement is UIImageElement) {
                return;
            }

            if (currentElement.style.LayoutType == layoutType) {
                return;
            }

            // todo -- pool layoutbox

            switch (layoutType) {
                default:
                case LayoutType.Unset:
                case LayoutType.Flex:
                    if (currentElement.layoutBox is AwesomeFlexLayoutBox) {
                        return;
                    }

                    currentElement.layoutBox.Destroy();
                    currentElement.layoutBox = new AwesomeFlexLayoutBox();
                    break;
                case LayoutType.Grid:
                    if (currentElement.layoutBox is AwesomeGridLayoutBox) {
                        return;
                    }

                    currentElement.layoutBox.Destroy();
                    currentElement.layoutBox = new AwesomeGridLayoutBox();
                    break;
                case LayoutType.Radial:
                    throw new NotImplementedException();
                case LayoutType.Stack:
                    if (currentElement.layoutBox is AwesomeStackLayoutBox) {
                        return;
                    }

                    currentElement.layoutBox.Destroy();
                    currentElement.layoutBox = new AwesomeStackLayoutBox();
                    break;
            }

            currentElement.layoutBox.Initialize(currentElement, application.frameId);

            // parent will need to update children
            if (currentElement.parent?.layoutBox != null) {
                currentElement.parent.layoutBox.flags |= LayoutBoxFlags.GatherChildren;
            }
        }

        private void HandleStylePropertyChanged(UIElement element, StructList<StyleProperty> properties) {
            bool checkAlignHorizontal = false;
            bool updateAlignVertical = false;
            bool updateTransform = false;

            // if it was just enabled, let the system handle that as normal
            if (element.enableStateChangedFrameId == application.frameId) {
                return;
            }

            if (element.layoutBox == null) {
                // create box here
                CreateLayoutBox(element);
                return;
            }

            // assume box didn't change
            // if it did, it'll be updated later anyway

            for (int i = 0; i < properties.size; i++) {
                ref StyleProperty property = ref properties.array[i];
                // todo -- these flags can maybe probably be baked into setting the property
                switch (property.propertyId) {
                    case StylePropertyId.ClipBehavior:

                        element.layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                        element.layoutBox.clipBehavior = property.AsClipBehavior;

                        break;

                    case StylePropertyId.ClipBounds:
                        element.layoutBox.clipBounds = property.AsClipBounds;

                        break;
                    case StylePropertyId.OverflowX:
                    case StylePropertyId.OverflowY:
                        element.layoutBox.UpdateClipper();
                        break;

                    case StylePropertyId.LayoutType:
                        ChangeLayoutBox(element, property.AsLayoutType);
                        break;

                    case StylePropertyId.LayoutBehavior:
                        element.flags |= UIElementFlags.LayoutTypeOrBehaviorDirty;
                        break;
                    case StylePropertyId.TransformRotation: {
                        element.layoutBox.transformRotation = property.AsFloat;

                        updateTransform = true;
                        break;
                    }
                    case StylePropertyId.TransformPositionX: {
                        element.layoutBox.transformPositionX = property.AsOffsetMeasurement;

                        updateTransform = true;
                        break;
                    }
                    case StylePropertyId.TransformPositionY: {
                        element.layoutBox.transformPositionY = property.AsOffsetMeasurement;

                        updateTransform = true;
                        break;
                    }
                    case StylePropertyId.TransformScaleX: {
                        element.layoutBox.transformScaleX = property.AsFloat;

                        updateTransform = true;
                        break;
                    }
                    case StylePropertyId.TransformScaleY: {
                        element.layoutBox.transformScaleY = property.AsFloat;

                        updateTransform = true;
                        break;
                    }
                    case StylePropertyId.TransformPivotX: {
                        element.layoutBox.transformPivotX = property.AsUIFixedLength;

                        updateTransform = true;
                        break;
                    }
                    case StylePropertyId.TransformPivotY:
                        element.layoutBox.transformPivotY = property.AsUIFixedLength;

                        updateTransform = true;
                        break;

                    case StylePropertyId.AlignmentTargetX:
                    case StylePropertyId.AlignmentOriginX:
                    case StylePropertyId.AlignmentOffsetX:
                    case StylePropertyId.AlignmentDirectionX:
                    case StylePropertyId.AlignmentBoundaryX:
                        checkAlignHorizontal = true;
                        break;
                    case StylePropertyId.AlignmentTargetY:
                    case StylePropertyId.AlignmentOriginY:
                    case StylePropertyId.AlignmentOffsetY:
                    case StylePropertyId.AlignmentDirectionY:
                    case StylePropertyId.AlignmentBoundaryY:
                        updateAlignVertical = true;
                        break;
                    case StylePropertyId.MinWidth:
                    case StylePropertyId.MaxWidth:
                    case StylePropertyId.PreferredWidth:
                        element.layoutBox.UpdateBlockProviderWidth();
                        element.layoutBox.MarkForLayoutHorizontal();
                        break;
                    case StylePropertyId.MinHeight:
                    case StylePropertyId.MaxHeight:
                    case StylePropertyId.PreferredHeight:
                        element.layoutBox.UpdateBlockProviderHeight();
                        element.layoutBox.MarkForLayoutVertical();
                        break;
                    case StylePropertyId.PaddingLeft:
                    case StylePropertyId.PaddingRight:
                    case StylePropertyId.BorderLeft:
                    case StylePropertyId.BorderRight:
                        element.layoutBox.flags |= LayoutBoxFlags.ContentAreaWidthChanged;
                        break;
                    case StylePropertyId.PaddingTop:
                    case StylePropertyId.PaddingBottom:
                    case StylePropertyId.BorderTop:
                    case StylePropertyId.BorderBottom:
                        if (element.layoutBox != null) {
                            element.layoutBox.flags |= LayoutBoxFlags.ContentAreaHeightChanged;
                        }

                        break;
                    case StylePropertyId.LayoutFitHorizontal:
                        element.flags |= UIElementFlags.LayoutFitWidthDirty;
                        break;
                    case StylePropertyId.LayoutFitVertical:
                        element.flags |= UIElementFlags.LayoutFitHeightDirty;
                        break;
                    case StylePropertyId.ZIndex:
                        element.layoutBox.zIndex = property.AsInt;
                        break;
                }
            }

            if (updateTransform) {
                float rotation = element.style.TransformRotation;
                float scaleX = element.style.TransformScaleX;
                float scaleY = element.style.TransformScaleY;
                float positionX = element.style.TransformPositionX.value;
                float positionY = element.style.TransformPositionY.value;

                if (rotation != 0 || scaleX != 1 || scaleY != 1 || positionX != 0 || positionY != 0) {
                    element.flags |= UIElementFlags.LayoutTransformNotIdentity;
                }
                else {
                    element.flags &= ~UIElementFlags.LayoutTransformNotIdentity;
                }

                element.layoutBox.flags |= LayoutBoxFlags.TransformDirty;
            }

            AwesomeLayoutBox layoutBox = element.layoutBox;
            if (checkAlignHorizontal) {
                layoutBox.UpdateRequiresHorizontalAlignment();
            }

            if (updateAlignVertical) {
                layoutBox.UpdateRequiresVerticalAlignment();
            }

            layoutBox.OnStyleChanged(properties);
            // don't need to null check since root box will never have a style changed
            layoutBox.OnChildStyleChanged(element.layoutBox, properties);
        }

        public void OnReset() { }

        public void OnUpdate() {
            traversalIndex = 0;
            for (int i = 0; i < runners.size; i++) {
                runners[i].RunLayout();
            } 
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            runners.Add(new AwesomeLayoutRunner(this, view.dummyRoot));
        }

        public void OnViewRemoved(UIView view) {
            for (int i = 0; i < runners.size; i++) {
                if (runners[i].rootElement == view.dummyRoot) {
                    runners.RemoveAt(i);
                    return;
                }
            }
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) {
            // disable / destroy layout box?
        }

        public void OnElementDestroyed(UIElement element) {
            if (element.layoutBox != null) {
                element.layoutBox.Destroy();
                element.layoutBox = null;
            }
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void OnElementCreated(UIElement element) { }

        public IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn) {
            for (int i = 0; i < runners.size; i++) {
                runners[i].QueryPoint(point, retn);
            }

            return retn;
        }

        public AwesomeLayoutRunner GetLayoutRunner(UIElement viewRoot) {
            for (int i = 0; i < runners.size; i++) {
                if (runners.array[i].rootElement == viewRoot) {
                    return runners.array[i];
                }
            }

            return null;
        }

    }

}