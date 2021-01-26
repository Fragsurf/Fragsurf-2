using UIForia.Attributes;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEngine;

namespace UIForia.Elements {

    [Template(TemplateType.Internal, "Elements/ScrollView.xml")]
    public class ScrollView : UIElement {

        public float fadeTarget;
        public float fadeTime = 2f;

        public float scrollSpeedY = 48f;
        public float scrollSpeedX = 16f;
        public float trackSize = 10f;

        public bool disableOverflowX;
        public bool disableOverflowY;

        public bool disableAutoScroll = false;

        public bool verticalScrollingEnabled => !disableOverflowY && isOverflowingY;
        public bool horizontalScrollingEnabled => !disableOverflowX && isOverflowingX;

        private Size previousChildrenSize;

        public float scrollPercentageX;
        public float scrollPercentageY;

        public bool isOverflowingX { get; internal set; }
        public bool isOverflowingY { get; internal set; }

        internal float scrollDeltaX;
        internal float scrollDeltaY;
        internal int xDirection;
        internal int yDirection;

        internal float scrollPixelAmountX;
        internal float scrollPixelAmountY;

        internal UIElement verticalHandle;
        internal UIElement horizontalHandle;
        
        private float elapsedTotalTime;

        private float fromScrollY;
        private float toScrollY;
        private bool isScrollingY;

        private float fromScrollX;
        private float toScrollX;
        private bool isScrollingX;

        private float accumulatedScrollSpeedY;
        private float accumulatedScrollSpeedX;

        public override void OnEnable() {
            verticalHandle = children[2];
            horizontalHandle = children[4];
        }

        public override void OnUpdate() {
            scrollPixelAmountX = 0;
            scrollPixelAmountY = 0;
            
            if (isScrollingY) {
                elapsedTotalTime += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(Easing.Interpolate(elapsedTotalTime / 0.500f, EasingFunction.CubicEaseOut));
                scrollPercentageY = Mathf.Lerp( fromScrollY, toScrollY, t);
                isScrollingY = t < 1;
            }
            else if (isScrollingX) {
                elapsedTotalTime += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(Easing.Interpolate(elapsedTotalTime / 0.500f, EasingFunction.CubicEaseOut));
                scrollPercentageX = Mathf.Lerp( fromScrollX, toScrollX, t);
                isScrollingX = t < 1;
            }

            if (!children[0].isEnabled) {
                isOverflowingX = false;
                isOverflowingY = false;
            }
            else {
                Size currentChildrenSize = new Size(children[0].layoutResult.actualSize.width, children[0].layoutResult.allocatedSize.height);

                isOverflowingX = currentChildrenSize.width > layoutResult.actualSize.width;
                isOverflowingY = currentChildrenSize.height > layoutResult.actualSize.height;

                if (!disableAutoScroll && currentChildrenSize != previousChildrenSize) {
                    ScrollToHorizontalPercent(0);
                    ScrollToVerticalPercent(0);
                }

                previousChildrenSize = currentChildrenSize;
            }
        }

        public override void OnDisable() {
            scrollDeltaX = 0;
            scrollDeltaY = 0;
        }

        [OnMouseWheel]
        public void OnMouseWheel(MouseInputEvent evt) {
            if (verticalScrollingEnabled) {
                float actualContentHeight = children[0].layoutResult.actualSize.height;
                float visibleContentHeight = layoutResult.ActualHeight;
                if (!isScrollingY || (int) Mathf.Sign(evt.ScrollDelta.y) != (int) Mathf.Sign(fromScrollY - toScrollY)) {
                    accumulatedScrollSpeedY = scrollSpeedY;
                }
                else {
                    accumulatedScrollSpeedY *= 1.2f;
                }
                scrollDeltaY = -evt.ScrollDelta.y * accumulatedScrollSpeedY / (actualContentHeight - visibleContentHeight);
                fromScrollY = scrollPercentageY;
                if (scrollDeltaY != 0) {
                    toScrollY = Mathf.Clamp01(fromScrollY + scrollDeltaY);
                    if (fromScrollY != toScrollY) {
                        evt.StopPropagation();
                        elapsedTotalTime = 0;
                        isScrollingY = true;
                    }
                }
            }

            if (horizontalScrollingEnabled) {
                float actualContentWidth = children[0].layoutResult.actualSize.width;
                float visibleContentWidth = layoutResult.ActualWidth;
                if (!isScrollingX || (int) Mathf.Sign(-evt.ScrollDelta.x) != (int) Mathf.Sign(fromScrollX - toScrollX)) {
                    accumulatedScrollSpeedX = scrollSpeedX;
                }
                else {
                    accumulatedScrollSpeedX *= 1.2f;
                }
                scrollDeltaX = evt.ScrollDelta.x * accumulatedScrollSpeedX / (actualContentWidth - visibleContentWidth);
                if (scrollDeltaX != 0) {
                    fromScrollX = scrollPercentageX;
                    toScrollX = Mathf.Clamp01(fromScrollX + scrollDeltaX);
                    if (fromScrollX != toScrollX) {
                        evt.StopPropagation();
                        elapsedTotalTime = 0;
                        isScrollingX = true;
                    }
                }
            }
        }

        public void OnClickVertical(MouseInputEvent evt) {

            float contentAreaHeight = layoutResult.ContentAreaHeight;
            float contentHeight = children[0].layoutResult.actualSize.height;
            float paddingBorderStart = layoutResult.VerticalPaddingBorderStart;
            float y = evt.MousePosition.y - layoutResult.screenPosition.y - paddingBorderStart;
            
            if (contentHeight == 0) return;

            float handleHeight = (contentAreaHeight / contentHeight) * contentAreaHeight;

            float handlePosition = (paddingBorderStart + contentAreaHeight - handleHeight) * scrollPercentageY;

            float pageSize = evt.element.layoutResult.allocatedSize.height / contentHeight;

            if (y < handlePosition) {
                pageSize = -pageSize;
            }

            ScrollToVerticalPercent(scrollPercentageY + pageSize);

            evt.StopPropagation();
        }

        public void OnClickHorizontal(MouseInputEvent evt) {
            float x = evt.MousePosition.x - layoutResult.screenPosition.x;

            float contentAreaWidth = layoutResult.ContentAreaWidth;
            float contentWidth = children[0].layoutResult.actualSize.width;

            if (contentWidth == 0) return;

            float handleWidth = (contentAreaWidth / contentWidth) * contentAreaWidth;

            float handlePosition = (contentAreaWidth - handleWidth) * scrollPercentageX;

            float pageSize = evt.element.layoutResult.allocatedSize.width / contentWidth;

            if (x < handlePosition) {
                pageSize = -pageSize;
            }

            ScrollToHorizontalPercent(scrollPercentageX + pageSize);

            evt.StopPropagation();
        }

        [OnDragCreate(EventPhase.Capture)]
        public DragEvent OnMiddleMouseDrag(MouseInputEvent evt) {
            if (!evt.IsMouseMiddleDown) {
                return null;
            }

            Vector2 baseOffset = new Vector2();
            ScrollbarOrientation orientation = 0;

            if (horizontalScrollingEnabled) {
                baseOffset.x = evt.MousePosition.x - horizontalHandle.layoutResult.screenPosition.x;
                orientation |= ScrollbarOrientation.Horizontal;
            }

            if (verticalScrollingEnabled) {
                baseOffset.y = evt.MousePosition.y - verticalHandle.layoutResult.screenPosition.y;
                orientation |= ScrollbarOrientation.Vertical;
            }

            return new ScrollbarDragEvent(orientation, baseOffset, this);
        }

        public virtual DragEvent OnCreateVerticalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            float baseOffset = evt.MousePosition.y - (evt.element.layoutResult.screenPosition.y);
            return new ScrollbarDragEvent(ScrollbarOrientation.Vertical, new Vector2(0, baseOffset), this);
        }

        public virtual DragEvent OnCreateHorizontalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            float baseOffset = evt.MousePosition.x - evt.element.layoutResult.screenPosition.x;
            return new ScrollbarDragEvent(ScrollbarOrientation.Horizontal, new Vector2(baseOffset, 0), this);
        }

        public void ScrollToVerticalPercent(float percentage) {
            scrollDeltaY = 0;
            scrollPercentageY = Mathf.Clamp01(percentage);
        }

        public void ScrollToHorizontalPercent(float percentage) {
            scrollDeltaX = 0;
            scrollPercentageX = Mathf.Clamp01(percentage);
        }

        public class ScrollbarDragEvent : DragEvent {

            public readonly Vector2 baseOffset;
            public readonly ScrollView scrollView;
            public readonly ScrollbarOrientation orientation;

            public ScrollbarDragEvent(ScrollbarOrientation orientation, Vector2 baseOffset, ScrollView scrollView) {
                this.orientation = orientation;
                this.baseOffset = baseOffset;
                this.scrollView = scrollView;
            }

            public override void Update() {
                if ((orientation & ScrollbarOrientation.Vertical) != 0) {
                    float height = scrollView.layoutResult.ContentAreaHeight;
    
                    height -= scrollView.verticalHandle.layoutResult.actualSize.height;

                    float y = Mathf.Clamp(MousePosition.y - (scrollView.layoutResult.screenPosition.y + scrollView.layoutResult.VerticalPaddingBorderStart) - baseOffset.y, 0, height);

                    if (height == 0) {
                        scrollView.ScrollToVerticalPercent(0);
                    }
                    else {
                        scrollView.ScrollToVerticalPercent(y / height);
                    }
                }

                if ((orientation & ScrollbarOrientation.Horizontal) != 0) {
                    float width = scrollView.layoutResult.ContentAreaWidth;

                    width -= scrollView.horizontalHandle.layoutResult.actualSize.width;

                    float x = Mathf.Clamp(MousePosition.x - (scrollView.layoutResult.screenPosition.x  + scrollView.layoutResult.HorizontalPaddingBorderStart) - baseOffset.x, 0, width);

                    if (width == 0) {
                        scrollView.ScrollToHorizontalPercent(0);
                    }
                    else {
                        scrollView.ScrollToHorizontalPercent(x / width);
                    }
                }
            }

        }

        public float ScrollOffsetX => -(children[0].layoutResult.alignedPosition.x - layoutResult.HorizontalPaddingBorderStart);
        public float ScrollOffsetY => -(children[0].layoutResult.alignedPosition.y - layoutResult.VerticalPaddingBorderStart);

        internal void ScrollElementIntoView(UIElement element, float crawlPositionX, float crawlPositionY) {
            
            float scrollOffsetX = ScrollOffsetX;
            float localPositionX = crawlPositionX - layoutResult.HorizontalPaddingBorderStart;

            float elementWidth = element.layoutResult.ActualWidth;
            float elementRight = localPositionX + scrollOffsetX + elementWidth;

            float childrenWidth = children[0].layoutResult.ActualWidth;
            float contentWidth = layoutResult.ContentAreaWidth;

            if (localPositionX < 0) {
                // scrolls to the left edge of the element
                ScrollToHorizontalPercent((localPositionX + scrollOffsetX) / (childrenWidth - contentWidth));
            } else if (elementRight - scrollOffsetX > contentWidth) {
                // scrolls to the right edge but keeps the element at the right edge of the scrollView
                ScrollToHorizontalPercent(((elementRight - contentWidth) / (childrenWidth - contentWidth)));
            }
            
            float scrollOffsetY = ScrollOffsetY;
            float localPositionY = crawlPositionY - layoutResult.VerticalPaddingBorderStart;

            float elementHeight = element.layoutResult.ActualHeight;
            float elementBottom = localPositionY + scrollOffsetY + elementHeight;

            float childrenHeight = children[0].layoutResult.ActualHeight;
            float contentHeight = layoutResult.ContentAreaHeight;

            if (localPositionY < 0) {
                // scrolls up to the upper edge of the element
                ScrollToVerticalPercent((localPositionY + scrollOffsetY) / (childrenHeight - contentHeight));
            } else if (elementBottom - scrollOffsetY > contentHeight) {
                // scrolls down but keeps the element at the lower edge of the scrollView
                ScrollToVerticalPercent(((elementBottom - contentHeight) / (childrenHeight - contentHeight)));
            }
        }

    }

}