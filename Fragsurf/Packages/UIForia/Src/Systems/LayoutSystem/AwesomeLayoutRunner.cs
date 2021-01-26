using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = System.Diagnostics.Debug;

namespace UIForia.Systems {

    // this is crazy, but by using a struct as our array type, even though it just contains a reference, 
    // causes a massive performance increase. The reason is we avoid mono doing `Object.virt_stelemref_class_small_idepth`,
    // which is a complete undocumented part of mono that runs when you assign a reference type to an array slot.
    // By having a struct type on the array (even though its just a wrapper around our reference type),
    // we can increase the performance of array usage DRAMATICALLY. Example: Converting from using LightStack<UIElement>
    // to StructStack<ElemRef> improved performance of layout by over 300%!!!!!!!!!

    // source code reference is in the mono project: mono/mono/metadata/marshal.c
    // there is even a comment there that says: 
    // "Arrays are sealed but are covariant on their element type, We can't use any of the fast paths."
    // this means that arrays of reference types are always way slower than arrays of values type because of 
    // polymorphism and interfaces. Anyway, with this optimization and some other small changes
    // we brought the no-op layout run time of around 4000 elements from ~2.5ms down to ~0.2ms with deep profiling on

    // the bonus here is that this probably incurs no performance overhead with il2cpp because the struct is the same
    // size as the pointer so dereferencing it should yield identical performance (I haven't verified this though)

    // as far as I can tell, reading from ref typed arrays do not have this stelemref overhead, only writing.
    public struct ElemRef {

        public UIElement element;

        public ElemRef(UIElement element) {
            this.element = element;
        }

    }

    internal struct BoxRef {

        public AwesomeLayoutBox box;

    }

    public class AwesomeLayoutRunner {

        private int frameId;
        internal readonly UIElement rootElement;
        internal LightList<UIElement> hierarchyRebuildList;
        internal LightList<UIElement> alignHorizontalList;
        internal LightList<UIElement> alignVerticalList;
        internal LightList<AwesomeLayoutBox> ignoredList;
        internal StructList<ElemRef> queryableElements;
        internal StructStack<ElemRef> elemRefStack;
        internal StructStack<BoxRef> boxRefStack;
        internal LightList<UIElement> matrixUpdateList;
        internal LightList<ClipData> clipperList;

        private readonly LightStack<ClipData> clipStack;
        private readonly ClipData screenClipper;
        private readonly ClipData viewClipper;
        private readonly AwesomeLayoutSystem layoutSystem;

        private static readonly StructList<Vector2> s_SubjectRect = new StructList<Vector2>(4);

        private float lastDpi;

        public AwesomeLayoutRunner(AwesomeLayoutSystem layoutSystem, UIElement rootElement) {
            this.layoutSystem = layoutSystem;
            this.rootElement = rootElement;
            this.rootElement.layoutBox = new AwesomeRootLayoutBox();
            this.rootElement.layoutBox.Initialize(rootElement, 0);
            this.hierarchyRebuildList = new LightList<UIElement>();
            this.ignoredList = new LightList<AwesomeLayoutBox>();
            this.alignHorizontalList = new LightList<UIElement>();
            this.alignVerticalList = new LightList<UIElement>();
            this.queryableElements = new StructList<ElemRef>(32);
            this.elemRefStack = new StructStack<ElemRef>(32);
            this.boxRefStack = new StructStack<BoxRef>(32);
            this.matrixUpdateList = new LightList<UIElement>();
            this.clipperList = new LightList<ClipData>();
            this.screenClipper = new ClipData(null);
            this.viewClipper = new ClipData(rootElement);
            this.clipStack = new LightStack<ClipData>();
            lastDpi = rootElement.application.DPIScaleFactor;
        }


        public void RunLayout() {
            frameId = rootElement.application.frameId;


            if (rootElement.isDisabled) {
                return;
            }

            float currentDpi = rootElement.application.DPIScaleFactor;
            if (currentDpi != lastDpi) {
                InvalidateAll(rootElement);
            }

            clipperList.Clear();
            hierarchyRebuildList.Clear();
            queryableElements.Clear();
            ignoredList.Clear();

            float screenWidth = rootElement.application.Width;
            float screenHeight = rootElement.application.Height;

            screenClipper.orientedBounds.p0 = new Vector2(0, 0);
            screenClipper.orientedBounds.p1 = new Vector2(screenWidth, 0);
            screenClipper.orientedBounds.p2 = new Vector2(screenWidth, screenHeight);
            screenClipper.orientedBounds.p3 = new Vector2(0, screenHeight);

            screenClipper.intersected.array[0] = screenClipper.orientedBounds.p0;
            screenClipper.intersected.array[1] = screenClipper.orientedBounds.p1;
            screenClipper.intersected.array[2] = screenClipper.orientedBounds.p2;
            screenClipper.intersected.array[3] = screenClipper.orientedBounds.p3;
            screenClipper.intersected.size = 4;

            screenClipper.aabb = new Vector4(screenClipper.orientedBounds.p0.x, screenClipper.orientedBounds.p0.y, screenClipper.orientedBounds.p2.x, screenClipper.orientedBounds.p2.y);

            screenClipper.isCulled = false;

            viewClipper.parent = screenClipper;

            viewClipper.orientedBounds.p0 = new Vector2(0, 0);
            viewClipper.orientedBounds.p1 = new Vector2(screenWidth, 0);
            viewClipper.orientedBounds.p2 = new Vector2(screenWidth, screenHeight);
            viewClipper.orientedBounds.p3 = new Vector2(0, screenHeight);

            viewClipper.intersected.array[0] = viewClipper.orientedBounds.p0;
            viewClipper.intersected.array[1] = viewClipper.orientedBounds.p1;
            viewClipper.intersected.array[2] = viewClipper.orientedBounds.p2;
            viewClipper.intersected.array[3] = viewClipper.orientedBounds.p3;
            viewClipper.intersected.size = 4;

            viewClipper.aabb = new Vector4(viewClipper.orientedBounds.p0.x, viewClipper.orientedBounds.p0.y, viewClipper.orientedBounds.p2.x, viewClipper.orientedBounds.p2.y);

            viewClipper.isCulled = false; // todo -- wrong

            clipStack.Push(viewClipper);

            clipperList.Add(screenClipper);
            clipperList.Add(viewClipper);

            screenClipper.clipList.Clear();
            viewClipper.clipList.Clear();

            if (rootElement.enableStateChangedFrameId == frameId) {
                EnableHierarchy(rootElement);
                rootElement.layoutBox.flags |= LayoutBoxFlags.RequiresMatrixUpdate;
                matrixUpdateList.Add(rootElement);
            }
            else {
                GatherLayoutData();
                RebuildHierarchy();
            }

            clipStack.Pop(); // view

            PerformLayout();
            ApplyHorizontalAlignments();
            ApplyVerticalAlignments();
            ApplyLayoutResults();
            ApplyBoxSizeChanges();
            UpdateClippers();
        }

        private void InvalidateAll(UIElement element) {
            if (!element.isEnabled) {
                return;
            }

            if (element.layoutBox != null) {
                element.layoutBox.Invalidate();
            }

            if (element.children == null) {
                return;
            }
            
            for (int i = 0; i < element.children.size; i++) {
                InvalidateAll(element.children[i]);
            }
        }

        public void GatherLayoutData() {
            elemRefStack.array[elemRefStack.size++].element = rootElement;

            while (elemRefStack.size > 0) {
                UIElement currentElement = elemRefStack.array[--elemRefStack.size].element;

                // we push null onto the stack to signify clippers needing to be popped
                if (currentElement == null) {
                    clipStack.Pop();
                    continue;
                }

                UIElementFlags flags = currentElement.flags;

                AwesomeLayoutBox layoutBox = currentElement.layoutBox;

                layoutBox.traversalIndex = layoutSystem.traversalIndex++;

                if ((layoutBox.flags & LayoutBoxFlags.RequireLayoutHorizontal) != 0) {
                    if (currentElement.style.LayoutBehavior == LayoutBehavior.TranscludeChildren) {
                        layoutBox.flags &= ~LayoutBoxFlags.RequireLayoutHorizontal;
                    }
                    else {
                        layoutBox.MarkContentParentsHorizontalDirty(frameId, LayoutReason.DescendentStyleSizeChanged);
                    }
                }

                if ((layoutBox.flags & LayoutBoxFlags.RequireLayoutVertical) != 0) {
                    if (currentElement.style.LayoutBehavior == LayoutBehavior.TranscludeChildren) {
                        layoutBox.flags &= ~LayoutBoxFlags.RequireLayoutVertical;
                    }
                    else {
                        layoutBox.MarkContentParentsVerticalDirty(frameId, LayoutReason.DescendentStyleSizeChanged);
                    }
                }

                if ((layoutBox.flags & LayoutBoxFlags.Ignored) != 0) {
                    ignoredList.Add(layoutBox);
                }

                if ((layoutBox.flags & LayoutBoxFlags.RequireAlignmentHorizontal) != 0) {
                    alignHorizontalList.Add(currentElement);
                }

                if ((layoutBox.flags & LayoutBoxFlags.RequireAlignmentVertical) != 0) {
                    alignVerticalList.Add(currentElement);
                }

                if ((layoutBox.flags & LayoutBoxFlags.TransformDirty) != 0) {
                    layoutBox.flags |= LayoutBoxFlags.RequiresMatrixUpdate;
                    layoutBox.flags &= ~LayoutBoxFlags.TransformDirty;
                    matrixUpdateList.Add(currentElement);
                }

                if ((layoutBox.flags & LayoutBoxFlags.GatherChildren) != 0) {
                    hierarchyRebuildList.Add(currentElement);
                }

                switch (layoutBox.clipBehavior) {
                    case ClipBehavior.Never:

                        if (currentElement.layoutResult.clipper != null) {
                            layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                            currentElement.layoutResult.clipper = null;
                        }

                        currentElement.layoutResult.isCulled = false;
                        queryableElements.Add(new ElemRef(layoutBox.element)); // todo -- inline, or just avoid and iterate non culled clipper lists instead

                        break;

                    default:
                    case ClipBehavior.Normal: {
                        ClipData currentClipper = clipStack.array[clipStack.size - 1];
                        StructList<ElemRef> clipList = currentClipper.clipList;

                        if (currentElement.layoutResult.clipper != currentClipper) {
                            layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                            currentElement.layoutResult.clipper = currentClipper;
                        }

                        // this is inlined because it was identified as a hot point, inlining this makes it run significantly faster
                        if (clipList.size + 1 > clipList.array.Length) {
                            Array.Resize(ref clipList.array, (clipList.size + 1) * 2);
                        }

                        clipList.array[clipList.size++].element = currentElement;

                        break;
                    }
                    case ClipBehavior.Skip: {
                        ClipData currentClipper = clipStack.array[clipStack.size - 2];
                        StructList<ElemRef> clipList = currentClipper.clipList;

                        if (currentElement.layoutResult.clipper != currentClipper) {
                            layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                            currentElement.layoutResult.clipper = currentClipper;
                        }

                        // this is inlined because it was identified as a hot point, inlining this makes it run significantly faster
                        if (clipList.size + 1 > clipList.array.Length) {
                            Array.Resize(ref clipList.array, (clipList.size + 1) * 2);
                        }

                        clipList.array[clipList.size++].element = currentElement;

                        break;
                    }
                    case ClipBehavior.View: {
                        if (currentElement.layoutResult.clipper != viewClipper) {
                            layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                            currentElement.layoutResult.clipper = viewClipper;
                        }

                        viewClipper.clipList.Add(new ElemRef(currentElement));
                        break;
                    }

                    case ClipBehavior.Screen: {
                        if (currentElement.layoutResult.clipper != screenClipper) {
                            layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                            currentElement.layoutResult.clipper = screenClipper;
                        }

                        screenClipper.clipList.Add(new ElemRef(currentElement));
                        break;
                    }
                }

                if ((layoutBox.flags & LayoutBoxFlags.Clipper) != 0) {
                    layoutBox.clipData = layoutBox.clipData ?? new ClipData(layoutBox.element);
                    if (layoutBox.element.style.ClipBehavior == ClipBehavior.Never) {
                        layoutBox.clipData.parent = screenClipper;
                    }
                    else {
                        layoutBox.clipData.parent = clipStack.size != 0 ? clipStack.array[clipStack.size - 1] : screenClipper;
                    }

                    layoutBox.clipData.clipList.Clear();
                    clipStack.Push(layoutBox.clipData);
                    clipperList.Add(layoutBox.clipData);
                }

                currentElement.flags = flags; // write changes back to element

                UIElement[] childArray = currentElement.children.array;
                int childCount = currentElement.children.size;

                // todo -- presize to view.enabled count and remove check
                if (elemRefStack.size + childCount + 1 > elemRefStack.array.Length) {
                    elemRefStack.EnsureAdditionalCapacity(childCount);
                }

                // push null to signify we need to pop the clip stack later
                if ((layoutBox.flags & LayoutBoxFlags.Clipper) != 0) {
                    elemRefStack.array[elemRefStack.size++].element = null;
                }

                bool needsGather = (layoutBox.flags & LayoutBoxFlags.GatherChildren) != 0;

                // if any child had it's type changed, enabled, disabled, or behavior changed, need to gather

                for (int i = childCount - 1; i >= 0; i--) {
                    UIElement child = childArray[i];

                    bool childEnabled = (child.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet;

                    if (child.enableStateChangedFrameId == frameId) {
                        needsGather = true;

                        if (childEnabled) {
                            EnableHierarchy(child);
                            child.layoutBox.flags |= LayoutBoxFlags.RequiresMatrixUpdate;
                            matrixUpdateList.Add(child);
                        }
                    }
                    else if (childEnabled) {
                        // if child was previously enabled, it will definitely have a layout box
                        needsGather ^= (child.layoutBox.flags & LayoutBoxFlags.TypeOrBehaviorChanged) != 0;
                        if (elemRefStack.array.Length <= elemRefStack.size) {
                            elemRefStack.EnsureAdditionalCapacity(childCount);
                        }

                        elemRefStack.array[elemRefStack.size++].element = childArray[i];
                    }
                }

                if (needsGather) {
                    UIElement ptr = currentElement;
                    while (ptr != null) {
                        if (ptr.style.LayoutBehavior != LayoutBehavior.TranscludeChildren) {
                            if (!hierarchyRebuildList.Contains(ptr)) {
                                hierarchyRebuildList.Add(ptr);
                            }

                            break;
                        }

                        ptr = ptr.parent;
                    }
                }
            }
        }

        private void EnableHierarchy(UIElement currentElement) {
            if (currentElement.layoutBox == null) {
                layoutSystem.CreateLayoutBox(currentElement);
            }
            else {
                layoutSystem.ChangeLayoutBox(currentElement, currentElement.style.LayoutType);
            }

            Debug.Assert(currentElement.layoutBox != null, "currentElement.layoutBox != null");

            AwesomeLayoutBox layoutBox = currentElement.layoutBox;
            layoutBox.traversalIndex = layoutSystem.traversalIndex++;

            currentElement.layoutBox.Enable();

            if ((layoutBox.flags & LayoutBoxFlags.RequireAlignmentHorizontal) != 0) {
                alignHorizontalList.Add(currentElement);
            }

            if ((layoutBox.flags & LayoutBoxFlags.RequireAlignmentVertical) != 0) {
                alignVerticalList.Add(currentElement);
            }

            bool isClipper = (currentElement.layoutBox.flags & LayoutBoxFlags.Clipper) != 0;

            if (isClipper) {
                layoutBox.clipData = layoutBox.clipData ?? new ClipData(layoutBox.element);
                if (layoutBox.element.style.ClipBehavior == ClipBehavior.Never) {
                    layoutBox.clipData.parent = screenClipper;
                }
                else {
                    layoutBox.clipData.parent = clipStack.size != 0 ? clipStack.array[clipStack.size - 1] : screenClipper;
                }

                layoutBox.clipData.clipList.Clear();
                clipStack.Push(layoutBox.clipData);
                clipperList.Add(layoutBox.clipData);
            }

            LightList<AwesomeLayoutBox> list = LightList<AwesomeLayoutBox>.Get();

            for (int i = 0; i < currentElement.children.size; i++) {
                UIElement child = currentElement.children.array[i];

                if ((child.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
                    continue;
                }

                EnableHierarchy(child);

                switch (child.style.LayoutBehavior) {
                    case LayoutBehavior.Ignored:
                        child.layoutResult.layoutParent = currentElement.layoutResult; // todo -- multiple ignore levels?
                        ignoredList.Add(child.layoutBox);
                        break;
                    case LayoutBehavior.TranscludeChildren:
                        child.layoutBox.parent = currentElement.layoutBox;
                        child.layoutResult.layoutParent = currentElement.layoutResult; // todo -- multiple ignore levels?
                        child.layoutBox.GetChildren(list);
                        break;
                    default:
                        list.Add(child.layoutBox);
                        break;
                }
            }

            if (isClipper) {
                clipStack.Pop();
            }

            switch (layoutBox.clipBehavior) {
                case ClipBehavior.Never:

                    if (currentElement.layoutResult.clipper != null) {
                        layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                        currentElement.layoutResult.clipper = null;
                    }

                    currentElement.layoutResult.isCulled = false;
                    queryableElements.Add(new ElemRef(layoutBox.element)); // todo -- inline, or just avoid and iterate non culled clipper lists instead

                    break;

                default:
                case ClipBehavior.Normal: {
                    ClipData currentClipper = clipStack.array[clipStack.size - 1];
                    StructList<ElemRef> clipList = currentClipper.clipList;

                    if (currentElement.layoutResult.clipper != currentClipper) {
                        layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                        currentElement.layoutResult.clipper = currentClipper;
                    }

                    // this is inlined because it was identified as a hot point, inlining this makes it run significantly faster
                    if (clipList.size + 1 > clipList.array.Length) {
                        Array.Resize(ref clipList.array, (clipList.size + 1) * 2);
                    }

                    clipList.array[clipList.size++].element = currentElement;

                    break;
                }
                case ClipBehavior.View: {
                    if (currentElement.layoutResult.clipper != viewClipper) {
                        layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                        currentElement.layoutResult.clipper = viewClipper;
                    }

                    viewClipper.clipList.Add(new ElemRef(currentElement));
                    break;
                }

                case ClipBehavior.Screen: {
                    if (currentElement.layoutResult.clipper != screenClipper) {
                        layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
                        currentElement.layoutResult.clipper = screenClipper;
                    }

                    screenClipper.clipList.Add(new ElemRef(currentElement));
                    break;
                }
            }

            currentElement.layoutBox.SetChildren(list);
            LightList<AwesomeLayoutBox>.Release(ref list);
        }

        private void UpdateClippers() {
            for (int i = 1; i < clipperList.size; i++) {
                ClipData clipper = clipperList.array[i];
                clipper.isCulled = false;
                clipper.intersected.size = 0;
                clipper.visibleBoxCount = 0;

                if (clipper.parent.isCulled) {
                    clipper.aabb = default;
                    clipper.isCulled = true;
                    for (int j = 0; j < clipper.clipList.size; j++) {
                        UIElement element = clipper.clipList.array[j].element;
                        element.layoutResult.isCulled = true;
                    }

                    continue;
                }

                if (i != 1 && (clipper.element.layoutResult.actualSize.width == 0 || clipper.element.layoutResult.actualSize.height == 0)) {
                    clipper.isCulled = true;
                }
                else {
                    OrientedBounds bounds = clipper.orientedBounds;
                    s_SubjectRect.array[0] = bounds.p0;
                    s_SubjectRect.array[1] = bounds.p1;
                    s_SubjectRect.array[2] = bounds.p2;
                    s_SubjectRect.array[3] = bounds.p3;
                    s_SubjectRect.size = 4;

                    SutherlandHodgman.GetIntersectedPolygon(s_SubjectRect, clipper.parent.intersected, clipper.intersected);
                    clipper.isCulled = clipper.intersected.size == 0;
                }

                if (clipper.isCulled) {
                    clipper.aabb = default;
                    for (int j = 0; j < clipper.clipList.size; j++) {
                        UIElement element = clipper.clipList.array[j].element;
                        element.layoutResult.isCulled = true;
                    }
                }
                else {
                    clipper.aabb = PolygonUtil.GetBounds(clipper.intersected);

                    // definitely culled = clipper culled || aabb doesnt overlap clipper aabb
                    // let the gpu handle per pixel clipping, this is just a broadphase
                    for (int j = 0; j < clipper.clipList.size; j++) {
                        UIElement element = clipper.clipList.array[j].element;
                        if (element == rootElement) continue;
                        ref Vector4 aabb = ref element.layoutResult.axisAlignedBounds;
                        bool overlappingOrContains = aabb.z >= clipper.aabb.x && aabb.x <= clipper.aabb.z && aabb.w >= clipper.aabb.y && aabb.y <= clipper.aabb.w;
                        element.layoutResult.isCulled = !overlappingOrContains || (element.layoutResult.actualSize.width == 0 || element.layoutResult.actualSize.height == 0);

                        if (!element.layoutResult.isCulled) {
                            clipper.visibleBoxCount++;
                            queryableElements.Add(new ElemRef(element)); // todo -- inline, or just avoid and iterate non culled clipper lists instead
                        }
                    }
                }
            }
        }

        private void ApplyHorizontalAlignments() {
            float viewportWidth = rootElement.View.Viewport.width;
            float viewportHeight = rootElement.View.Viewport.height;
            UIView view = rootElement.View;
            InputSystem inputSystem = view.application.InputSystem;
            float screenWidth = view.application.Width;

            for (int i = 0; i < alignHorizontalList.size; i++) {
                UIElement element = alignHorizontalList.array[i];
                AwesomeLayoutBox box = element.layoutBox;

                // if box was aligned from a scroll view, continue

                LayoutResult result = element.layoutResult;

                // todo -- cache these values on layout box or make style reads fast
                OffsetMeasurement originX = box.element.style.AlignmentOriginX;
                OffsetMeasurement offsetX = box.element.style.AlignmentOffsetX;
                AlignmentDirection direction = box.element.style.AlignmentDirectionX;
                AlignmentTarget alignmentTargetX = element.style.AlignmentTargetX;
                AlignmentBoundary alignmentBoundaryX = element.style.AlignmentBoundaryX;

                float originBase = MeasurementUtil.ResolveOriginBaseX(result, view.position.x, alignmentTargetX, direction, inputSystem);
                float originSize = MeasurementUtil.ResolveOffsetOriginSizeX(result, viewportWidth, alignmentTargetX);
                float originOffset = MeasurementUtil.ResolveOffsetMeasurement(element, viewportWidth, viewportHeight, originX, originSize);
                float offset = MeasurementUtil.ResolveOffsetMeasurement(element, viewportWidth, viewportHeight, offsetX, box.finalWidth);

                if (direction == AlignmentDirection.End) {
                    result.alignedPosition.x = (originBase + originSize) - (originOffset + offset) - box.finalWidth;
                }
                else {
                    result.alignedPosition.x = originBase + originOffset + offset;
                }

                if (alignmentBoundaryX != AlignmentBoundary.Unset) {
                    switch (alignmentBoundaryX) {
                        case AlignmentBoundary.View: {
                            float viewPos = MeasurementUtil.GetXDistanceToView(result);
                            if (result.alignedPosition.x < viewPos) {
                                result.alignedPosition.x = viewPos;
                            }

                            if (result.alignedPosition.x + result.actualSize.width > viewportWidth + viewPos) {
                                result.alignedPosition.x = viewportWidth + viewPos - result.actualSize.width;
                            }

                            break;
                        }

                        case AlignmentBoundary.Clipper: {
                            float clipperPos = MeasurementUtil.GetXDistanceToClipper(result, out float clipperWidth);

                            if (result.alignedPosition.x < clipperPos) {
                                result.alignedPosition.x = clipperPos;
                            }

                            if (result.alignedPosition.x + result.actualSize.width > clipperWidth + clipperPos) {
                                result.alignedPosition.x = clipperWidth + clipperPos - result.actualSize.width;
                            }

                            break;
                        }
                        case AlignmentBoundary.Screen:
                            float screenPos = MeasurementUtil.GetXDistanceToScreen(result);
                            if (result.alignedPosition.x < screenPos) {
                                result.alignedPosition.x = screenPos;
                            }

                            if (result.alignedPosition.x + result.actualSize.width > screenWidth + screenPos) {
                                result.alignedPosition.x = screenWidth + screenPos - result.actualSize.width;
                            }

                            break;

                        case AlignmentBoundary.Parent: {
                            if (result.alignedPosition.x < 0) {
                                result.alignedPosition.x = 0;
                            }

                            AwesomeLayoutBox parent = box.parent;
                            if (parent == null) {
                                parent = element.parent.layoutBox;
                            }
                            
                            if (result.alignedPosition.x + result.actualSize.width > parent.finalWidth) {
                                result.alignedPosition.x -= (result.alignedPosition.x + result.actualSize.width) - parent.finalWidth;
                            }

                            break;
                        }
                        case AlignmentBoundary.ParentContentArea: {
                            if (result.alignedPosition.x < box.parent.paddingBorderHorizontalStart) {
                                result.alignedPosition.x = box.parent.paddingBorderHorizontalStart;
                            }

                            float width = box.parent.finalWidth - box.parent.paddingBorderHorizontalEnd;
                            if (result.alignedPosition.x + result.actualSize.width > width) {
                                result.alignedPosition.x -= (result.alignedPosition.x + result.actualSize.width) - width;
                            }

                            break;
                        }
                    }
                }

                // todo -- this is caching problem! fix it!
                //  if (!Mathf.Approximately(previousPosition, result.alignedPosition.x)) {
                //   if ((box.flags & LayoutBoxFlags.RequiresMatrixUpdate) != 0) {
                box.flags |= LayoutBoxFlags.RequiresMatrixUpdate;
                matrixUpdateList.Add(box.element);
                //   }
                // }
            }

            alignHorizontalList.Clear();
        }

        private void ApplyVerticalAlignments() {
            float viewportWidth = rootElement.View.Viewport.width;
            float viewportHeight = rootElement.View.Viewport.height;
            UIView view = rootElement.View;
            float screenHeight = rootElement.application.Height;
            InputSystem inputSystem = view.application.InputSystem;

            for (int i = 0; i < alignVerticalList.size; i++) {
                UIElement element = alignVerticalList.array[i];
                AwesomeLayoutBox box = element.layoutBox;
                LayoutResult result = element.layoutResult;

                // todo -- cache these values on layout box or make style reads fast
                OffsetMeasurement originY = box.element.style.AlignmentOriginY;
                OffsetMeasurement offsetY = box.element.style.AlignmentOffsetY;
                AlignmentDirection direction = box.element.style.AlignmentDirectionY;
                AlignmentTarget alignmentTargetY = element.style.AlignmentTargetY;
                AlignmentBoundary alignmentBoundaryY = element.style.AlignmentBoundaryY;

                float originBase = MeasurementUtil.ResolveOriginBaseY(result, view.position.y, alignmentTargetY, direction, inputSystem);
                float originSize = MeasurementUtil.ResolveOffsetOriginSizeY(result, viewportHeight, alignmentTargetY);
                float originOffset = MeasurementUtil.ResolveOffsetMeasurement(element, viewportWidth, viewportHeight, originY, originSize);
                float offset = MeasurementUtil.ResolveOffsetMeasurement(element, viewportWidth, viewportHeight, offsetY, box.finalHeight);

                if (direction == AlignmentDirection.End) {
                    result.alignedPosition.y = (originBase + originSize) - (originOffset + offset) - box.finalHeight;
                }
                else {
                    result.alignedPosition.y = originBase + originOffset + offset;
                }

                if (alignmentBoundaryY != AlignmentBoundary.Unset) {
                    switch (alignmentBoundaryY) {
                        case AlignmentBoundary.View: {
                            float viewPos = MeasurementUtil.GetYDistanceToView(result);
                            if (result.alignedPosition.y < viewPos) {
                                result.alignedPosition.y = viewPos;
                            }

                            if (result.alignedPosition.y + result.actualSize.height > viewportHeight + viewPos) {
                                result.alignedPosition.y = viewportHeight + viewPos - result.actualSize.height;
                            }

                            break;
                        }

                        case AlignmentBoundary.Clipper: {
                            float clipperPos = MeasurementUtil.GetYDistanceToClipper(result, out float clipperHeight);

                            if (result.alignedPosition.y < clipperPos) {
                                result.alignedPosition.y = clipperPos;
                            }

                            if (result.alignedPosition.y + result.actualSize.height > clipperHeight + clipperPos) {
                                result.alignedPosition.y = clipperHeight + clipperPos - result.actualSize.height;
                            }

                            break;
                        }

                        case AlignmentBoundary.Screen:

                            float screenPos = MeasurementUtil.GetYDistanceToScreen(result);
                            if (result.alignedPosition.y < screenPos) {
                                result.alignedPosition.y = screenPos;
                            }

                            if (result.alignedPosition.y + result.actualSize.height > screenHeight + screenPos) {
                                result.alignedPosition.y = screenHeight + screenPos - result.actualSize.height;
                            }

                            break;
                        case AlignmentBoundary.Parent: {
                            if (result.alignedPosition.y < 0) {
                                result.alignedPosition.y = 0;
                            }

                            if (result.alignedPosition.y + result.actualSize.height > box.parent.finalHeight) {
                                result.alignedPosition.y -= (result.alignedPosition.y + result.actualSize.height) - box.parent.finalHeight;
                            }

                            break;
                        }
                        case AlignmentBoundary.ParentContentArea: {
                            if (result.alignedPosition.y < box.parent.paddingBorderVerticalStart) {
                                result.alignedPosition.y = box.parent.paddingBorderVerticalStart;
                            }

                            float height = box.parent.finalHeight - box.parent.paddingBorderVerticalEnd;
                            if (result.alignedPosition.y + result.actualSize.height > height) {
                                result.alignedPosition.y -= (result.alignedPosition.y + result.actualSize.height) - height;
                            }

                            break;
                        }
                    }
                }

                // if (alignmentBoundary == AlignmentBoundary.ScreenEnd) {
                //     
                //  
                //     if (diff > 0) {
                //         result.alignedPosition.y += diff;
                //     }
                //     
                // }

                // todo -- this is caching problem! fix it!

                // if (!Mathf.Approximately(previousPosition, result.alignedPosition.y)) {
                //  if ((box.flags & LayoutBoxFlags.RequiresMatrixUpdate) != 0) {
                box.flags |= LayoutBoxFlags.RequiresMatrixUpdate;
                matrixUpdateList.Add(box.element);
                //   }
                // }
            }

            alignVerticalList.Clear();
        }

        private void PerformLayoutStepHorizontal(AwesomeLayoutBox rootBox) {
            boxRefStack.Push(new BoxRef() {box = rootBox});

            // Resolve all widths first, then process heights. These operations cannot be interleaved for we can't be sure
            // that widths are final before heights are computed, this is critical for the system to work.

            while (boxRefStack.size > 0) {
                AwesomeLayoutBox layoutBox = boxRefStack.array[--boxRefStack.size].box;

                if ((layoutBox.flags & LayoutBoxFlags.ContentAreaWidthChanged) != 0) {
                    layoutBox.UpdateContentAreaWidth();
                }

                if ((layoutBox.flags & (LayoutBoxFlags.AlwaysUpdate | LayoutBoxFlags.RequireLayoutHorizontal)) != 0) {
                    layoutBox.RunLayoutHorizontal(frameId);
                    layoutBox.flags &= ~LayoutBoxFlags.RequireLayoutHorizontal;
                }

                // no need to size check the stack, same size as element stack which was already sized
                AwesomeLayoutBox ptr = layoutBox.firstChild;
                while (ptr != null) {
                    boxRefStack.Push(new BoxRef { box = ptr });
                    ptr = ptr.nextSibling;
                }
            }
        }

        private void PerformLayoutStepVertical(AwesomeLayoutBox rootBox) {
            float viewWidth = rootBox.element.View.Viewport.width;
            float viewHeight = rootBox.element.View.Viewport.height;

            boxRefStack.Push(new BoxRef() {box = rootBox});

            while (boxRefStack.size > 0) {
                AwesomeLayoutBox layoutBox = boxRefStack.array[--boxRefStack.size].box;

                if ((layoutBox.flags & LayoutBoxFlags.ContentAreaHeightChanged) != 0) {
                    layoutBox.UpdateContentAreaHeight();
                }

                if ((layoutBox.flags & (LayoutBoxFlags.AlwaysUpdate | LayoutBoxFlags.RequireLayoutVertical)) != 0) {
                    layoutBox.RunLayoutVertical(frameId);
                    layoutBox.flags &= ~LayoutBoxFlags.RequireLayoutVertical;
                }

                if ((layoutBox.element.flags & UIElementFlags.LayoutTransformNotIdentity) != 0) {
                    float x = MeasurementUtil.ResolveOffsetMeasurement(layoutBox.element, viewWidth, viewHeight, layoutBox.transformPositionX, layoutBox.finalWidth);
                    float y = MeasurementUtil.ResolveOffsetMeasurement(layoutBox.element, viewWidth, viewHeight, layoutBox.transformPositionY, layoutBox.finalHeight);
                    if (!Mathf.Approximately(x, layoutBox.transformX) || !Mathf.Approximately(y, layoutBox.transformY)) {
                        layoutBox.transformX = x;
                        layoutBox.transformY = y;
                        layoutBox.flags |= LayoutBoxFlags.RequiresMatrixUpdate;
                    }
                }

                // if we need to update this element's matrix then add to the list
                // this can happen if the parent assigned a different position to 
                // this element (and the element doesn't have an alignment override)
                if ((layoutBox.flags & LayoutBoxFlags.RequiresMatrixUpdate) != 0) {
                    matrixUpdateList.Add(layoutBox.element); // could be a struct list with 
                }

                // no need to size check the stack, same size as element stack which was already sized

                AwesomeLayoutBox ptr = layoutBox.firstChild;
                while (ptr != null) {
                    boxRefStack.Push(new BoxRef { box = ptr });
                    ptr = ptr.nextSibling;
                }
            }
        }

        private void PerformLayoutStep(AwesomeLayoutBox rootBox) {
            PerformLayoutStepHorizontal(rootBox);
            PerformLayoutStepVertical(rootBox);
        }

        private void PerformLayoutStepHorizontal_Ignored(AwesomeLayoutBox ignoredBox) {
            AwesomeLayoutBox.LayoutSize size = default;
            //if ((ignoredBox.flags & LayoutBoxFlags.RequireLayoutHorizontal) != 0) {
            ignoredBox.GetWidths(ref size);
            float outputSize = size.Clamped;
            ignoredBox.ApplyLayoutHorizontal(0, 0, size, outputSize, ignoredBox.parent?.finalWidth ?? outputSize, LayoutFit.None, frameId);
            PerformLayoutStepHorizontal(ignoredBox);
            //}
        }

        private void PerformLayoutStepVertical_Ignored(AwesomeLayoutBox ignoredBox) {
            AwesomeLayoutBox.LayoutSize size = default;
            ignoredBox.GetHeights(ref size);
            float outputSize = size.Clamped;
            ignoredBox.ApplyLayoutVertical(0, 0, size, outputSize, ignoredBox.parent?.finalHeight ?? outputSize, LayoutFit.None, frameId);
            PerformLayoutStepVertical(ignoredBox);
        }

        private void PerformLayout() {
            // save size checks later while traversing
            // todo find the real number of elements (huge repeats) to use the inlined boxRefStack.Push variant during layout again
            boxRefStack.EnsureCapacity(elemRefStack.array.Length * 4);

            PerformLayoutStep(rootElement.layoutBox);

            for (int i = 0; i < ignoredList.size; i++) {
                AwesomeLayoutBox ignoredBox = ignoredList.array[i];

                // todo -- account for margin on ignored element

                PerformLayoutStepHorizontal_Ignored(ignoredBox);
                PerformLayoutStepVertical_Ignored(ignoredBox);
            }
        }

        private void ApplyLayoutResults() {
            int size = matrixUpdateList.size;
            UIElement[] array = matrixUpdateList.array;

            float viewWidth = rootElement.View.Viewport.width;
            float viewHeight = rootElement.View.Viewport.height;

            SVGXMatrix identity = SVGXMatrix.identity;

            for (int i = 0; i < size; i++) {
                UIElement startElement = array[i];
                AwesomeLayoutBox box = startElement.layoutBox;

                // this element might have been processed by it's parent traversing, this check makes sure we only traverse an element hierarchy once

                if ((box.flags & LayoutBoxFlags.RequiresMatrixUpdate) != 0) {
                    elemRefStack.Push(new ElemRef(startElement));

                    while (elemRefStack.size > 0) {
                        UIElement currentElement = elemRefStack.array[--elemRefStack.size].element;
                        AwesomeLayoutBox currentBox = currentElement.layoutBox;
                        LayoutResult result = currentElement.layoutResult;
                        currentBox.flags &= ~LayoutBoxFlags.RequiresMatrixUpdate;

                        float scrollPixelAmountX = 0;
                        float scrollPixelAmountY = 0;
                        if (layoutSystem.horizontalScrollFixed.Contains(currentElement)) {
                            ScrollView scrollView = currentElement.FindParent<ScrollView>();
                            scrollPixelAmountX = scrollView.scrollPixelAmountX;
                        }
                        
                        if (layoutSystem.verticalScrollFixed.Contains(currentElement)) {
                            ScrollView scrollView = currentElement.FindParent<ScrollView>();
                            scrollPixelAmountY = scrollView.scrollPixelAmountY;
                        }
                        // TODO(roman): Handling removing stuff from the hor/ver scroll lists
                        // for (int j = 0; j < layoutSystem.horizontalScrollFixed.size; ++j) {
                        //     UIElement element = layoutSystem.horizontalScrollFixed[j];
                        //     if (!element.isEnabled || element.style.ScrollBehaviorX != ScrollBehavior.Fixed) {
                        //         layoutSystem.horizontalScrollFixed.SwapRemoveAt(j);
                        //         j--;
                        //         continue;
                        //     }
                        //
                        //     ScrollView scrollView = element.FindParent<ScrollView>();
                        //     if (scrollView == null) {
                        //         layoutSystem.horizontalScrollFixed.SwapRemoveAt(j);
                        //         j--;
                        //         continue;
                        //     }
                        //
                        //     element.layoutResult.alignedPosition.x += scrollView.scrollPixelAmountX;
                        // }
            
                        // for (int j = 0; j < layoutSystem.verticalScrollFixed.size; ++j) {
                        //     UIElement element = layoutSystem.verticalScrollFixed[j];
                        //     if (!element.isEnabled || element.style.ScrollBehaviorY != ScrollBehavior.Fixed) {
                        //         layoutSystem.verticalScrollFixed.SwapRemoveAt(j);
                        //         j--;
                        //         continue;
                        //     }
                        //
                        //     ScrollView scrollView = element.FindParent<ScrollView>();
                        //     if (scrollView == null) {
                        //         layoutSystem.verticalScrollFixed.SwapRemoveAt(j);
                        //         j--;
                        //         continue;
                        //     }
                        //
                        //     element.layoutResult.alignedPosition.y += scrollView.scrollPixelAmountY;
                        // }


                        result.localPosition.x = result.alignedPosition.x - scrollPixelAmountX;
                        result.localPosition.y = result.alignedPosition.y - scrollPixelAmountY;

                        ref SVGXMatrix localMatrix = ref result.localMatrix;

                        // todo -- maybe faster to get pointer to struct and check that memory is 0
                        bool isIdentity = (
                            currentBox.transformRotation == 0 &&
                            currentBox.transformPositionX.value == 0 &&
                            currentBox.transformPositionY.value == 0 &&
                            currentBox.transformScaleX == 1 &&
                            currentBox.transformScaleY == 1
                            // don't think we need to apply pivot unless transforming
                            // currentBox.transformPivotX.value == 0 && 
                            // currentBox.transformPivotY.value == 0
                        );

                        if (!isIdentity) {
                            // todo -- only need to do this if transform is not identity
                            float x = MeasurementUtil.ResolveOffsetMeasurement(currentElement, viewWidth, viewHeight, currentBox.transformPositionX, currentBox.finalWidth);
                            float y = MeasurementUtil.ResolveOffsetMeasurement(currentElement, viewWidth, viewHeight, currentBox.transformPositionY, currentBox.finalHeight);

                            // todo -- em size
                            float px = MeasurementUtil.ResolveFixedSize(result.actualSize.width, viewWidth, viewHeight, 0, currentBox.transformPivotX);
                            float py = MeasurementUtil.ResolveFixedSize(result.actualSize.height, viewWidth, viewHeight, 0, currentBox.transformPivotY);

                            float rotation = currentBox.transformRotation * Mathf.Deg2Rad;
                            float ca = Mathf.Cos(rotation);
                            float sa = Mathf.Sin(rotation);
                            float scaleX = currentBox.transformScaleX;
                            float scaleY = currentBox.transformScaleY;

                            localMatrix.m0 = ca * scaleX;
                            localMatrix.m1 = sa * scaleX;
                            localMatrix.m2 = -sa * scaleY;
                            localMatrix.m3 = ca * scaleY;
                            localMatrix.m4 = result.alignedPosition.x + x; // not totally sure just adding x and y is correct
                            localMatrix.m5 = result.alignedPosition.y + y; // not totally sure just adding x and y is correct

                            if (px != 0 || py != 0) {
                                SVGXMatrix pivot = new SVGXMatrix(1, 0, 0, 1, px, py);
                                SVGXMatrix pivotBack = pivot;
                                pivotBack.m4 = -px;
                                pivotBack.m5 = -py;
                                // todo -- there must be a way to not do a matrix multiply here
                                localMatrix = pivot * localMatrix * pivotBack;
                            }
                        }
                        else {
                            localMatrix = identity;
                            localMatrix.m4 = result.localPosition.x;
                            localMatrix.m5 = result.localPosition.y;
                        }

                        // todo -- avoid this null check by never adding root to stack

                        if (currentElement.parent != null) {
                            // inlined this because according to the profiler this is a lot faster now
                            // result.matrix = element.parent.layoutResult.matrix * localMatrix;
                            ref SVGXMatrix m = ref result.matrix;
                            ref SVGXMatrix left = ref currentElement.parent.layoutResult.matrix;
                            ref SVGXMatrix right = ref localMatrix;
                            m.m0 = left.m0 * right.m0 + left.m2 * right.m1;
                            m.m1 = left.m1 * right.m0 + left.m3 * right.m1;
                            m.m2 = left.m0 * right.m2 + left.m2 * right.m3;
                            m.m3 = left.m1 * right.m2 + left.m3 * right.m3;
                            m.m4 = left.m0 * right.m4 + left.m2 * right.m5 + left.m4;
                            m.m5 = left.m1 * right.m4 + left.m3 * right.m5 + left.m5;
                        }
                        else {
                            result.matrix = localMatrix;
                        }

                        result.screenPosition.x = result.matrix.m4; // maybe should be aabb position?
                        result.screenPosition.y = result.matrix.m5; // maybe should be aabb position?

                        int childCount = currentElement.children.size;

                        if (elemRefStack.size + childCount > elemRefStack.array.Length) {
                            elemRefStack.EnsureAdditionalCapacity(childCount);
                        }

                        for (int childIdx = 0; childIdx < childCount; childIdx++) {
                            UIElement child = currentElement.children.array[childIdx];
                            if ((child.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet) {
                                elemRefStack.array[elemRefStack.size++].element = child;
                            }
                        }
                    }
                }
            }

            matrixUpdateList.Clear();
        }

        private void ApplyBoxSizeChanges() {
            // for anything that had a matrix update or a size change we need to recompute bounding boxes

            elemRefStack.array[elemRefStack.size++].element = rootElement;

            // todo -- only do this if matrix or size changed

            while (elemRefStack.size > 0) {
                UIElement currentElement = elemRefStack.array[--elemRefStack.size].element;
                LayoutResult result = currentElement.layoutResult;

                float x = 0;
                float y = 0;

                float width = result.actualSize.width;
                float height = result.actualSize.height;

                switch (currentElement.layoutBox.clipBounds) {
                    case ClipBounds.ContentBox:
                        x = result.padding.left + result.border.left;
                        y = result.padding.top + result.border.top;
                        width -= result.padding.right + result.border.right;
                        height -= result.padding.bottom + result.border.bottom;
                        break;
                }

                OrientedBounds orientedBounds = result.orientedBounds;
                ref SVGXMatrix m = ref result.matrix;

                // inlined svgxMatrix.Transform(point), takes runtime for ~4000 elements from 4.5ms to 0.47ms w/ deep profile on
                orientedBounds.p0.x = m.m0 * x + m.m2 * y + m.m4;
                orientedBounds.p0.y = m.m1 * x + m.m3 * y + m.m5;

                orientedBounds.p1.x = m.m0 * width + m.m2 * y + m.m4;
                orientedBounds.p1.y = m.m1 * width + m.m3 * y + m.m5;

                orientedBounds.p2.x = m.m0 * width + m.m2 * height + m.m4;
                orientedBounds.p2.y = m.m1 * width + m.m3 * height + m.m5;

                orientedBounds.p3.x = m.m0 * x + m.m2 * height + m.m4;
                orientedBounds.p3.y = m.m1 * x + m.m3 * height + m.m5;

                result.orientedBounds = orientedBounds;

                float xMin = float.MaxValue;
                float xMax = float.MinValue;
                float yMin = float.MaxValue;
                float yMax = float.MinValue;

                if (orientedBounds.p0.x < xMin) xMin = orientedBounds.p0.x;
                if (orientedBounds.p1.x < xMin) xMin = orientedBounds.p1.x;
                if (orientedBounds.p2.x < xMin) xMin = orientedBounds.p2.x;
                if (orientedBounds.p3.x < xMin) xMin = orientedBounds.p3.x;

                if (orientedBounds.p0.x > xMax) xMax = orientedBounds.p0.x;
                if (orientedBounds.p1.x > xMax) xMax = orientedBounds.p1.x;
                if (orientedBounds.p2.x > xMax) xMax = orientedBounds.p2.x;
                if (orientedBounds.p3.x > xMax) xMax = orientedBounds.p3.x;

                if (orientedBounds.p0.y < yMin) yMin = orientedBounds.p0.y;
                if (orientedBounds.p1.y < yMin) yMin = orientedBounds.p1.y;
                if (orientedBounds.p2.y < yMin) yMin = orientedBounds.p2.y;
                if (orientedBounds.p3.y < yMin) yMin = orientedBounds.p3.y;

                if (orientedBounds.p0.y > yMax) yMax = orientedBounds.p0.y;
                if (orientedBounds.p1.y > yMax) yMax = orientedBounds.p1.y;
                if (orientedBounds.p2.y > yMax) yMax = orientedBounds.p2.y;
                if (orientedBounds.p3.y > yMax) yMax = orientedBounds.p3.y;

                result.axisAlignedBounds.x = xMin;
                result.axisAlignedBounds.y = yMin;
                result.axisAlignedBounds.z = xMax;
                result.axisAlignedBounds.w = yMax;

                if ((currentElement.layoutBox.flags & LayoutBoxFlags.Clipper) != 0) {
                    currentElement.layoutBox.clipData.orientedBounds = orientedBounds;
                }

                int childCount = currentElement.children.size;

                if (elemRefStack.size + childCount >= elemRefStack.array.Length) {
                    elemRefStack.EnsureAdditionalCapacity(childCount);
                }

                // no need to size check since we are reusing the stack

                for (int childIdx = 0; childIdx < childCount; childIdx++) {
                    UIElement child = currentElement.children.array[childIdx];
                    if ((child.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet) {
                        elemRefStack.array[elemRefStack.size++].element = child;
                    }
                }
            }
        }

        private void RebuildHierarchy() {
            if (hierarchyRebuildList.size == 0) return;

            // do this back to front so parent always works with final children
            LightList<AwesomeLayoutBox> childList = LightList<AwesomeLayoutBox>.Get();

            // input list is already in depth traversal order, so iterating backwards will effectively walk up the leaves
            for (int i = hierarchyRebuildList.size - 1; i >= 0; i--) {
                UIElement element = hierarchyRebuildList.array[i];

                Assert.IsTrue((element.flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet);

                AwesomeLayoutBox elementBox = element.layoutBox;

                LightList<UIElement> elementChildList = element.children;

                for (int j = 0; j < elementChildList.size; j++) {
                    UIElement child = elementChildList.array[j];

                    if ((child.flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet) {
                        continue;
                    }

                    switch (child.style.LayoutBehavior) {
                        // todo -- flag on box instead would be faster
                        case LayoutBehavior.Unset:
                        case LayoutBehavior.Normal:
                            childList.Add(child.layoutBox);
                            break;
                        case LayoutBehavior.Ignored:
                            child.layoutResult.layoutParent = element.layoutResult; // not 100% sure of this
                            ignoredList.Add(child.layoutBox);
                            break;
                        case LayoutBehavior.TranscludeChildren:
                            child.layoutResult.layoutParent = element.layoutResult; // not 100% sure of this
                            child.layoutBox.GetChildren(childList);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                // element.layoutHistory.AddLogEntry(LayoutDirection.Horizontal, frameId, LayoutReason.HierarchyChanged, string.Empty);
                // element.layoutHistory.AddLogEntry(LayoutDirection.Vertical, frameId, LayoutReason.HierarchyChanged, string.Empty);

                elementBox.flags |= (LayoutBoxFlags.RequireLayoutHorizontal | LayoutBoxFlags.RequireLayoutVertical);

                elementBox.cachedContentWidth = -1;
                elementBox.cachedContentHeight = -1;

                elementBox.MarkContentParentsHorizontalDirty(frameId, LayoutReason.DescendentStyleSizeChanged);
                elementBox.MarkContentParentsVerticalDirty(frameId, LayoutReason.DescendentStyleSizeChanged);

                elementBox.SetChildren(childList);
                elementBox.flags &= ~LayoutBoxFlags.GatherChildren;
                element.flags &= ~UIElementFlags.LayoutHierarchyDirty;
                childList.size = 0;
            }

            LightList<AwesomeLayoutBox>.Release(ref childList);
        }

        // public enum QueryFilter {
        //
        //     Hover,
        //     MouseHandler,
        //     DragHandler,
        //     TouchHandler
        //
        // }
        //
        // public void QueryPoint(Vector2 point, QueryFilter filter, IList<UIElement> retn) { }

        public void QueryPoint(Vector2 point, IList<UIElement> retn) {
            Application app = rootElement.application;

            if (!new Rect(0, 0, app.Width, app.Height).Contains(point) || rootElement.isDisabled) {
                return;
            }

            for (int i = 0; i < queryableElements.size; i++) {
                UIElement element = queryableElements.array[i].element;
                if (element is IPointerQueryHandler pointerQueryHandler) {
                    if (pointerQueryHandler.ContainsPoint(point)) {
                        retn.Add(element);
                    }

                    continue;
                }

                LayoutResult layoutResult = element.layoutResult;

                // todo - for some reason the layoutbox can be null. matt, pls fix k thx bye
                if (layoutResult.isCulled || element.layoutBox == null) {
                    continue;
                }

                ClipData ptr = layoutResult.clipper;

                bool pointVisibleInClipperHierarchy = true;

                while (ptr != null) {
                    if (!ptr.ContainsPoint(point)) {
                        pointVisibleInClipperHierarchy = false;
                        break;
                    }

                    ptr = ptr.parent;
                }

                if (!pointVisibleInClipperHierarchy) {
                    continue;
                }

                if (layoutResult.actualSize.width == 0 || layoutResult.actualSize.height == 0) {
                    continue;
                }

                if (PolygonUtil.PointInOrientedBounds(point, layoutResult.orientedBounds)) {
                    // todo -- make this property look up not slow
                    if (element.style.Visibility == Visibility.Hidden) {
                        continue;
                    }

                    if (element.style.PointerEvents == PointerEvents.None) {
                        continue;
                    }

                    retn.Add(element);
                }
            }
        }

    }

}