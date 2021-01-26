using System;
using SVGX;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout {

    public class LayoutResult {

        public float localRotation;
        public Vector2 localScale;
        public Vector2 localPosition;

        public Vector2 scale;
        public Vector2 screenPosition;
        public Vector2 pivot;

        public Size actualSize;
        public Size allocatedSize;

        public float rotation;

        public OffsetRect border;
        public OffsetRect padding;
        public OffsetRect margin;

        public SVGXMatrix matrix;
        public SVGXMatrix localMatrix;
        public Vector2 pivotOffset;

        public Vector2 allocatedPosition; // where the parent told this element to be

        public Vector2 alignedPosition; // where the element wants to be (might be relative to allocated, might not be) 
        // local position = actual position post transform

        public Rect ScreenRect => new Rect(screenPosition, new Vector2(actualSize.width, actualSize.height));
        public Rect AllocatedRect => new Rect(allocatedPosition, new Vector2(allocatedSize.width, allocatedSize.height));

        public Rect LocalRect => new Rect(alignedPosition, new Vector2(actualSize.width, actualSize.height));

        public float AllocatedWidth => allocatedSize.width; // this should be size with padding & border already subtracted
        public float AllocatedHeight => allocatedSize.height;

        public float ActualWidth => actualSize.width;
        public float ActualHeight => actualSize.height;

        public float ContentAreaWidth => actualSize.width - padding.left - border.left - padding.right - border.right;
        public float ContentAreaHeight => actualSize.height - padding.top - border.top - padding.bottom - border.bottom;

        public LayoutResult layoutParent;
        public UIElement element;
        public OrientedBounds orientedBounds;
        public Vector4 axisAlignedBounds;

        internal ClipData clipper;
        public bool isCulled;
        public bool rebuildGeometry;

        public Rect ContentRect => new Rect(
            padding.left + border.left,
            padding.top + border.top,
            actualSize.width - padding.left - border.left - padding.right - border.right,
            actualSize.height - padding.top - border.top - padding.bottom - border.bottom
        );

        public float VerticalPaddingBorderStart => padding.top + border.top;
        public float VerticalPaddingBorderEnd => padding.bottom + border.bottom;
        public float HorizontalPaddingBorderStart => padding.left + border.left;
        public float HorizontalPaddingBorderEnd => padding.right + border.right;

        internal LayoutResult(UIElement element) {
            this.element = element;
            this.matrix = SVGXMatrix.identity;
        }

    }

}