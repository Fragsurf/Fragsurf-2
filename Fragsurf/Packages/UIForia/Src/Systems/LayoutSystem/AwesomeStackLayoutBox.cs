using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    // stack ignores extra space distribution since we never have any, items are always allocated the entire space so 
    // that fit properties will fill the whole layout box
    // aligning items works though
    public class AwesomeStackLayoutBox : AwesomeLayoutBox {

        protected override float ComputeContentWidth() {
            AwesomeLayoutBox ptr = firstChild;
            float retn = 0f;
            
            while (ptr != null) {
                LayoutSize size = default;
                ptr.GetWidths(ref size);
                // todo clamp to min/max?
                float clampedWidth = size.Clamped + size.marginStart + size.marginEnd;
                if (clampedWidth > retn) retn = clampedWidth;
                ptr = ptr.nextSibling;
            }

            return retn;
        }

        protected override float ComputeContentHeight() {
            AwesomeLayoutBox ptr = firstChild;
            float retn = 0f;
            while (ptr != null) {
                LayoutSize size = default;
                ptr.GetHeights(ref size);
                float clampedHeight = size.Clamped + size.marginStart + size.marginEnd;
                if (clampedHeight > retn) retn = clampedHeight;
                ptr = ptr.nextSibling;
            }

            return retn;
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) { }

        public override void OnStyleChanged(StructList<StyleProperty> propertyList) {
            for (int i = 0; i < propertyList.size; i++) {
                // note: space distribution is ignored, we don't care if it changes
                switch (propertyList.array[i].propertyId) {
                    case StylePropertyId.AlignItemsHorizontal:
                    case StylePropertyId.FitItemsHorizontal:
                        flags |= LayoutBoxFlags.RequireLayoutHorizontal;
                        // todo -- log history entry
                        break;
                    case StylePropertyId.AlignItemsVertical:
                    case StylePropertyId.FitItemsVertical:
                        flags |= LayoutBoxFlags.RequireLayoutVertical;
                        // todo -- log history entry
                        break;
                }
            }
        }

        public override void RunLayoutHorizontal(int frameId) {
            AwesomeLayoutBox ptr = firstChild;

            float contentAreaWidth = finalWidth - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);

            float alignment = element.style.AlignItemsHorizontal;

            // SpaceDistribution extraSpace = element.style.DistributeExtraSpaceHorizontal;
            
            float inset = paddingBorderHorizontalStart;

            while (ptr != null) {
                LayoutSize size = default;
                ptr.GetWidths(ref size);
                float clampedWidth = size.Clamped;

                float x = inset + size.marginStart;
                float originBase = x;
                float originOffset = contentAreaWidth * alignment;
                float alignedPosition = originBase + originOffset + (clampedWidth * -alignment);
                ptr.ApplyLayoutHorizontal(x, alignedPosition, size, clampedWidth, contentAreaWidth, LayoutFit.None, frameId);
                ptr = ptr.nextSibling;
            }
        }

        public override void RunLayoutVertical(int frameId) {
            AwesomeLayoutBox ptr = firstChild;

            float contentAreaHeight = finalHeight - (paddingBorderVerticalStart + paddingBorderVerticalEnd);

            float alignment = element.style.AlignItemsVertical;
            float inset = paddingBorderVerticalStart;

            while (ptr != null) {
                LayoutSize size = default;
                ptr.GetHeights(ref size);
                float clampedHeight = size.Clamped;

                float y = inset + size.marginStart;
                float originBase = y;
                float originOffset = contentAreaHeight * alignment;
                float alignedPosition = originBase + originOffset + (clampedHeight * -alignment);
                ptr.ApplyLayoutVertical(y, alignedPosition, size, clampedHeight, contentAreaHeight, LayoutFit.None, frameId);
                ptr = ptr.nextSibling;
            }
        }

    }

}