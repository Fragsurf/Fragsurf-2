using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Text;
using UIForia.UIInput;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Rendering {

    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("type = {propertyId.ToString()}")]
    public readonly partial struct StyleProperty {

        [FieldOffset(0)] internal readonly object objectField; // must be at offset 0 8 or 16 because of alignment, cannot overlap non reference types

        [FieldOffset(8)] public readonly StylePropertyId propertyId;

        [FieldOffset(10)] internal readonly ushort flags;

        [FieldOffset(12)] internal readonly int int0;
        [FieldOffset(12)] internal readonly long longVal;

        [FieldOffset(12)] internal readonly float float0;

        [FieldOffset(16)] internal readonly int int1; // can merge this with flags (except for 1 bit at end) if we make sure each measurement type fits in ushort - 1 bit 

        [FieldOffset(16)] internal readonly int float1; // unused atm. if continues to be unused can merge this with flags (except for 1 bit at end) if we make sure each measurement type fits in ushort - 1 bit 

        public bool hasValue => flags != 0;

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId) : this() {
            this.propertyId = propertyId;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in Color color) : this() {
            this.propertyId = propertyId;
            this.int0 = new StyleColor(color).rgba;
            this.int1 = 1;
            this.flags = 1;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in MaterialId materialId) : this() {
            this.propertyId = propertyId;
            this.longVal = materialId.id;
            this.flags = 1;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in MaterialId? materialId) : this() {
            this.propertyId = propertyId;
            if (materialId.HasValue) {
                this.int0 = (int) materialId.Value;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in Color? color) : this() {
            this.propertyId = propertyId;
            if (color.HasValue) {
                this.int0 = new StyleColor(color.Value).rgba;
                this.int1 = 1;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in UIFixedLength length) : this() {
            this.propertyId = propertyId;
            this.flags = 1;
            this.float0 = length.value;
            this.int1 = (int) length.unit;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in UIFixedLength? length) : this() {
            this.propertyId = propertyId;
            if (length.HasValue) {
                UIFixedLength v = length.Value;
                this.float0 = v.value;
                this.int1 = (int) v.unit;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in UIMeasurement measurement) : this() {
            this.propertyId = propertyId;
            this.flags = 1;
            this.float0 = measurement.value;
            this.int1 = (int) measurement.unit;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in UIMeasurement? measurement) : this() {
            this.propertyId = propertyId;
            if (measurement.HasValue) {
                UIMeasurement val = measurement.Value;
                this.float0 = val.value;
                this.int1 = (int) val.unit;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in OffsetMeasurement offset) : this() {
            this.propertyId = propertyId;
            this.flags = 1;
            this.float0 = offset.value;
            this.int1 = (int) offset.unit;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in OffsetMeasurement? offset) : this() {
            this.propertyId = propertyId;
            if (offset.HasValue) {
                OffsetMeasurement val = offset.Value;
                this.float0 = val.value;
                this.int1 = (int) val.unit;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, float float0) : this() {
            this.propertyId = propertyId;
            this.flags = 1;
            this.objectField = null;
            this.float0 = float0;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in float? floatValue) : this() {
            this.propertyId = propertyId;
            this.objectField = null;
            if (floatValue.HasValue) {
                this.float0 = floatValue.Value;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int intValue) : this() {
            this.propertyId = propertyId;
            this.flags = 1;
            this.objectField = null;
            this.int0 = intValue;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, int? intValue) : this() {
            this.propertyId = propertyId;
            this.objectField = null;
            if (intValue.HasValue) {
                this.int0 = intValue.Value;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, in GridItemPlacement placement) : this() {
            this.propertyId = propertyId;
            this.int0 = placement.index;
            this.int1 = 0;
            this.flags = 1;
            this.objectField = placement.name;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, GridItemPlacement? placement) : this() {
            this.propertyId = propertyId;
            this.objectField = null;
            if (placement.HasValue) {
                GridItemPlacement val = placement.Value;
                this.objectField = val.name;
                this.int0 = val.index;
                this.flags = 1;
            }
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, string objectField) : this() {
            this.propertyId = propertyId;
            this.flags = objectField != null ? (ushort) 1 : (ushort) 0;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, Texture objectField) : this() {
            this.propertyId = propertyId;
            this.flags = !ReferenceEquals(objectField, null) ? (ushort) 1 : (ushort) 0;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, CursorStyle objectField) : this() {
            this.propertyId = propertyId;
            this.flags = objectField != null ? (ushort) 1 : (ushort) 0;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, IReadOnlyList<GridTrackSize> objectField) : this() {
            this.propertyId = propertyId;
            this.flags = objectField != null ? (ushort) 1 : (ushort) 0;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, FontAsset objectField) : this() {
            this.propertyId = propertyId;
            this.flags = objectField != null ? (ushort) 1 : (ushort) 0;
            this.objectField = objectField;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, MeshType meshType) : this() {
            this.propertyId = propertyId;
            this.int0 = (int) meshType;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, MeshFillOrigin meshFillOrigin) : this() {
            this.propertyId = propertyId;
            this.int0 = (int) meshFillOrigin;
        }

        [DebuggerStepThrough]
        public StyleProperty(StylePropertyId propertyId, MeshFillDirection meshFillDirection) : this() {
            this.propertyId = propertyId;
            this.int0 = (int) meshFillDirection;
        }

        public WhitespaceMode AsWhitespaceMode => (WhitespaceMode) int0;

        public MeshType AsMeshType => (MeshType) int0;
        public MeshFillOrigin AsMeshFillOrigin => (MeshFillOrigin) int0;
        public MeshFillDirection AsMeshFillDirection => (MeshFillDirection) int0;

        public MaterialId AsMaterialId => (MaterialId) longVal;
        public int AsInt => int0;
        public float AsFloat => float0;
        public GridAxisAlignment AsGridAxisAlignment => (GridAxisAlignment) int0;
        public SpaceDistribution AsSpaceDistribution => (SpaceDistribution) int0;
        public AlignmentBoundary AsAlignmentBoundary => (AlignmentBoundary) int0;
        public Overflow AsOverflow => (Overflow) int0;
        public ClipBehavior AsClipBehavior => (ClipBehavior) int0;
        public Color AsColor => new StyleColor(int0);

        public FontAsset AsFont => (FontAsset) objectField;
        public Texture2D AsTexture => (Texture2D) objectField;

        public FontStyle AsFontStyle => (FontStyle) int0;
        public TextAlignment AsTextAlignment => (TextAlignment) int0;
        public LayoutDirection AsLayoutDirection => (LayoutDirection) int0;
        public LayoutWrap AsLayoutWrap => (LayoutWrap) int0;

        public UIMeasurement AsUIMeasurement => new UIMeasurement(float0, (UIMeasurementUnit) int1);
        public UIFixedLength AsUIFixedLength => new UIFixedLength(float0, (UIFixedUnit) int1);
        public OffsetMeasurement AsOffsetMeasurement => new OffsetMeasurement(float0, (OffsetMeasurementUnit) int1);

        public GridItemPlacement AsGridItemPlacement {
            get {
                if (objectField is string name) {
                    return new GridItemPlacement(name);
                }

                return new GridItemPlacement(int0);
            }
        }

        public IReadOnlyList<GridTrackSize> AsGridTrackTemplate => (IReadOnlyList<GridTrackSize>) objectField;

        public RenderLayer AsRenderLayer => (RenderLayer) int0;
        public Texture2D AsTexture2D => (Texture2D) objectField;
        public GridLayoutDensity AsGridLayoutDensity => (GridLayoutDensity) int0;
        public LayoutType AsLayoutType => (LayoutType) int0;
        public TextTransform AsTextTransform => (TextTransform) int0;
        public LayoutBehavior AsLayoutBehavior => (LayoutBehavior) int0;
        public ScrollBehavior AsScrollBehavior => (ScrollBehavior)int0;

        public IReadOnlyList<GridTrackSize> AsGridTemplate => (IReadOnlyList<GridTrackSize>) objectField;
        public Visibility AsVisibility => (Visibility) int0;
        public CursorStyle AsCursorStyle => (CursorStyle) objectField;
        public string AsString => (string) objectField;
        public UnderlayType AsUnderlayType => (UnderlayType) int0;
        public LayoutFit AsLayoutFit => (LayoutFit) int0;
        public BackgroundFit AsBackgroundFit => (BackgroundFit) int0;
        public ClipBounds AsClipBounds => (ClipBounds) int0;
        public PointerEvents AsPointerEvents => (PointerEvents) int0;

        public AlignmentTarget AsAlignmentTarget => (AlignmentTarget) int0;
        public AlignmentDirection AsAlignmentDirection => (AlignmentDirection) int0;

        public static bool operator ==(in StyleProperty a, in StyleProperty b) {
            return a.propertyId == b.propertyId &&
                   a.int0 == b.int0 &&
                   a.int1 == b.int1 &&
                   a.flags == b.flags &&
                   a.objectField == b.objectField;
        }

        public static bool operator !=(in StyleProperty a, in StyleProperty b) {
            return a.propertyId != b.propertyId ||
                   a.int0 != b.int0 ||
                   a.int1 != b.int1 ||
                   a.flags != b.flags ||
                   a.objectField != b.objectField;
        }

        public bool Equals(in StyleProperty other) {
            return propertyId == other.propertyId &&
                   int0 == other.int0 &&
                   int1 == other.int1 &&
                   flags == other.flags &&
                   objectField == other.objectField;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is StyleProperty property && Equals(property);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (int) propertyId;
                hashCode = (hashCode * 397) ^ int0;
                hashCode = (hashCode * 397) ^ int1;
                hashCode = (hashCode * 397) ^ flags;
                hashCode = (hashCode * 397) ^ (objectField != null ? objectField.GetHashCode() : 0);
                return hashCode;
            }
        }

        [DebuggerStepThrough]
        public static StyleProperty Unset(StylePropertyId propertyId) {
            StyleProperty retn = new StyleProperty(propertyId);
            return retn;
        }

        public static StyleProperty TransformPositionX(OffsetMeasurement length) {
            return new StyleProperty(StylePropertyId.TransformPositionX, length);
        }

        public static StyleProperty TransformPositionY(OffsetMeasurement length) {
            return new StyleProperty(StylePropertyId.TransformPositionY, length);
        }

        public static StyleProperty TransformPivotX(UIFixedLength length) {
            return new StyleProperty(StylePropertyId.TransformPivotX, length);
        }

        public static StyleProperty TransformPivotY(UIFixedLength length) {
            return new StyleProperty(StylePropertyId.TransformPivotY, length);
        }

        public static StyleProperty TransformScaleX(float scaleX) {
            return new StyleProperty(StylePropertyId.TransformScaleX, scaleX);
        }

        public static StyleProperty TransformScaleY(float scaleY) {
            return new StyleProperty(StylePropertyId.TransformScaleY, scaleY);
        }

        public static StyleProperty TransformRotation(float rotation) {
            return new StyleProperty(StylePropertyId.TransformRotation, rotation);
        }

        public static StyleProperty BackgroundColor(Color color) {
            return new StyleProperty(StylePropertyId.BackgroundColor, color);
        }

        public static StyleProperty BackgroundTint(Color color) {
            return new StyleProperty(StylePropertyId.BackgroundTint, color);
        }

        public static StyleProperty BackgroundImage(Texture2D texture) {
            return new StyleProperty(StylePropertyId.BackgroundImage, texture);
        }

        public static StyleProperty Opacity(float opacity) {
            return new StyleProperty(StylePropertyId.Opacity, opacity);
        }

        public static StyleProperty Cursor(Texture2D texture) {
            return new StyleProperty(StylePropertyId.Cursor, texture);
        }

        public static StyleProperty GridItemColStart(int colStart) {
            return new StyleProperty(StylePropertyId.GridItemY, colStart);
        }

        public static StyleProperty GridItemColSpan(int colSpan) {
            return new StyleProperty(StylePropertyId.GridItemHeight, colSpan);
        }

        public static StyleProperty GridItemRowStart(int rowStart) {
            return new StyleProperty(StylePropertyId.GridItemX, rowStart);
        }

        public static StyleProperty GridItemRowSpan(int rowSpan) {
            return new StyleProperty(StylePropertyId.GridItemWidth, rowSpan);
        }

        public static StyleProperty GridLayoutDensity(GridLayoutDensity density) {
            return new StyleProperty(StylePropertyId.GridLayoutDensity, (int) density);
        }

        public static StyleProperty GridLayoutColTemplate(IReadOnlyList<GridTrackSize> colTemplate) {
            return new StyleProperty(StylePropertyId.GridLayoutColTemplate, colTemplate);
        }

        public static StyleProperty GridLayoutRowTemplate(IReadOnlyList<GridTrackSize> rowTemplate) {
            return new StyleProperty(StylePropertyId.GridLayoutRowTemplate, rowTemplate);
        }

        public static StyleProperty GridLayoutDirection(LayoutDirection direction) {
            return new StyleProperty(StylePropertyId.GridLayoutDirection, (int) direction);
        }

        public static StyleProperty GridLayoutColAutoSize(IReadOnlyList<GridTrackSize> autoColSize) {
            return new StyleProperty(StylePropertyId.GridLayoutColAutoSize, autoColSize);
        }

        public static StyleProperty GridLayoutRowAutoSize(IReadOnlyList<GridTrackSize> autoRowSize) {
            return new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, autoRowSize);
        }

        public static StyleProperty GridLayoutColGap(float colGap) {
            return new StyleProperty(StylePropertyId.GridLayoutColGap, colGap);
        }

        public static StyleProperty GridLayoutRowGap(float rowGap) {
            return new StyleProperty(StylePropertyId.GridLayoutRowGap, rowGap);
        }

        public static StyleProperty GridLayoutColAlignment(CrossAxisAlignment alignment) {
            return new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int) alignment);
        }

        public static StyleProperty GridLayoutRowAlignment(CrossAxisAlignment alignment) {
            return new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int) alignment);
        }

        public static StyleProperty MarginTop(UIMeasurement marginTop) {
            return new StyleProperty(StylePropertyId.MarginTop, marginTop);
        }

        public static StyleProperty MarginRight(UIMeasurement marginRight) {
            return new StyleProperty(StylePropertyId.MarginRight, marginRight);
        }

        public static StyleProperty MarginBottom(UIMeasurement marginBottom) {
            return new StyleProperty(StylePropertyId.MarginBottom, marginBottom);
        }

        public static StyleProperty MarginLeft(UIMeasurement marginLeft) {
            return new StyleProperty(StylePropertyId.MarginLeft, marginLeft);
        }

        public static StyleProperty BorderTop(UIFixedLength borderTop) {
            return new StyleProperty(StylePropertyId.BorderTop, borderTop);
        }

        public static StyleProperty BorderRight(UIFixedLength borderRight) {
            return new StyleProperty(StylePropertyId.BorderRight, borderRight);
        }

        public static StyleProperty BorderBottom(UIFixedLength borderBottom) {
            return new StyleProperty(StylePropertyId.BorderBottom, borderBottom);
        }

        public static StyleProperty BorderLeft(UIFixedLength borderLeft) {
            return new StyleProperty(StylePropertyId.BorderLeft, borderLeft);
        }

        public static StyleProperty PaddingTop(UIFixedLength paddingTop) {
            return new StyleProperty(StylePropertyId.PaddingTop, paddingTop);
        }

        public static StyleProperty PaddingRight(UIFixedLength paddingRight) {
            return new StyleProperty(StylePropertyId.PaddingRight, paddingRight);
        }

        public static StyleProperty PaddingBottom(UIFixedLength paddingBottom) {
            return new StyleProperty(StylePropertyId.PaddingBottom, paddingBottom);
        }

        public static StyleProperty PaddingLeft(UIFixedLength paddingLeft) {
            return new StyleProperty(StylePropertyId.PaddingLeft, paddingLeft);
        }

        public static StyleProperty BorderRadiusTopLeft(UIFixedLength topLeftRadius) {
            return new StyleProperty(StylePropertyId.BorderRadiusTopLeft, topLeftRadius);
        }

        public static StyleProperty BorderRadiusTopRight(UIFixedLength topRightRadius) {
            return new StyleProperty(StylePropertyId.BorderRadiusTopRight, topRightRadius);
        }

        public static StyleProperty BorderRadiusBottomLeft(UIFixedLength bottomLeftRadius) {
            return new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, bottomLeftRadius);
        }

        public static StyleProperty BorderRadiusBottomRight(UIFixedLength bottomRightRadius) {
            return new StyleProperty(StylePropertyId.BorderRadiusBottomRight, bottomRightRadius);
        }

        public static StyleProperty MinWidth(UIMeasurement minWidth) {
            return new StyleProperty(StylePropertyId.MinWidth, minWidth);
        }

        public static StyleProperty MaxWidth(UIMeasurement maxWidth) {
            return new StyleProperty(StylePropertyId.MaxWidth, maxWidth);
        }

        public static StyleProperty PreferredWidth(UIMeasurement preferredWidth) {
            return new StyleProperty(StylePropertyId.PreferredWidth, preferredWidth);
        }

        public static StyleProperty MinHeight(UIMeasurement minHeight) {
            return new StyleProperty(StylePropertyId.MinHeight, minHeight);
        }

        public static StyleProperty MaxHeight(UIMeasurement maxHeight) {
            return new StyleProperty(StylePropertyId.MaxHeight, maxHeight);
        }

        public static StyleProperty PreferredHeight(UIMeasurement preferredHeight) {
            return new StyleProperty(StylePropertyId.PreferredHeight, preferredHeight);
        }

        public static StyleProperty ZIndex(int zIndex) {
            return new StyleProperty(StylePropertyId.ZIndex, zIndex);
        }

        public static StyleProperty LayerOffset(int layerOffset) {
            return new StyleProperty(StylePropertyId.RenderLayerOffset, layerOffset);
        }

        public static StyleProperty RenderLayer(RenderLayer renderLayer) {
            return new StyleProperty(StylePropertyId.RenderLayer, (int) renderLayer);
        }

        public static StyleProperty Font(FontAsset fontAsset) {
            return new StyleProperty(StylePropertyId.TextFontAsset, fontAsset);
        }

    }

}