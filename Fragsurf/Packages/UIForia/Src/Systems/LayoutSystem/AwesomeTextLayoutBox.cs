using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class AwesomeTextLayoutBox : AwesomeLayoutBox {

        private TextInfo textInfo;
        public Action onTextContentChanged;
        private bool textAlreadyDirty;
        private bool ignoreUpdate;

        protected override void OnInitialize() {
            onTextContentChanged = onTextContentChanged ?? HandleTextContentChanged;
            textInfo = ((UITextElement) element).textInfo;
            textInfo.onTextLayoutRequired += onTextContentChanged;
            textAlreadyDirty = true;
            flags |= (LayoutBoxFlags.RequireLayoutHorizontal | LayoutBoxFlags.RequireLayoutHorizontal);
        }

        private void HandleTextContentChanged() {
            if (ignoreUpdate) return;
            flags |= (LayoutBoxFlags.RequireLayoutHorizontal | LayoutBoxFlags.RequireLayoutHorizontal);
            finalWidth = -1;
            finalHeight = -1;
            cachedContentWidth = -1;
            cachedContentHeight = -1;
            if (textAlreadyDirty) return;
            textAlreadyDirty = true;
            AwesomeLayoutBox ptr = parent;

            while (ptr != null) {
                // once we hit a block provider we can safely stop traversing since the provider's parent doesn't care about content size changing
                bool stop = (ptr.flags & LayoutBoxFlags.WidthBlockProvider) != 0;
                // can't break out if already flagged for layout because parent of parent might not be and might be content sized
                ptr.flags |= LayoutBoxFlags.RequireLayoutHorizontal;
                ptr.cachedContentWidth = -1;
//                ptr.element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, -1, LayoutReason.DescendentStyleSizeChanged);
                if (stop) break;
                ptr = ptr.parent;
            }

            ptr = parent;

            while (ptr != null) {
                // once we hit a block provider we can safely stop traversing since the provider's parent doesn't care about content size changing
                bool stop = (ptr.flags & LayoutBoxFlags.HeightBlockProvider) != 0;

                // can't break out if already flagged for layout because parent of parent might not be and might be content sized
                ptr.flags |= LayoutBoxFlags.RequireLayoutVertical;
                ptr.cachedContentHeight = -1;
//                ptr.element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, -1, LayoutReason.DescendentStyleSizeChanged);
                if (stop) break;
                ptr = ptr.parent;
            }
        }

        protected override void OnDestroy() {
            textInfo.onTextLayoutRequired -= onTextContentChanged;
        }

        protected override float ComputeContentWidth() {
            ignoreUpdate = true;
            // by definition when computing width in the width pass we only care about its natural width
            float retn = textInfo.ComputeContentWidth(float.MaxValue);
            ignoreUpdate = false;
            return retn;
        }

        protected override float ComputeContentHeight() {
            ignoreUpdate = true;
            
            // todo -- might need to subtract padding / border from this value
            float retn = textInfo.ComputeHeightForWidth(finalWidth);
            ignoreUpdate = false;

            return retn;
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) { }

        public override void RunLayoutHorizontal(int frameId) { }

        public override void RunLayoutVertical(int frameId) {
            textAlreadyDirty = false;
            textInfo.ForceLayout(); // might not need this
            float topOffset = paddingBorderVerticalStart;
            float leftOffset = paddingBorderHorizontalStart;
            textInfo.Layout(new Vector2(leftOffset, topOffset), finalWidth - paddingBorderHorizontalStart - paddingBorderHorizontalEnd);
        }

    }

}