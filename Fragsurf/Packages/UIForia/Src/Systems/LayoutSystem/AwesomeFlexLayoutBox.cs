using System.Diagnostics;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {
    [DebuggerDisplay("{element.ToString()} | Flex")]
    public class AwesomeFlexLayoutBox : AwesomeLayoutBox {
        private StructList<FlexItem> items;
        private LayoutDirection direction;
        private StructList<Track> wrappedTracks;

        protected override void OnInitialize() {
            direction = element.style.FlexLayoutDirection;
        }

        protected override float ComputeContentWidth() {
            if (childCount == 0) return 0;
            return direction == LayoutDirection.Horizontal
                ? ComputeContentWidthHorizontal()
                : ComputeContentWidthVertical();
        }

        protected override float ComputeContentHeight() {
            if (childCount == 0) return 0;
            return direction == LayoutDirection.Horizontal
                ? ComputeContentHeightHorizontal()
                : ComputeContentHeightVertical();
        }

        private float ComputeContentWidthHorizontal() {
            float totalSize = 0;

            for (int i = 0; i < items.size; i++) {
                LayoutSize widths = default;
                items.array[i].layoutBox.GetWidths(ref widths);
                float baseSize = Mathf.Max(widths.minimum, Mathf.Min(widths.preferred, widths.maximum));
                totalSize += baseSize + widths.marginStart + widths.marginEnd;
            }
            
            float gap = element.style.FlexLayoutGapHorizontal;
            if (element.style.DistributeExtraSpaceHorizontal == SpaceDistribution.AroundContent) {
                gap = 0;
            }

            totalSize += gap * (items.size - 1);

            return totalSize;
        }

        private float ComputeContentWidthVertical() {
            float maxSize = 0;

            for (int i = 0; i < items.size; i++) {
                LayoutSize widths = default;
                items.array[i].layoutBox.GetWidths(ref widths);
                float baseSize = Mathf.Max(widths.minimum, Mathf.Min(widths.preferred, widths.maximum));
                float totalSize = baseSize + widths.marginStart + widths.marginEnd;
                if (totalSize > maxSize) {
                    maxSize = totalSize;
                }
            }

            return maxSize;
        }

        private float ComputeContentHeightHorizontal() {
            float maxSize = 0;

            if (wrappedTracks != null && wrappedTracks.size != 0) {
                float retn = 0;
                for (int i = 0; i < wrappedTracks.size; i++) {
                    ref Track track = ref wrappedTracks.array[i];

                    float maxHeight = 0;

                    for (int j = track.startIndex; j < track.endIndex; j++) {
                        ref FlexItem item = ref items.array[j];
                        item.layoutBox.GetHeights(ref item.heightData);
                        item.baseHeight = item.heightData.Clamped;

                        if (item.baseHeight + item.heightData.marginStart + item.heightData.marginEnd > maxHeight) {
                            maxHeight = item.baseHeight + item.heightData.marginStart + item.heightData.marginEnd;
                        }
                    }

                    retn += maxHeight;
                }

                return retn;
            }

            for (int i = 0; i < items.size; i++) {
                LayoutSize heights = default;
                items.array[i].layoutBox.GetHeights(ref heights);
                float baseSize = Mathf.Max(heights.minimum, Mathf.Min(heights.preferred, heights.maximum));
                float totalSize = baseSize + heights.marginStart + heights.marginEnd;
                if (totalSize > maxSize) {
                    maxSize = totalSize;
                }
            }

            return maxSize;
        }

        private float ComputeContentHeightVertical() {
            float totalSize = 0;

            for (int i = 0; i < items.size; i++) {
                LayoutSize heights = default;
                items.array[i].layoutBox.GetHeights(ref heights);
                float baseSize = Mathf.Max(heights.minimum, Mathf.Min(heights.preferred, heights.maximum));
                totalSize += baseSize + heights.marginStart + heights.marginEnd;
            }
            
            float gap = element.style.FlexLayoutGapHorizontal;
            if (element.style.DistributeExtraSpaceHorizontal == SpaceDistribution.AroundContent) {
                gap = 0;
            }

            totalSize += gap * (items.size - 1);

            return totalSize;
        }

        public override void OnStyleChanged(StructList<StyleProperty> propertyList) {
            // todo -- a lot of these won't require a full layout, optimize this later to just do alignment / etc
            for (int i = 0; i < propertyList.size; i++) {
                ref StyleProperty property = ref propertyList.array[i];
                switch (property.propertyId) {
                    case StylePropertyId.FlexLayoutWrap:
                    case StylePropertyId.FlexLayoutDirection:
                        flags |= (LayoutBoxFlags.RequireLayoutHorizontal | LayoutBoxFlags.RequireLayoutVertical);
                        // todo - notify parent of layout
                        break;
                    case StylePropertyId.DistributeExtraSpaceHorizontal:
                        flags |= LayoutBoxFlags.RequireLayoutHorizontal;
                        break;

                    case StylePropertyId.DistributeExtraSpaceVertical:
                        flags |= LayoutBoxFlags.RequireLayoutVertical;
                        break;

                    case StylePropertyId.AlignItemsHorizontal:
                        flags |= LayoutBoxFlags.RequireLayoutHorizontal;
                        break;
                    case StylePropertyId.AlignItemsVertical:
                    case StylePropertyId.FitItemsHorizontal:
                        flags |= LayoutBoxFlags.RequireLayoutHorizontal;
                        break;
                    case StylePropertyId.FitItemsVertical:
                        flags |= LayoutBoxFlags.RequireLayoutVertical;
                        break;
                }
            }
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) {
            items?.Clear();

            if (childList.size == 0) {
                return;
            }

            items = items ?? new StructList<FlexItem>(childCount);
            items.EnsureCapacity(childCount);
            items.size = childCount;
            for (int i = 0; i < childList.size; i++) {
                items.array[i] = new FlexItem() {
                    layoutBox = childList.array[i],
                    growPieces = childList.array[i].element.style.FlexItemGrow,
                    shrinkPieces = childList.array[i].element.style.FlexItemShrink
                };
            }
        }

        public override void RunLayoutHorizontal(int frameId) {
            if (childCount == 0) return;
            if (direction == LayoutDirection.Horizontal) {
                if (element.style.FlexLayoutWrap == LayoutWrap.WrapHorizontal) {
                    RunLayoutHorizontalStep_HorizontalDirection_Wrapped(frameId);
                }
                else {
                    RunLayoutHorizontalStep_HorizontalDirection(frameId);
                }
            }
            else {
                RunLayoutHorizontalStep_VerticalDirection(frameId);
            }
        }

        private void RunLayoutHorizontalStep_HorizontalDirection_Wrapped(int frameId) {
            float contentAreaWidth = finalWidth - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);
            float gap = element.style.FlexLayoutGapHorizontal;
            if (element.style.DistributeExtraSpaceHorizontal == SpaceDistribution.AroundContent) {
                gap = 0;
            }
            
            Track currentTrack = new Track(contentAreaWidth, 0);
            wrappedTracks = wrappedTracks ?? new StructList<Track>(4);
            wrappedTracks.QuickClear();

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.grew = 0;
                item.shrunk = 0;
                item.layoutBox.GetWidths(ref item.widthData);
                item.baseWidth = item.widthData.Clamped;
                item.availableSize = item.baseWidth;
                // available size is equal to base size but we need to take up base size + margin in the layout
                float itemSize = item.baseWidth + item.widthData.marginStart + item.widthData.marginEnd;

                // if item is bigger than content area, stop current track if not empty
                if (itemSize >= contentAreaWidth) {
                    // stop current track
                    if (!currentTrack.IsEmpty) {
                        currentTrack.endIndex = i;
                        wrappedTracks.Add(currentTrack);
                        currentTrack = new Track(contentAreaWidth, i);
                        currentTrack.remaining -= itemSize;
                        currentTrack.endIndex = i + 1;
                        wrappedTracks.Add(currentTrack);
                        currentTrack = new Track(contentAreaWidth, i + 1);
                    }
                    else {
                        currentTrack.endIndex = i + 1;
                        currentTrack.remaining -= itemSize;
                        wrappedTracks.Add(currentTrack);
                        currentTrack = new Track(contentAreaWidth, i + 1);
                    }
                }
                else {
                    if (currentTrack.IsEmpty) {
                        if (currentTrack.remaining - itemSize == 0) {
                            currentTrack.remaining -= itemSize;
                            currentTrack.endIndex = i + 1;
                            wrappedTracks.Add(currentTrack);
                            currentTrack = new Track(contentAreaWidth, i + 1);
                        } else if (currentTrack.remaining - itemSize < 0) {
                            currentTrack.endIndex = i;
                            wrappedTracks.Add(currentTrack);
                            currentTrack = new Track(contentAreaWidth, i);
                            currentTrack.remaining -= itemSize;
                            currentTrack.endIndex = i + 1;
                        } else {
                            currentTrack.remaining -= itemSize;
                            currentTrack.endIndex = i + 1;
                        }
                    } else {
                        if (currentTrack.remaining - (gap + itemSize) == 0) {
                            currentTrack.remaining -= gap + itemSize;
                            currentTrack.endIndex = i + 1;
                            wrappedTracks.Add(currentTrack);
                            currentTrack = new Track(contentAreaWidth, i + 1);
                        } else if (currentTrack.remaining - (gap + itemSize) < 0) {
                            currentTrack.endIndex = i;
                            wrappedTracks.Add(currentTrack);
                            currentTrack = new Track(contentAreaWidth, i);
                            currentTrack.remaining -= itemSize;
                            currentTrack.endIndex = i + 1;
                        } else {
                            currentTrack.remaining -= (gap + itemSize);
                            currentTrack.endIndex = i + 1;
                        }
                    }
                }
            }

            if (!currentTrack.IsEmpty) {
                wrappedTracks.Add(currentTrack);
            }

            float itemAlignment = element.style.AlignItemsHorizontal;
            
            SpaceDistribution alignment = element.style.DistributeExtraSpaceHorizontal;

            if (alignment == SpaceDistribution.Default) {
                alignment = SpaceDistribution.AfterContent;
            }

            for (int trackIndex = 0; trackIndex < wrappedTracks.size; trackIndex++) {
                ref Track track = ref wrappedTracks.array[trackIndex];

                if (track.remaining > 0) {
                    GrowHorizontal(ref track);
                }
                else if (track.remaining < 0) {
                    ShrinkHorizontal(ref track);
                }

                float offset = 0;
                float spacerSize = 0;

                SpaceDistributionUtil.GetAlignmentOffsets(track.remaining, track.endIndex - track.startIndex, alignment, out offset, out spacerSize);

                int startIdx = track.startIndex;
                int endIdx = track.endIndex;
                for (int i = startIdx; i < endIdx; i++) {
                    ref FlexItem item = ref items.array[i];
                    float x = paddingBorderHorizontalStart + offset + item.widthData.marginStart;
                    offset += item.availableSize + spacerSize;
                    LayoutFit fit = LayoutFit.None;

                    float elementWidth = item.baseWidth;
                    float originOffset = item.availableSize * itemAlignment;
                    float alignedPosition = x + originOffset + (elementWidth * -itemAlignment);

                    if (item.grew == 1) {
                        fit = LayoutFit.Grow;
                    }
                    else if (item.shrunk == 1) {
                        fit = LayoutFit.Shrink;
                    }

                    item.layoutBox.ApplyLayoutHorizontal(x, alignedPosition, item.widthData, item.baseWidth, item.availableSize, fit, frameId);
                    offset += item.widthData.marginStart + item.widthData.marginEnd + gap;
                }

                MarkForLayoutVertical(frameId);
            }
        }


        private void GrowHorizontal(ref Track track) {
            int pieces = 0;

            for (int i = track.startIndex; i < track.endIndex; i++) {
                pieces += items.array[i].growPieces;
                items.array[i].grew = 0;
            }

            bool allocate = pieces > 0;
            while (allocate && (int) track.remaining > 0) {
                allocate = false;

                float pieceSize = track.remaining / pieces;

                bool recomputePieces = false;
                for (int i = track.startIndex; i < track.endIndex; i++) {
                    ref FlexItem item = ref items.array[i];
                    float max = item.widthData.maximum;
                    float output = item.availableSize;
                    int growthFactor = item.growPieces;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    item.grew = 1;
                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    if (totalGrowth >= max) {
                        output = max;
                        recomputePieces = true;
                    }
                    else {
                        output = totalGrowth;
                    }

                    track.remaining -= output - start;
                    item.availableSize = output;
                }

                if (recomputePieces) {
                    pieces = 0;
                    for (int j = track.startIndex; j < track.endIndex; j++) {
                        if (items.array[j].availableSize != items.array[j].widthData.maximum) {
                            pieces += items.array[j].growPieces;
                        }
                    }

                    if (pieces == 0) {
                        return;
                    }
                }
            }
        }

        private float GrowVertical(float remaining) {
            int pieces = 0;

            for (int i = 0; i < items.size; i++) {
                pieces += items.array[i].growPieces;
            }

            bool allocate = pieces > 0;
            while (allocate && (int) remaining > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remaining / pieces;
                bool recomputePieces = false;

                for (int i = 0; i < items.size; i++) {
                    ref FlexItem item = ref items.array[i];
                    float max = item.heightData.maximum;
                    float output = item.availableSize;
                    int growthFactor = item.growPieces;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    item.grew = 1;
                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    if (totalGrowth >= max) {
                        output = max;
                        recomputePieces = true;
                    }
                    else {
                        output = totalGrowth;
                    }

                    remaining -= output - start;
                    item.availableSize = output;
                }

                if (recomputePieces) {
                    pieces = 0;
                    for (int j = 0; j < items.size; j++) {
                        if (items.array[j].availableSize != items.array[j].heightData.maximum) {
                            pieces += items.array[j].growPieces;
                        }
                    }

                    if (pieces == 0) {
                        return remaining;
                    }
                }
            }

            return remaining;
        }

        private void ShrinkHorizontal(ref Track track) {
            int startIndex = track.startIndex;
            int endIndex = track.endIndex;
            int pieces = 0;

            for (int i = startIndex; i < endIndex; i++) {
                pieces += items.array[i].shrinkPieces;
            }

            float overflow = -track.remaining;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;
                bool recomputePieces = false;

                for (int i = startIndex; i < endIndex; i++) {
                    ref FlexItem item = ref items.array[i];
                    float min = item.widthData.minimum;
                    float output = item.availableSize;
                    int shrinkFactor = item.shrinkPieces;

                    if (shrinkFactor == 0 || (int) output == (int) min || (int) output == 0) {
                        continue;
                    }

                    allocate = true;
                    item.shrunk = 1;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    if (totalShrink <= min) {
                        output = min;
                        recomputePieces = true;
                    }
                    else {
                        output = totalShrink;
                    }

                    overflow += output - start;
                    item.availableSize = output;
                }

                if (recomputePieces) {
                    pieces = 0;
                    for (int j = track.startIndex; j < track.endIndex; j++) {
                        ref FlexItem item = ref items.array[j];
                        if (item.availableSize != item.widthData.minimum) {
                            pieces += item.shrinkPieces;
                        }
                    }

                    if (pieces == 0) {
                        break;
                    }
                }
            }

            track.remaining = -overflow;
        }

        private float ShrinkVertical(float remaining) {
            int pieces = 0;

            for (int i = 0; i < items.size; i++) {
                pieces += items.array[i].shrinkPieces;
            }

            float overflow = -remaining;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;
                bool recomputePieces = false;

                for (int i = 0; i < items.size; i++) {
                    ref FlexItem item = ref items.array[i];
                    float min = item.heightData.minimum;
                    float output = item.availableSize;
                    int shrinkFactor = item.shrinkPieces;

                    if (shrinkFactor == 0 || (int) output == (int) min || (int) output == 0) {
                        continue;
                    }

                    allocate = true;
                    item.shrunk = 1;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    if (totalShrink <= min) {
                        output = min;
                        recomputePieces = true;
                    }
                    else {
                        output = totalShrink;
                    }

                    overflow += output - start;
                    item.availableSize = output;
                }

                if (recomputePieces) {
                    pieces = 0;
                    for (int j = 0; j < items.size; j++) {
                        ref FlexItem item = ref items.array[j];
                        if (item.availableSize != item.heightData.minimum) {
                            pieces += item.shrinkPieces;
                        }
                    }

                    if (pieces == 0) {
                        break;
                    }
                }
            }

            return -overflow;
        }

        protected override bool IsAutoWidthContentBased() {
            return direction != LayoutDirection.Vertical;
        }

        protected override float ResolveAutoWidth(AwesomeLayoutBox child, float factor) {
            if (direction == LayoutDirection.Vertical) {
                return child.ComputeBlockContentAreaWidth(factor);
            }

            return child.GetContentWidth(factor);
        }


        public override void RunLayoutVertical(int frameId) {
            if (childCount == 0) return;
            if (direction == LayoutDirection.Horizontal) {
                if (element.style.FlexLayoutWrap == LayoutWrap.WrapHorizontal) {
                    RunLayoutVerticalStep_HorizontalDirection_Wrapped(frameId);
                }
                else {
                    RunLayoutVerticalStep_HorizontalDirection(frameId);
                }
            }
            else {
                RunLayoutVerticalStep_VerticalDirection(frameId);
            }
        }

        private void RunLayoutHorizontalStep_HorizontalDirection(int frameId) {
            Track track = new Track();
            track.remaining = finalWidth - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);
            track.startIndex = 0;
            track.endIndex = childCount;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.grew = 0;
                item.shrunk = 0;
                item.layoutBox.GetWidths(ref item.widthData);
                item.baseWidth = item.widthData.Clamped;
                item.availableSize = item.baseWidth;
                // available size is equal to base size but we need to take up base size + margin in the layout
                track.remaining -= item.baseWidth + item.widthData.marginStart + item.widthData.marginEnd;
            }
            
            float gap = element.style.FlexLayoutGapHorizontal;
            if (element.style.DistributeExtraSpaceHorizontal == SpaceDistribution.AroundContent) {
                gap = 0;
            }

            track.remaining -= gap * (items.size - 1);

            if (track.remaining > 0) {
                GrowHorizontal(ref track);
            }
            else if (track.remaining < 0) {
                ShrinkHorizontal(ref track);
            }

            float offset = 0;
            float inset = paddingBorderHorizontalStart;
            float spacerSize = 0;
            SpaceDistribution alignment = element.style.DistributeExtraSpaceHorizontal;

            if (alignment == SpaceDistribution.Default) {
                alignment = SpaceDistribution.AfterContent;
            }

            SpaceDistributionUtil.GetAlignmentOffsets(track.remaining, track.endIndex - track.startIndex, alignment, out offset, out spacerSize);

            float itemAlignment = element.style.AlignItemsHorizontal;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                float x = inset + offset + item.widthData.marginStart;
                offset += item.availableSize + spacerSize;
                LayoutFit fit = LayoutFit.None;

                float elementWidth = item.baseWidth;
                float originOffset = item.availableSize * itemAlignment;
                float alignedPosition = x + originOffset + (elementWidth * -itemAlignment);

                if (item.grew == 1) {
                    fit = LayoutFit.Grow;
                }
                else if (item.shrunk == 1) {
                    fit = LayoutFit.Shrink;
                }

                item.layoutBox.ApplyLayoutHorizontal(x, alignedPosition, item.widthData, item.baseWidth, item.availableSize, fit, frameId);
                offset += item.widthData.marginStart + item.widthData.marginEnd + gap;
            }
        }

        private void RunLayoutVerticalStep_HorizontalDirection(int frameId) {
            float adjustedHeight = finalHeight - (paddingBorderVerticalStart + paddingBorderVerticalEnd);

            // todo -- once tracks get implemented we want to be able to use space-between and space-around to vertically align the tracks
            // track heights are equal to the max height of all contents in that track
            // when we only have 1 track the allocated height for every box is the full height of this element's content area and align content vertical does nothing

            // MainAxisAlignment contentAlignment = element.style.AlignContentVertical;

            // 2 options for horizontal height sizing:
            // 1. use the full content area height of this element
            // 2. use the tallest child's height + margin
            // if using the tallest child then we can center/start/end as normal
            // however it might be awkward if AlignContentVertical = Center is used and that just centers the allocated rects, not the actual content
            // so for now I have implemented option 1 for the case where we are not wrapping / only have 1 track

            float itemAlignment = element.style.AlignItemsVertical;
            LayoutFit verticalLayoutFit = element.style.FitItemsVertical;

            SpaceDistribution alignment = element.style.DistributeExtraSpaceVertical;

            if (alignment == SpaceDistribution.Default) {
                alignment = SpaceDistribution.AfterContent;
            }

            float inset = paddingBorderVerticalStart;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.layoutBox.GetHeights(ref item.heightData);
                item.baseHeight = item.heightData.Clamped;
                float offset = 0;
                SpaceDistributionUtil.GetAlignmentOffsets(adjustedHeight - (item.baseHeight + item.heightData.marginStart + item.heightData.marginEnd), 1,
                    alignment, out offset, out _);

                float y = inset + offset + item.heightData.marginStart;
                float allocatedHeight = adjustedHeight - (item.heightData.marginStart + item.heightData.marginEnd);
                float originOffset = allocatedHeight * itemAlignment;
                float alignedPosition = y + originOffset + (item.baseHeight * -itemAlignment);
                item.layoutBox.ApplyLayoutVertical(
                    y,
                    alignedPosition,
                    item.heightData,
                    item.baseHeight,
                    allocatedHeight,
                    verticalLayoutFit,
                    frameId
                );
            }
        }

        private void RunLayoutVerticalStep_VerticalDirection(int frameId) {
            float adjustedHeight = finalHeight - (paddingBorderVerticalStart + paddingBorderVerticalEnd);

            float remaining = adjustedHeight;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.grew = 0;
                item.shrunk = 0;
                item.layoutBox.GetHeights(ref item.heightData);
                item.baseHeight = item.heightData.Clamped;
                item.availableSize = item.baseHeight;
                remaining -= item.baseHeight + item.heightData.marginStart + item.heightData.marginEnd;
            }
            
            float gap = element.style.FlexLayoutGapVertical;

            if (element.style.DistributeExtraSpaceVertical == SpaceDistribution.AroundContent) {
                gap = 0;
            }

            remaining -= gap * (items.size - 1);

            if (remaining > 0) {
                remaining = GrowVertical(remaining);
            }
            else if (remaining < 0) {
                remaining = ShrinkVertical(remaining);
            }

            float offset = 0;
            float inset = paddingBorderVerticalStart;
            float spacerSize = 0;

            SpaceDistribution alignment = element.style.DistributeExtraSpaceVertical;
            if (alignment == default) {
                alignment = SpaceDistribution.AfterContent;
            }

            SpaceDistributionUtil.GetAlignmentOffsets(remaining, childCount, alignment, out offset, out spacerSize);
            float itemAlignment = element.style.AlignItemsVertical;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                float y = inset + item.heightData.marginStart + offset;
                offset += item.availableSize + spacerSize;
                LayoutFit fit = LayoutFit.None;

                float elementHeight = item.baseHeight;
                float originOffset = item.availableSize * itemAlignment;
                float alignedPosition = y + originOffset + (elementHeight * -itemAlignment);

                if (item.grew == 1) {
                    fit = LayoutFit.Grow;
                }
                else if (item.shrunk == 1) {
                    fit = LayoutFit.Shrink;
                }

                item.layoutBox.ApplyLayoutVertical(y, alignedPosition, item.heightData, item.baseHeight, item.availableSize, fit, frameId);
                offset += item.heightData.marginStart + item.heightData.marginEnd + gap;
            }
        }

        private void RunLayoutHorizontalStep_VerticalDirection(int frameId) {
            float contentStartX = paddingBorderHorizontalStart;
            LayoutFit fit = LayoutFit.Grow;

            fit = element.style.FitItemsHorizontal;

            float adjustedWidth = finalWidth - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);

            float inset = paddingBorderHorizontalStart;
            float spacerSize = 0;

            SpaceDistribution alignment = element.style.DistributeExtraSpaceHorizontal;

            if (alignment == SpaceDistribution.Default) {
                alignment = SpaceDistribution.AfterContent;
            }

            float itemAlignment = element.style.AlignItemsVertical;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.layoutBox.GetWidths(ref item.widthData);
                item.baseWidth = item.widthData.Clamped;

                SpaceDistributionUtil.GetAlignmentOffsets(adjustedWidth - item.baseWidth - (item.widthData.marginStart + item.widthData.marginEnd), 1, alignment,
                    out float offset, out spacerSize);

                float x = inset + offset;

                float availableWidth = adjustedWidth - (item.widthData.marginStart + item.widthData.marginEnd);
                float originBase = contentStartX + item.widthData.marginStart;
                float originOffset = (x - inset) + availableWidth * itemAlignment;
                float alignedPosition = originBase + originOffset + (item.baseWidth * -itemAlignment);

                item.layoutBox.ApplyLayoutHorizontal(x + item.widthData.marginStart, alignedPosition, item.widthData, item.baseWidth, availableWidth, fit, frameId);
            }
        }

        private void RunLayoutVerticalStep_HorizontalDirection_Wrapped(int frameId) {
            float contentStartY = paddingBorderVerticalStart;
            float gap = element.style.FlexLayoutGapVertical;
            if (element.style.DistributeExtraSpaceVertical == SpaceDistribution.AroundContent) {
                gap = 0;
            }

            float verticalAlignment = element.style.AlignItemsVertical;
            LayoutFit verticalLayoutFit = element.style.FitItemsVertical;

            float remainingHeight = finalHeight - (paddingBorderVerticalStart + paddingBorderVerticalEnd);
            SpaceDistribution spaceDistribution = element.style.DistributeExtraSpaceVertical;

            for (int i = 0; i < wrappedTracks.size; i++) {
                ref Track track = ref wrappedTracks.array[i];

                float maxHeight = 0;

                for (int j = track.startIndex; j < track.endIndex; j++) {
                    ref FlexItem item = ref items.array[j];
                    item.layoutBox.GetHeights(ref item.heightData);
                    item.baseHeight = item.heightData.Clamped;

                    float height = item.baseHeight + item.heightData.marginStart + item.heightData.marginEnd;

                    if (height > maxHeight) {
                        maxHeight = height;
                    }
                }

                track.height = maxHeight;
                remainingHeight -= maxHeight;
            }

            remainingHeight -= gap * (wrappedTracks.size - 1);

            float offset = 0;
            float spacerSize = 0;

            SpaceDistributionUtil.GetAlignmentOffsets(remainingHeight, wrappedTracks.size, spaceDistribution, out offset, out spacerSize);

            for (int i = 0; i < wrappedTracks.size; i++) {
                ref Track track = ref wrappedTracks.array[i];

                float height = track.height;
                float y = contentStartY + offset;
                
                for (int j = track.startIndex; j < track.endIndex; j++) {
                    ref FlexItem item = ref items.array[j];

                    float allocatedHeight = height - (item.heightData.marginStart + item.heightData.marginEnd);
                    float originBase = y + item.heightData.marginStart;
                    float originOffset = allocatedHeight * verticalAlignment;

                    item.layoutBox.ApplyLayoutVertical(
                        y + item.heightData.marginStart,
                        originBase + originOffset + (item.baseHeight * -verticalAlignment),
                        item.heightData,
                        item.baseHeight,
                        allocatedHeight,
                        verticalLayoutFit,
                        frameId
                    );
                }
                
                offset += height + spacerSize + gap;
            }
        }

        private struct FlexItem {
            public AwesomeLayoutBox layoutBox;
            public LayoutSize widthData;
            public LayoutSize heightData;
            public int growPieces;
            public int shrinkPieces;
            public float baseWidth;
            public float baseHeight;
            public float availableSize;
            public byte grew;
            public byte shrunk;
        }

        private struct Track {
            public int endIndex;
            public int startIndex;
            public float remaining;
            public float height;

            public Track(float remaining, int startIndex) {
                this.remaining = remaining;
                this.startIndex = startIndex;
                this.endIndex = startIndex;
                this.height = 0;
            }

            public bool IsEmpty => startIndex == endIndex;
        }
    }
}