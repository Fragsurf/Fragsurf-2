using System;
using System.Collections.Generic;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    /* ideas:
        when used as a base size, fr resolves to 0
        when used as a non base size (if base size is not also intrinsic) mx and mn resolve to 0
        mx and mn can only really be resolved as base sizes
        alternative would be treat grow(100px, 1mx) as non fixed, always
        reason being: we need a consistent value for mx and mn across grow/shrink/clamp
        grow(baseSize, growLimit)
        shrink(baseSize, shrinkLimit)
        flex(baseSize, fr)
        clamp(baseSize, min, max)
                        
        1fr 100px ==  clamp(0, 0, 1fr) clamp(100px, 100px, 100px)
          
        A grid is a 2d layout box that works by taking a template definition on the row and column axis
        Items can be placed explicitly in numbered grid cells or placed implicitly using one of two placement
        algorithms(explained later)
        
        
    */

    public struct GridCellDefinition {

        public int growFactor;
        public int shrinkFactor;
        public GridCellSize baseSize;
        public GridCellSize growLimit;
        public GridCellSize shrinkLimit;

    }

    public struct GridCellSize {

        public float value;
        public GridTemplateUnit unit;

        public GridCellSize(float value, GridTemplateUnit unit) {
            this.value = value;
            this.unit = unit;
        }

    }

    public class AwesomeGridLayoutBox : AwesomeLayoutBox {

        private bool placementDirty;
        internal readonly StructList<GridTrack> colTrackList;
        internal readonly StructList<GridTrack> rowTrackList;
        internal readonly StructList<GridPlacement> placementList;
        internal static readonly StructList<GridRegion> s_OccupiedAreas = new StructList<GridRegion>(32);
        private StructList<int> deferredList;
        private bool finalSizeResolutionMode;

        public int RowCount => rowTrackList.size;
        public int ColCount => colTrackList.size;

        public AwesomeGridLayoutBox() {
            this.placementList = new StructList<GridPlacement>();
            this.colTrackList = new StructList<GridTrack>(4);
            this.rowTrackList = new StructList<GridTrack>(4);
        }

        public override bool CanProvideHorizontalBlockSize(AwesomeLayoutBox layoutBox, out float blockSize) {
            blockSize = 0;
            if (finalSizeResolutionMode) {
                for (int i = 0; i < placementList.size; i++) {
                    if (placementList.array[i].layoutBox == layoutBox) {
                        ref GridPlacement placement = ref placementList.array[i];

                        for (int x = placement.x; x < placement.x + placement.width; x++) {
                            ref GridTrack track = ref colTrackList.array[x];

                            if ((track.cellDefinition.baseSize.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            if ((track.cellDefinition.shrinkLimit.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            if ((track.cellDefinition.growLimit.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            blockSize += track.size;
                        }

                        return true;
                    }
                }

                return false;
            }
            else {
                for (int i = 0; i < placementList.size; i++) {
                    if (placementList.array[i].layoutBox == layoutBox) {
                        ref GridPlacement placement = ref placementList.array[i];

                        for (int x = placement.x; x < placement.x + placement.width; x++) {
                            ref GridTrack track = ref colTrackList.array[x];

                            if ((track.cellDefinition.baseSize.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            if ((track.cellDefinition.shrinkLimit.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            if ((track.cellDefinition.growLimit.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            blockSize += track.resolvedBaseSize;
                        }

                        deferredList = deferredList ?? new StructList<int>();

                        bool contains = false;

                        for (int j = 0; j < deferredList.size; j++) {
                            if (deferredList.array[j] == i) {
                                contains = true;
                                break;
                            }
                        }

                        if (!contains) {
                            deferredList.Add(i);
                        }

                        return true;
                    }
                }

                return false;
            }
        }

        public override bool CanProvideVerticalBlockSize(AwesomeLayoutBox layoutBox, out float blockSize) {
            blockSize = 0;
            if (finalSizeResolutionMode) {
                for (int i = 0; i < placementList.size; i++) {
                    if (placementList.array[i].layoutBox == layoutBox) {
                        ref GridPlacement placement = ref placementList.array[i];

                        for (int y = placement.y; y < placement.y + placement.height; y++) {
                            ref GridTrack track = ref rowTrackList.array[y];

                            if ((track.cellDefinition.baseSize.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            if ((track.cellDefinition.shrinkLimit.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            if ((track.cellDefinition.growLimit.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            blockSize += track.size;
                        }

                        return true;
                    }
                }

                return false;
            }
            else {
                for (int i = 0; i < placementList.size; i++) {
                    if (placementList.array[i].layoutBox == layoutBox) {
                        ref GridPlacement placement = ref placementList.array[i];

                        for (int y = placement.y; y < placement.y + placement.height; y++) {
                            ref GridTrack track = ref rowTrackList.array[y];

                            // todo -- implement base units
                            // first pass uses base size
                            if ((track.cellDefinition.baseSize.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            if ((track.cellDefinition.shrinkLimit.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            if ((track.cellDefinition.growLimit.unit & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                                blockSize = 0;
                                return false;
                            }

                            blockSize += track.resolvedBaseSize;
                        }

                        deferredList = deferredList ?? new StructList<int>();

                        bool contains = false;

                        for (int j = 0; j < deferredList.size; j++) {
                            if (deferredList.array[j] == i) {
                                contains = true;
                                break;
                            }
                        }

                        if (!contains) {
                            deferredList.Add(i);
                        }

                        return true;
                    }
                }

                return false;
            }
        }

        protected override float ComputeContentWidth() {
            if (firstChild == null) {
                return 0;
            }

            Place();

            // first pass, get fixed sizes
            ComputeHorizontalFixedTrackSizes();

            ComputeItemWidths();

            // now compute the intrinsic sizes
            ComputeContentWidthContributionSizes();

            ResolveTrackSizes(colTrackList);

            float retn = 0;
            // this ignores fr sizes on purpose, fr size is always 0 for content size
            for (int i = 0; i < colTrackList.size; i++) {
                retn += colTrackList.array[i].resolvedBaseSize;
            }

            retn += element.style.GridLayoutColGap * (colTrackList.size - 1);

            return retn;
        }

        private float ResolveFixedHorizontalCellSize(in GridCellSize cellSize) {
            switch (cellSize.unit) {
                case GridTemplateUnit.Unset:
                case GridTemplateUnit.Pixel:
                    return cellSize.value;

                case GridTemplateUnit.ParentSize:
                    return ComputeBlockWidth(cellSize.value);

                case GridTemplateUnit.ParentContentArea:
                    return ComputeBlockContentAreaWidth(cellSize.value);

                case GridTemplateUnit.Em:
                    return element.style.GetResolvedFontSize() * cellSize.value;

                case GridTemplateUnit.ViewportWidth:
                    return element.View.Viewport.width * cellSize.value;

                case GridTemplateUnit.ViewportHeight:
                    return element.View.Viewport.height * cellSize.value;

                case GridTemplateUnit.Percent: {
                    if ((flags & LayoutBoxFlags.WidthBlockProvider) != 0) {
                        float w = element.layoutResult.actualSize.width - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);
                        if (w < 0) w = 0;
                        return w * cellSize.value;
                    }
                    else {
                        return ComputeBlockContentAreaWidth(cellSize.value);
                    }
                }

                case GridTemplateUnit.MinContent:
                case GridTemplateUnit.MaxContent:
                    return -1;

                default:
                    throw new ArgumentOutOfRangeException(cellSize.unit.ToString());
            }
        }

        private float ResolveFixedVerticalCellSize(in GridCellSize cellSize) {
            switch (cellSize.unit) {
                case GridTemplateUnit.Unset:
                case GridTemplateUnit.Pixel:
                    return cellSize.value;

                case GridTemplateUnit.ParentSize:
                    return ComputeBlockHeight(cellSize.value);

                case GridTemplateUnit.ParentContentArea:
                    return ComputeBlockContentHeight(cellSize.value);

                case GridTemplateUnit.Em:
                    return element.style.GetResolvedFontSize() * cellSize.value;

                case GridTemplateUnit.ViewportWidth:
                    return element.View.Viewport.width * cellSize.value;

                case GridTemplateUnit.ViewportHeight:
                    return element.View.Viewport.height * cellSize.value;

                case GridTemplateUnit.Percent: {
                    // if we have a fixed height and the unit is a percentage of our height, then we can safely return a fixed pixel value 
                    // as a percentage of this element's resolved height. If we have an unresolvable height we follow the BlockContentHeight
                    // algorithm as normal in all layout boxes. (ie check the ancestors until a resolvable height is found or the view height
                    // if no resolvable heights are found)
                    if ((flags & LayoutBoxFlags.HeightBlockProvider) != 0) {
                        float h = element.layoutResult.actualSize.height - (paddingBorderVerticalStart + paddingBorderVerticalEnd);
                        if (h < 0) h = 0;
                        return h * cellSize.value;
                    }
                    else {
                        return ComputeBlockContentHeight(cellSize.value);
                    }
                }

                case GridTemplateUnit.MinContent:
                case GridTemplateUnit.MaxContent:
                    return -1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ComputeHorizontalFixedTrackSizes() {
            for (int i = 0; i < colTrackList.size; i++) {
                ref GridTrack track = ref colTrackList.array[i];
                track.resolvedBaseSize = ResolveFixedHorizontalCellSize(track.cellDefinition.baseSize);
                track.resolvedGrowLimit = ResolveFixedHorizontalCellSize(track.cellDefinition.growLimit);
                track.resolvedShrinkLimit = ResolveFixedHorizontalCellSize(track.cellDefinition.shrinkLimit);
            }
        }

        /// <summary>
        /// Compute the fixed sizes of all grid tracks on the vertical axis. This will resolve all units except fr, mn, and mx into pixel values
        /// it will also set the fixedSizeMin and fixedSizeMax values on each track. If the unit is fr, mn, or mx, the fixed size is always 0
        /// </summary>
        private void ComputeVerticalFixedTrackSizes() {
            for (int i = 0; i < rowTrackList.size; i++) {
                ref GridTrack track = ref rowTrackList.array[i];
                track.resolvedBaseSize = ResolveFixedVerticalCellSize(track.cellDefinition.baseSize);
                track.resolvedGrowLimit = ResolveFixedVerticalCellSize(track.cellDefinition.growLimit);
                track.resolvedShrinkLimit = ResolveFixedVerticalCellSize(track.cellDefinition.shrinkLimit);
            }
        }

        /// <summary>
        /// Some sizes in the grid track definition are not immediately resolvable to pixels sizes. For these cases (MaxContent, MinContent)
        /// we need to look at every item in the children and figure out what the min and max content contributions are. This is not a 1-1 with elements
        /// since an element might span multiple tracks. The resolution algorithm is as follows:
        /// First we find the sum of all the fixed sized tracks that this element spans (horizontal axis in this case)
        /// While doing this, keep track of the number of intrinsically sized tracks the element spans.
        /// if we have any intrinsically sized spanned tracks, we compute the content contribution size as
        /// (element width + horizontal margins - spanned fixed track sizes) / number of spanned intrinsic tracks
        /// this value will be used later to resolve the sizes of mx and mn tracks 
        /// </summary>
        private void ComputeContentWidthContributionSizes() {
            for (int i = 0; i < colTrackList.size; i++) {
                ref GridTrack track = ref colTrackList.array[i];
                track.minContentContribution = -1;
                track.maxContentContribution = 0;
            }

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                int spannedIntrinsics = 0;
                float width = placement.outputWidth;

                for (int k = placement.x; k < placement.x + placement.width; k++) {
                    ref GridTrack track = ref colTrackList.array[k];
                    if (track.IsIntrinsic) {
                        spannedIntrinsics++;
                    }
                    else {
                        width -= track.resolvedBaseSize;
                    }
                }

                width = Mathf.Max(0, width);

                if (spannedIntrinsics > 0) {
                    placement.widthContributionSize = width / spannedIntrinsics;
                }
                else {
                    placement.widthContributionSize = 0;
                }

                for (int k = placement.x; k < placement.x + placement.width; k++) {
                    ref GridTrack track = ref colTrackList.array[k];

                    if (track.maxContentContribution < placement.widthContributionSize) {
                        track.maxContentContribution = placement.widthContributionSize;
                    }

                    if (track.minContentContribution == -1 || track.minContentContribution > placement.widthContributionSize) {
                        track.minContentContribution = placement.widthContributionSize;
                    }
                }
            }

        }

        /// See ComputeContentWidthContributionSizes, use height instead of width 
        private void ComputeContentHeightContributionSizes() {
            for (int i = 0; i < rowTrackList.size; i++) {
                ref GridTrack track = ref rowTrackList.array[i];
                track.minContentContribution = -1;
                track.maxContentContribution = 0;
            }

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                int spannedIntrinsics = 0;
                float height = placement.outputHeight;

                for (int k = placement.y; k < placement.y + placement.height; k++) {
                    ref GridTrack track = ref rowTrackList.array[k];
                    if (track.IsIntrinsic) {
                        spannedIntrinsics++;
                    }
                    else {
                        height -= track.resolvedBaseSize;
                    }
                }

                height = Mathf.Max(0, height);

                if (spannedIntrinsics > 0) {
                    placement.heightContributionSize = height / spannedIntrinsics;
                }
                else {
                    placement.heightContributionSize = 0;
                }

                for (int k = placement.y; k < placement.y + placement.height; k++) {
                    ref GridTrack track = ref rowTrackList.array[k];

                    if (track.maxContentContribution < placement.heightContributionSize) {
                        track.maxContentContribution = placement.heightContributionSize;
                    }

                    if (track.minContentContribution == -1 || track.minContentContribution > placement.heightContributionSize) {
                        track.minContentContribution = placement.heightContributionSize;
                    }
                }
            }
        }

        private static void ResolveTrackSizes(StructList<GridTrack> tracks) {
            for (int i = 0; i < tracks.size; i++) {
                ref GridTrack track = ref tracks.array[i];

                if (track.IsIntrinsic) {
                    // fixed sizes will have already been figured out by now
                    if (track.cellDefinition.baseSize.unit == GridTemplateUnit.MaxContent) {
                        track.resolvedBaseSize = track.cellDefinition.baseSize.value * track.maxContentContribution;
                    }

                    if (track.cellDefinition.baseSize.unit == GridTemplateUnit.MinContent) {
                        track.resolvedBaseSize = track.cellDefinition.baseSize.value * track.minContentContribution;
                    }

                    if (track.cellDefinition.growLimit.unit == GridTemplateUnit.MaxContent) {
                        track.resolvedGrowLimit = track.cellDefinition.growLimit.value * track.maxContentContribution;
                    }

                    if (track.cellDefinition.growLimit.unit == GridTemplateUnit.MinContent) {
                        track.resolvedGrowLimit = track.cellDefinition.growLimit.value * track.minContentContribution;
                    }

                    if (track.cellDefinition.shrinkLimit.unit == GridTemplateUnit.MaxContent) {
                        track.resolvedShrinkLimit = track.cellDefinition.shrinkLimit.value * track.maxContentContribution;
                    }

                    if (track.cellDefinition.shrinkLimit.unit == GridTemplateUnit.MinContent) {
                        track.resolvedShrinkLimit = track.cellDefinition.shrinkLimit.value * track.minContentContribution;
                    }
                }
            }
        }

        private void ComputeItemWidths() {
            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetWidths(ref placement.widthData);
                placement.outputWidth = placement.widthData.Clamped + placement.widthData.marginStart + placement.widthData.marginEnd;
            }
        }

        private void ComputeItemHeights() {
            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetHeights(ref placement.heightData);
                placement.outputHeight = placement.heightData.Clamped + placement.heightData.marginStart + placement.heightData.marginEnd;
            }
        }

        protected override float ComputeContentHeight() {
            if (firstChild == null) {
                return 0;
            }

            Place();

            // get a height for all children
            ComputeItemHeights();

            // first pass, get fixed sizes
            ComputeVerticalFixedTrackSizes();

            // now compute the intrinsic sizes for all tracks so we can resolve mn and mx
            ComputeContentHeightContributionSizes();

            // now we have all the size data, figure out actual pixel sizes for all vertical tracks
            ResolveTrackSizes(rowTrackList);

            // at this point we are done because we never grow and we never allocate to fr because by definition content height
            // is only the base size of the content without any growth applied
            // this ignores fr sizes on purpose, fr size is always 0 for content size

            // last step is to sum the base heights and add the row gap size
            float retn = 0;

            for (int i = 0; i < rowTrackList.size; i++) {
                retn += rowTrackList.array[i].resolvedBaseSize;
            }

            retn += element.style.GridLayoutRowGap * (rowTrackList.size - 1);

            return retn;
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) {
            placementDirty = true;
            // todo -- history entry?
        }

        public override void OnStyleChanged(StructList<StyleProperty> propertyList) {
            StyleProperty[] array = propertyList.array;
            int size = propertyList.size;
            for (int i = 0; i < size; i++) {
                ref StyleProperty property = ref array[i];
                switch (property.propertyId) {
                    case StylePropertyId.GridLayoutColAlignment:
                    case StylePropertyId.GridLayoutRowAlignment:
                        // layout? not sure what to do here since we just need to adjust alignment, not re calc sizes
                        break;

                    case StylePropertyId.GridLayoutDensity:
                    case StylePropertyId.GridLayoutDirection:
                        placementDirty = true;
                        flags |= LayoutBoxFlags.RequireLayoutHorizontal | LayoutBoxFlags.RequireLayoutVertical;
                        break;

                    case StylePropertyId.GridLayoutColAutoSize:
                    case StylePropertyId.GridLayoutColTemplate:
                        placementDirty = true;
                        // todo -- history entry?
                        flags |= LayoutBoxFlags.RequireLayoutHorizontal;
                        break;

                    case StylePropertyId.GridLayoutRowAutoSize:
                    case StylePropertyId.GridLayoutRowTemplate:
                        flags |= LayoutBoxFlags.RequireLayoutVertical;
                        placementDirty = true;
                        break;

                    case StylePropertyId.GridLayoutColGap:
                        flags |= LayoutBoxFlags.RequireLayoutHorizontal;
                        break;

                    case StylePropertyId.GridLayoutRowGap:
                        flags |= LayoutBoxFlags.RequireLayoutVertical;
                        // todo -- don't need to compute sizes again, just need to reposition tracks and assign sizes to children
                        break;

                    case StylePropertyId.FitItemsHorizontal:
                    case StylePropertyId.AlignItemsHorizontal:
                        flags |= LayoutBoxFlags.RequireLayoutHorizontal;
                        break;

                    case StylePropertyId.FitItemsVertical:
                    case StylePropertyId.AlignItemsVertical:
                        flags |= LayoutBoxFlags.RequireLayoutVertical;
                        // todo -- don't need to layout but do need to apply sizes again so these values update on children
                        break;
                }
            }
        }

        public override void OnChildStyleChanged(AwesomeLayoutBox child, StructList<StyleProperty> propertyList) {
            StyleProperty[] array = propertyList.array;
            int size = propertyList.size;
            for (int i = 0; i < size; i++) {
                ref StyleProperty property = ref array[i];
                switch (property.propertyId) {
                    case StylePropertyId.GridItemX:
                    case StylePropertyId.GridItemY:
                    case StylePropertyId.GridItemWidth:
                    case StylePropertyId.GridItemHeight:
                        placementDirty = true;
                        // todo -- history entry?
                        flags |= (LayoutBoxFlags.RequireLayoutHorizontal | LayoutBoxFlags.RequireLayoutVertical);
                        break;
                }
            }
        }

        public override void RunLayoutHorizontal(int frameId) {
            if (firstChild == null) {
                return;
            }

            Place();

            // todo -- some of these might not be dirty if we did a content layout pass, can used cached values already

            // first pass, get fixed sizes
            ComputeHorizontalFixedTrackSizes();

            ComputeItemWidths();

            // now compute the intrinsic sizes
            ComputeContentWidthContributionSizes();

            ResolveTrackSizes(colTrackList);

            float contentWidth = element.layoutResult.actualSize.width - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);

            float retn = 0;

            // this ignores fr sizes on purpose, fr size is always 0 for content size
            for (int i = 0; i < colTrackList.size; i++) {
                ref GridTrack track = ref colTrackList.array[i];
                track.size = track.resolvedBaseSize;
                retn += track.size;
            }

            retn += element.style.GridLayoutColGap * (colTrackList.size - 1);

            float remaining = contentWidth - retn;

            if (remaining > 0) {
                remaining = Grow(colTrackList, remaining);
            }
            else if (remaining < 0) {
                remaining = Shrink(colTrackList, remaining);
            }

            SpaceDistribution distribution = element.style.DistributeExtraSpaceHorizontal;
            if (distribution == SpaceDistribution.Default) {
                distribution = SpaceDistribution.AfterContent;
            }

            SpaceDistributionUtil.GetAlignmentOffsets(remaining, colTrackList.size, distribution, out float distributionOffset, out float spacerSize);

            PositionTracks(colTrackList, element.style.GridLayoutColGap, paddingBorderHorizontalStart + distributionOffset, spacerSize);

            float alignment = element.style.AlignItemsHorizontal;
            LayoutFit fit = element.style.FitItemsHorizontal;

            if (deferredList != null) {
                // if we needed to defer size resolution (only happens for block sized children for which the grid cells can provide a block size)
                // go through each of those and ask for their widths again. This will recursively end up calling CanProvideHorizontalBlockSize
                // which will now give the final size of each cell (thanks to the finalSizeResolutionMode) instead of the base. This fixes
                // issues where the grid grew or shrunk in size since providing the initial block size on the initial run.
                finalSizeResolutionMode = true;
                for (int i = 0; i < deferredList.size; i++) {
                    ref GridPlacement placement = ref placementList.array[deferredList.array[i]];
                    placement.layoutBox.GetWidths(ref placement.widthData);
                    placement.outputWidth = placement.widthData.Clamped + placement.widthData.marginStart + placement.widthData.marginEnd;
                }

                finalSizeResolutionMode = false;
                deferredList.size = 0;
            }

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];
                ref GridTrack startTrack = ref colTrackList.array[placement.x];
                ref GridTrack endTrack = ref colTrackList.array[placement.x + placement.width - 1];
                float elementWidth = placement.outputWidth - placement.widthData.marginStart - placement.widthData.marginEnd;
                float x = startTrack.position;
                float layoutBoxWidth = (endTrack.position + endTrack.size) - x;
                //+ (placement.widthData.marginStart - placement.widthData.marginEnd);
                float originBase = x + placement.widthData.marginStart;
                float originOffset = layoutBoxWidth * alignment;
                float offset = elementWidth * -alignment;
                float alignedPosition = originBase + originOffset + offset;
                placement.layoutBox.ApplyLayoutHorizontal(x, alignedPosition, placement.widthData, elementWidth, layoutBoxWidth, fit, frameId);
            }
        }

        private static void PositionTracks(StructList<GridTrack> trackList, float gap, float inset, float spacerSize = 0) {
            float offset = inset;
            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];
                track.position = offset;
                offset += gap + track.size + spacerSize;
            }
        }

        public override void RunLayoutVertical(int frameId) {
            Place();

            // todo -- some of these might not be dirty if we did a content layout pass, can used cached values already
            // first pass, get fixed sizes
            ComputeVerticalFixedTrackSizes();

            ComputeItemHeights();

            // now compute the intrinsic sizes
            ComputeContentHeightContributionSizes();

            ResolveTrackSizes(rowTrackList);

            float contentHeight = element.layoutResult.actualSize.height - (paddingBorderVerticalStart + paddingBorderVerticalEnd);

            float retn = 0;

            // this ignores fr sizes on purpose, fr size is always 0 for content size
            for (int i = 0; i < rowTrackList.size; i++) {
                ref GridTrack track = ref rowTrackList.array[i];
                track.size = track.resolvedBaseSize;
                retn += track.size;
            }

            retn += element.style.GridLayoutRowGap * (rowTrackList.size - 1);

            float remaining = contentHeight - retn;
            SpaceDistribution distribution = element.style.DistributeExtraSpaceVertical;
            if (distribution == SpaceDistribution.Default) {
                distribution = SpaceDistribution.AfterContent;
            }

            if (remaining > 0) {
                remaining = Grow(rowTrackList, remaining);
            }
            else if (remaining < 0) {
                remaining = Shrink(rowTrackList, remaining);
            }

            SpaceDistributionUtil.GetAlignmentOffsets(remaining, colTrackList.size, distribution, out float distributionOffset, out float spacerSize);
            PositionTracks(rowTrackList, element.style.GridLayoutRowGap, paddingBorderVerticalStart + distributionOffset, spacerSize);

            float alignment = element.style.AlignItemsVertical;
            LayoutFit fit = element.style.FitItemsVertical;

            if (deferredList != null) {
                // if we needed to defer size resolution (only happens for block sized children for which the grid cells can provide a block size)
                // go through each of those and ask for their widths again. This will recursively end up calling CanProvideHorizontalBlockSize
                // which will now give the final size of each cell (thanks to the finalSizeResolutionMode) instead of the base. This fixes
                // issues where the grid grew or shrunk in size since providing the initial block size on the initial run.
                finalSizeResolutionMode = true;
                for (int i = 0; i < deferredList.size; i++) {
                    ref GridPlacement placement = ref placementList.array[deferredList.array[i]];
                    placement.layoutBox.GetHeights(ref placement.heightData);
                    placement.outputHeight = placement.heightData.Clamped + placement.heightData.marginStart + placement.heightData.marginEnd;
                }

                finalSizeResolutionMode = false;
                deferredList.size = 0;
            }

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];
                ref GridTrack startTrack = ref rowTrackList.array[placement.y];
                ref GridTrack endTrack = ref rowTrackList.array[placement.y + placement.height - 1];
                float elementHeight = placement.outputHeight - placement.heightData.marginStart - placement.heightData.marginEnd;
                float y = startTrack.position;
                float layoutBoxHeight = (endTrack.position + endTrack.size) - y;
                //+ (placement.heightData.marginStart + placement.heightData.marginEnd);
                float originBase = y + placement.heightData.marginStart;
                float originOffset = layoutBoxHeight * alignment;
                float offset = elementHeight * -alignment;
                float alignedPosition = originBase + originOffset + offset;
                placement.layoutBox.ApplyLayoutVertical(y, alignedPosition, placement.heightData, elementHeight, layoutBoxHeight, fit, frameId);
            }
        }

        private static float GrowStep(StructList<GridTrack> trackList, float remaining) {
            // get grow limits for each track
            // distribute space in 'virtual' fr units across things that still want to grow
            float desiredSpace = 0;
            int pieces = 0;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];

                if (track.size < track.resolvedGrowLimit && track.cellDefinition.growFactor > 0) {
                    desiredSpace += track.resolvedGrowLimit - track.size;
                    pieces += track.cellDefinition.growFactor;
                }
            }

            if (pieces == 0) {
                return 0;
            }

            float allocatedSpace = 0;

            if (desiredSpace > remaining) {
                desiredSpace = remaining;
            }

            float pieceSize = desiredSpace / pieces;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];

                if (track.size < track.resolvedGrowLimit) {
                    track.size += pieceSize;
                    allocatedSpace += pieceSize;
                    if (track.size > track.resolvedGrowLimit) {
                        allocatedSpace -= (track.size - track.resolvedGrowLimit);
                        track.size = track.resolvedGrowLimit;
                        return allocatedSpace;
                    }
                }
            }

            return allocatedSpace;
        }

        private static float ShrinkStep(StructList<GridTrack> trackList, float overflow) {
            int pieces = 0;

            if (overflow >= 0) return 0;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];

                if (track.size > track.resolvedShrinkLimit && track.cellDefinition.shrinkFactor > 0) {
                    pieces += track.cellDefinition.shrinkFactor;
                }
            }

            if (pieces == 0) {
                return 0;
            }

            float allocatedSpace = 0;

            float pieceSize = -overflow / pieces;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];

                if (track.size > track.resolvedShrinkLimit) {
                    track.size -= pieceSize;
                    allocatedSpace += pieceSize;
                    if (track.size < track.resolvedShrinkLimit) {
                        allocatedSpace -= (track.size - track.resolvedShrinkLimit);
                        track.size = track.resolvedShrinkLimit;
                        return allocatedSpace;
                    }
                }
            }

            return allocatedSpace;
        }

        private static float Grow(StructList<GridTrack> trackList, float remaining) {
            float allocated = 0;
            do {
                allocated = GrowStep(trackList, remaining);
                remaining -= allocated;
            } while (allocated != 0);

            return remaining;
        }

        private static float Shrink(StructList<GridTrack> trackList, float remaining) {
            float allocated = 0;
            do {
                allocated = ShrinkStep(trackList, remaining);
                remaining += allocated;
            } while (allocated != 0);

            return remaining;
        }

        private void Place() {
            if (!placementDirty) {
                return;
            }

            placementDirty = false;

            placementList.size = 0;
            AwesomeLayoutBox child = firstChild;

            while (child != null) {
                GridItemPlacement x = child.element.style.GridItemX;
                GridItemPlacement y = child.element.style.GridItemY;
                GridItemPlacement width = child.element.style.GridItemWidth;
                GridItemPlacement height = child.element.style.GridItemHeight;

                GridPlacement placement = default;

                placement.layoutBox = child;
                placement.x = x.name != null ? ResolveHorizontalStart(x.name) : x.index;
                placement.y = y.name != null ? ResolveVerticalStart(y.name) : y.index;
                placement.width = width.name != null ? ResolveHorizontalWidth(placement.x, width.name) : width.index;
                placement.height = height.name != null ? ResolveVerticalHeight(placement.y, height.name) : height.index;

                placementList.Add(placement);

                child = child.nextSibling;
            }

            GenerateExplicitTracks();

            s_OccupiedAreas.size = 0;

            IReadOnlyList<GridTrackSize> autoColSizePattern = element.style.GridLayoutColAutoSize;
            IReadOnlyList<GridTrackSize> autoRowSizePattern = element.style.GridLayoutRowAutoSize;

            int rowSizeAutoPtr = 0;
            int colSizeAutoPtr = 0;

            PreAllocateRowAndColumns(ref colSizeAutoPtr, ref rowSizeAutoPtr, autoColSizePattern, autoRowSizePattern);

            PlaceBothAxisLocked();

            PlaceSingleAxisLocked(ref colSizeAutoPtr, autoColSizePattern, ref rowSizeAutoPtr, autoRowSizePattern);

            PlaceRemainingItems(ref colSizeAutoPtr, autoColSizePattern, ref rowSizeAutoPtr, autoRowSizePattern);
        }

        private static void GenerateExplicitTracksForAxis(IReadOnlyList<GridTrackSize> templateList, StructList<GridTrack> trackList) {
            int idx = 0;

            trackList.size = 0;

            trackList.EnsureCapacity(templateList.Count);

            for (int i = 0; i < templateList.Count; i++) {
                GridTrackSize template = templateList[i];

                switch (template.type) {
                    case GridTrackSizeType.Value:
                        trackList.array[idx++] = new GridTrack(template.cell);
                        break;

                    case GridTrackSizeType.MinMax:
                    case GridTrackSizeType.Repeat:
                    case GridTrackSizeType.RepeatFit:
                    case GridTrackSizeType.RepeatFill:
                        throw new NotImplementedException();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            trackList.size = idx;
        }

        private void GenerateExplicitTracks() {
            colTrackList.size = 0;
            rowTrackList.size = 0;

            IReadOnlyList<GridTrackSize> rowTemplate = element.style.GridLayoutRowTemplate;
            IReadOnlyList<GridTrackSize> colTemplate = element.style.GridLayoutColTemplate;

            if (rowTemplate.Count == 0 && colTemplate.Count == 0) {
                return;
            }

            GenerateExplicitTracksForAxis(colTemplate, colTrackList);
            GenerateExplicitTracksForAxis(rowTemplate, rowTrackList);
        }

        private int ResolveHorizontalStart(string name) {
            return -1;
        }

        private int ResolveHorizontalWidth(int resolvedColStart, string name) {
            return 1;
        }

        private int ResolveVerticalStart(string name) {
            return -1;
        }

        private int ResolveVerticalHeight(int resolvedRowStart, string name) {
            return 1;
        }

        private void PlaceBothAxisLocked() {
            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                if (placement.y >= 0 && placement.x >= 0) {
                    GridRegion region = new GridRegion();
                    region.xMin = placement.x;
                    region.yMin = placement.y;
                    region.xMax = placement.x + placement.width;
                    region.yMax = placement.y + placement.height;
                    s_OccupiedAreas.Add(region);
                }
            }
        }

        private void PlaceSingleAxisLocked(ref int colSizeAutoPtr, IReadOnlyList<GridTrackSize> autoColSizePattern, ref int rowSizeAutoPtr, IReadOnlyList<GridTrackSize> autoRowSizePattern) {
            bool dense = element.style.GridLayoutDensity == GridLayoutDensity.Dense;

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                int x = placement.x;
                int y = placement.y;
                int width = placement.width;
                int height = placement.height;

                // x axis is in a fixed position, we need to find a valid Y
                if (y < 0 && x >= 0) {
                    int cursorY = dense ? 0 : colTrackList.array[x].autoPlacementCursor;

                    while (!IsGridAreaAvailable(x, cursorY, width, height)) {
                        cursorY++;
                    }

                    EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
                    EnsureImplicitTrackCapacity(colTrackList, x + width, ref rowSizeAutoPtr, autoRowSizePattern);

                    colTrackList.array[x].autoPlacementCursor = cursorY;

                    placement.y = cursorY;

                    s_OccupiedAreas.Add(new GridRegion() {
                        xMin = placement.x,
                        yMin = placement.y,
                        xMax = placement.x + placement.width,
                        yMax = placement.y + placement.height
                    });
                }

                // if row was fixed we definitely created it in an earlier step
                else if (x < 0 && y >= 0) {
                    int cursorX = dense ? 0 : rowTrackList[y].autoPlacementCursor;

                    while (!IsGridAreaAvailable(cursorX, y, width, height)) {
                        cursorX++;
                    }

                    EnsureImplicitTrackCapacity(colTrackList, cursorX + width, ref colSizeAutoPtr, autoColSizePattern);
                    EnsureImplicitTrackCapacity(rowTrackList, y + height, ref rowSizeAutoPtr, autoRowSizePattern);

                    rowTrackList.array[y].autoPlacementCursor = cursorX;

                    placement.x = cursorX;

                    s_OccupiedAreas.Add(new GridRegion() {
                        xMin = placement.x,
                        yMin = placement.y,
                        xMax = placement.x + placement.width,
                        yMax = placement.y + placement.height
                    });
                }
            }
        }

        private void PreAllocateRowAndColumns(ref int colPtr, ref int rowPtr, IReadOnlyList<GridTrackSize> colPattern, IReadOnlyList<GridTrackSize> rowPattern) {
            int maxColStartAndSpan = 0;
            int maxRowStartAndSpan = 0;

            GridPlacement[] placements = placementList.array;

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placements[i];
                int colStart = placement.x;
                int rowStart = placement.y;
                int colSpan = placement.width;
                int rowSpan = placement.height;

                if (colStart < 1) {
                    colStart = 0;
                }

                if (rowStart < 1) {
                    rowStart = 0;
                }

                maxColStartAndSpan = maxColStartAndSpan > colStart + colSpan ? maxColStartAndSpan : colStart + colSpan;
                maxRowStartAndSpan = maxRowStartAndSpan > rowStart + rowSpan ? maxRowStartAndSpan : rowStart + rowSpan;
            }

            EnsureImplicitTrackCapacity(colTrackList, maxColStartAndSpan, ref colPtr, colPattern);
            EnsureImplicitTrackCapacity(rowTrackList, maxRowStartAndSpan, ref rowPtr, rowPattern);
        }

        private void PlaceRemainingItems(ref int colSizeAutoPtr, IReadOnlyList<GridTrackSize> autoColSizePattern, ref int rowSizeAutoPtr, IReadOnlyList<GridTrackSize> autoRowSizePattern) {
            if (placementList.size == 0) {
                return;
            }

            bool flowHorizontal = element.style.GridLayoutDirection == LayoutDirection.Horizontal;
            bool dense = element.style.GridLayoutDensity == GridLayoutDensity.Dense;

            int sparseStartX = 0; // purposefully not reading autoCursor value because that results in weird behavior for sparse grids (this is not the same as css!)
            int sparseStartY = 0; // purposefully not reading autoCursor value because that results in weird behavior for sparse grids (this is not the same as css!)

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                int width = placement.width;
                int height = placement.height;

                int cursorX = 0;
                int cursorY = 0;

                if (placement.x >= 0 || placement.y >= 0) {
                    continue;
                }

                if (flowHorizontal) {
                    if (dense) {
                        cursorX = 0;
                        cursorY = 0;
                    }
                    else {
                        cursorX = sparseStartX;
                        cursorY = sparseStartY;
                    }

                    while (true) {
                        if (cursorX + width > colTrackList.size) {
                            cursorY++;
                            // make sure enough rows exist to contain the entire vertical span
                            EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
                            cursorX = !dense ? rowTrackList.array[cursorY].autoPlacementCursor : 0;
                            continue;
                        }

                        if (IsGridAreaAvailable(cursorX, cursorY, width, height)) {
                            break;
                        }

                        cursorX++;
                    }

                    sparseStartX = cursorX + width;
                    sparseStartY = cursorY;
                    placement.x = cursorX;
                    placement.y = cursorY;
                    EnsureImplicitTrackCapacity(colTrackList, cursorX + width, ref colSizeAutoPtr, autoColSizePattern);
                    EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
//                    rowTrackList.array[cursorY].autoPlacementCursor = cursorX + width;
                    colTrackList.array[cursorX].autoPlacementCursor = cursorY;
                    //for (int j = cursorX; j < cursorX + width; j++) {
                    //}
                    for (int j = cursorY; j < cursorY + height; j++) {
                        rowTrackList.array[j].autoPlacementCursor = cursorX + width;
                    }
                }
                else {
                    if (dense) {
                        cursorX = 0;
                        cursorY = 0;
                    }
                    else {
                        cursorX = sparseStartX;
                        cursorY = sparseStartY;
                    }

                    while (true) {
                        if (cursorY + height > rowTrackList.size) {
                            cursorX++;
                            EnsureImplicitTrackCapacity(colTrackList, cursorX + width, ref colSizeAutoPtr, autoColSizePattern);
                            cursorY = !dense ? colTrackList.array[cursorX].autoPlacementCursor : 0;
                            continue;
                        }

                        if (IsGridAreaAvailable(cursorX, cursorY, width, height)) {
                            break;
                        }

                        cursorY++;
                    }

                    sparseStartX = cursorX;
                    sparseStartY = cursorY;
                    placement.x = cursorX;
                    placement.y = cursorY;
//                    colTrackList.array[cursorX].autoPlacementCursor = cursorY;
                    rowTrackList.array[cursorY].autoPlacementCursor = cursorX;
                    for (int j = cursorX; j < cursorX + width; j++) {
                        colTrackList.array[j].autoPlacementCursor = cursorY + height;
                    }

                    EnsureImplicitTrackCapacity(colTrackList, cursorX + width, ref colSizeAutoPtr, autoColSizePattern);
                    EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
                }

                s_OccupiedAreas.Add(new GridRegion() {
                    xMin = placement.x,
                    yMin = placement.y,
                    xMax = placement.x + placement.width,
                    yMax = placement.y + placement.height
                });
            }
        }

        private static bool IsGridAreaAvailable(int x, int y, int width, int height) {
            int xMax = x + width;
            int yMax = y + height;

            GridRegion[] array = s_OccupiedAreas.array;
            int count = s_OccupiedAreas.size;

            for (int i = 0; i < count; i++) {
                ref GridRegion check = ref array[i];
                if (!(y >= check.yMax || yMax <= check.yMin || xMax <= check.xMin || x >= check.xMax)) {
                    return false;
                }
            }

            return true;
        }

        private static void EnsureImplicitTrackCapacity(StructList<GridTrack> tracksList, int count, ref int autoSize, IReadOnlyList<GridTrackSize> autoSizes) {
            if (count >= tracksList.size) {
                tracksList.EnsureCapacity(count);

                int idx = tracksList.size;
                int toCreate = count - tracksList.size;

                for (int i = 0; i < toCreate; i++) {
                    tracksList.array[idx++] = new GridTrack(autoSizes[autoSize].cell);
                    autoSize = (autoSize + 1) % autoSizes.Count;
                }

                tracksList.size = idx;
            }
        }

        internal struct GridPlacement {

            public int x;
            public int y;
            public int width;
            public int height;
            public AwesomeLayoutBox layoutBox;
            public LayoutSize widthData;
            public LayoutSize heightData;
            public float outputWidth;
            public float outputHeight;
            public float widthContributionSize;
            public float heightContributionSize;

        }

        internal struct GridRegion {

            public int xMin;
            public int yMin;
            public int xMax;
            public int yMax;

        }

        internal struct GridTrack {

            public float position;
            public float size;
            public int autoPlacementCursor;
            public float resolvedBaseSize;
            public float resolvedGrowLimit;
            public float resolvedShrinkLimit;
            public GridCellDefinition cellDefinition;
            public float maxContentContribution;
            public float minContentContribution;

            public GridTrack(in GridCellDefinition cellDefinition) {
                this.cellDefinition = cellDefinition;
                this.position = 0;
                this.size = 0;
                this.autoPlacementCursor = 0;
                this.resolvedBaseSize = 0;
                this.resolvedGrowLimit = 0;
                this.resolvedShrinkLimit = 0;
                this.maxContentContribution = 0;
                this.minContentContribution = 0;
            }

            public bool IsIntrinsic {
                get {
                    return
                        (cellDefinition.baseSize.unit & GridTemplateUnit.Intrinsic) != 0 ||
                        (cellDefinition.shrinkLimit.unit & GridTemplateUnit.Intrinsic) != 0 ||
                        (cellDefinition.growLimit.unit & GridTemplateUnit.Intrinsic) != 0;
                }
            }

        }

    }

}