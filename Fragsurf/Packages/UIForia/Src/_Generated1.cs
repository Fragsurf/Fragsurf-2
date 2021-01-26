
using UIForia.Util;

namespace UIForia.Rendering {
    
    public partial struct UIStyleSetStateProxy {
        
        public UIForia.Rendering.Visibility Visibility {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Visibility, state).AsVisibility; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Visibility, (int)value), state); }
        }
        
        public float Opacity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Opacity, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Opacity, value), state); }
        }
        
        public UIForia.Rendering.CursorStyle Cursor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Cursor, state).AsCursorStyle; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Cursor, value), state); }
        }
        
        public string Painter {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Painter, state).AsString; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Painter, value), state); }
        }
        
        public UIForia.Rendering.MaterialId Material {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Material, state).AsMaterialId; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Material, value), state); }
        }
        
        public UIForia.Rendering.MeshType MeshType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MeshType, state).AsMeshType; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MeshType, (int)value), state); }
        }
        
        public UIForia.Rendering.MeshFillDirection MeshFillDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MeshFillDirection, state).AsMeshFillDirection; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MeshFillDirection, (int)value), state); }
        }
        
        public UIForia.Rendering.MeshFillOrigin MeshFillOrigin {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MeshFillOrigin, state).AsMeshFillOrigin; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MeshFillOrigin, (int)value), state); }
        }
        
        public float MeshFillAmount {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MeshFillAmount, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MeshFillAmount, value), state); }
        }
        
        public UIForia.Rendering.Overflow OverflowX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.OverflowX, state).AsOverflow; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.OverflowX, (int)value), state); }
        }
        
        public UIForia.Rendering.Overflow OverflowY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.OverflowY, state).AsOverflow; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.OverflowY, (int)value), state); }
        }
        
        public UIForia.Layout.ClipBehavior ClipBehavior {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ClipBehavior, state).AsClipBehavior; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ClipBehavior, (int)value), state); }
        }
        
        public UIForia.Rendering.ClipBounds ClipBounds {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ClipBounds, state).AsClipBounds; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ClipBounds, (int)value), state); }
        }
        
        public UIForia.UIInput.PointerEvents PointerEvents {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PointerEvents, state).AsPointerEvents; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PointerEvents, (int)value), state); }
        }
        
        public UnityEngine.Color BackgroundColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value), state); }
        }
        
        public UnityEngine.Color BackgroundTint {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundTint, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundTint, value), state); }
        }
        
        public UIForia.UIFixedLength BackgroundImageOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetX, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetX, value), state); }
        }
        
        public UIForia.UIFixedLength BackgroundImageOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetY, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetY, value), state); }
        }
        
        public float BackgroundImageScaleX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageScaleX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleX, value), state); }
        }
        
        public float BackgroundImageScaleY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageScaleY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleY, value), state); }
        }
        
        public float BackgroundImageTileX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageTileX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileX, value), state); }
        }
        
        public float BackgroundImageTileY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageTileY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileY, value), state); }
        }
        
        public float BackgroundImageRotation {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImageRotation, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImageRotation, value), state); }
        }
        
        public UnityEngine.Texture2D BackgroundImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundImage, state).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, value), state); }
        }
        
        public UIForia.Rendering.BackgroundFit BackgroundFit {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BackgroundFit, state).AsBackgroundFit; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BackgroundFit, (int)value), state); }
        }
        
        public UnityEngine.Color BorderColorTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorTop, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorTop, value), state); }
        }
        
        public UnityEngine.Color BorderColorRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorRight, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorRight, value), state); }
        }
        
        public UnityEngine.Color BorderColorBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorBottom, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorBottom, value), state); }
        }
        
        public UnityEngine.Color BorderColorLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderColorLeft, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderColorLeft, value), state); }
        }
        
        public UIForia.UIFixedLength CornerBevelTopLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.CornerBevelTopLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.CornerBevelTopLeft, value), state); }
        }
        
        public UIForia.UIFixedLength CornerBevelTopRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.CornerBevelTopRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.CornerBevelTopRight, value), state); }
        }
        
        public UIForia.UIFixedLength CornerBevelBottomRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.CornerBevelBottomRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.CornerBevelBottomRight, value), state); }
        }
        
        public UIForia.UIFixedLength CornerBevelBottomLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.CornerBevelBottomLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.CornerBevelBottomLeft, value), state); }
        }
        
        public int FlexItemGrow {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexItemGrow, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexItemGrow, value), state); }
        }
        
        public int FlexItemShrink {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexItemShrink, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexItemShrink, value), state); }
        }
        
        public UIForia.Layout.LayoutDirection FlexLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutDirection, state).AsLayoutDirection; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value), state); }
        }
        
        public UIForia.Layout.LayoutWrap FlexLayoutWrap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutWrap, state).AsLayoutWrap; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)value), state); }
        }
        
        public float FlexLayoutGapHorizontal {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutGapHorizontal, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutGapHorizontal, value), state); }
        }
        
        public float FlexLayoutGapVertical {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FlexLayoutGapVertical, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FlexLayoutGapVertical, value), state); }
        }
        
        public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemX, state).AsGridItemPlacement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemX, value), state); }
        }
        
        public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemY, state).AsGridItemPlacement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemY, value), state); }
        }
        
        public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemWidth, state).AsGridItemPlacement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemWidth, value), state); }
        }
        
        public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridItemHeight, state).AsGridItemPlacement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridItemHeight, value), state); }
        }
        
        public UIForia.Layout.LayoutDirection GridLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutDirection, state).AsLayoutDirection; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutDirection, (int)value), state); }
        }
        
        public UIForia.Layout.GridLayoutDensity GridLayoutDensity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutDensity, state).AsGridLayoutDensity; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutDensity, (int)value), state); }
        }
        
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutColTemplate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColTemplate, state).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, value), state); }
        }
        
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowTemplate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowTemplate, state).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, value), state); }
        }
        
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutColAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColAutoSize, state).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAutoSize, value), state); }
        }
        
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowAutoSize, state).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, value), state); }
        }
        
        public float GridLayoutColGap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColGap, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColGap, value), state); }
        }
        
        public float GridLayoutRowGap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowGap, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowGap, value), state); }
        }
        
        public UIForia.Layout.GridAxisAlignment GridLayoutColAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutColAlignment, state).AsGridAxisAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)value), state); }
        }
        
        public UIForia.Layout.GridAxisAlignment GridLayoutRowAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.GridLayoutRowAlignment, state).AsGridAxisAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)value), state); }
        }
        
        public float AlignItemsHorizontal {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignItemsHorizontal, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignItemsHorizontal, value), state); }
        }
        
        public float AlignItemsVertical {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignItemsVertical, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignItemsVertical, value), state); }
        }
        
        public UIForia.Layout.LayoutFit FitItemsVertical {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FitItemsVertical, state).AsLayoutFit; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FitItemsVertical, (int)value), state); }
        }
        
        public UIForia.Layout.LayoutFit FitItemsHorizontal {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.FitItemsHorizontal, state).AsLayoutFit; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.FitItemsHorizontal, (int)value), state); }
        }
        
        public UIForia.Layout.SpaceDistribution DistributeExtraSpaceHorizontal {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.DistributeExtraSpaceHorizontal, state).AsSpaceDistribution; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.DistributeExtraSpaceHorizontal, (int)value), state); }
        }
        
        public UIForia.Layout.SpaceDistribution DistributeExtraSpaceVertical {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.DistributeExtraSpaceVertical, state).AsSpaceDistribution; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.DistributeExtraSpaceVertical, (int)value), state); }
        }
        
        public float RadialLayoutStartAngle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RadialLayoutStartAngle, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RadialLayoutStartAngle, value), state); }
        }
        
        public float RadialLayoutEndAngle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RadialLayoutEndAngle, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RadialLayoutEndAngle, value), state); }
        }
        
        public UIForia.UIFixedLength RadialLayoutRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RadialLayoutRadius, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RadialLayoutRadius, value), state); }
        }
        
        public UIForia.Layout.AlignmentDirection AlignmentDirectionX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentDirectionX, state).AsAlignmentDirection; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentDirectionX, (int)value), state); }
        }
        
        public UIForia.Layout.AlignmentDirection AlignmentDirectionY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentDirectionY, state).AsAlignmentDirection; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentDirectionY, (int)value), state); }
        }
        
        public UIForia.Layout.AlignmentTarget AlignmentTargetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentTargetX, state).AsAlignmentTarget; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetX, (int)value), state); }
        }
        
        public UIForia.Layout.AlignmentTarget AlignmentTargetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentTargetY, state).AsAlignmentTarget; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetY, (int)value), state); }
        }
        
        public UIForia.OffsetMeasurement AlignmentOriginX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentOriginX, state).AsOffsetMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentOriginX, value), state); }
        }
        
        public UIForia.OffsetMeasurement AlignmentOriginY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentOriginY, state).AsOffsetMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentOriginY, value), state); }
        }
        
        public UIForia.OffsetMeasurement AlignmentOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentOffsetX, state).AsOffsetMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetX, value), state); }
        }
        
        public UIForia.OffsetMeasurement AlignmentOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentOffsetY, state).AsOffsetMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetY, value), state); }
        }
        
        public UIForia.Layout.AlignmentBoundary AlignmentBoundaryX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentBoundaryX, state).AsAlignmentBoundary; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentBoundaryX, (int)value), state); }
        }
        
        public UIForia.Layout.AlignmentBoundary AlignmentBoundaryY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.AlignmentBoundaryY, state).AsAlignmentBoundary; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.AlignmentBoundaryY, (int)value), state); }
        }
        
        public UIForia.Layout.LayoutFit LayoutFitHorizontal {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.LayoutFitHorizontal, state).AsLayoutFit; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.LayoutFitHorizontal, (int)value), state); }
        }
        
        public UIForia.Layout.LayoutFit LayoutFitVertical {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.LayoutFitVertical, state).AsLayoutFit; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.LayoutFitVertical, (int)value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MinWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MinWidth, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MinWidth, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MaxWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MaxWidth, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement PreferredWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PreferredWidth, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MinHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MinHeight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MinHeight, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement MaxHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MaxHeight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value), state); }
        }
        
        public UIForia.Rendering.UIMeasurement PreferredHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PreferredHeight, state).AsUIMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value), state); }
        }
        
        public UIForia.UIFixedLength MarginTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginTop, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginTop, value), state); }
        }
        
        public UIForia.UIFixedLength MarginRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginRight, value), state); }
        }
        
        public UIForia.UIFixedLength MarginBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginBottom, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value), state); }
        }
        
        public UIForia.UIFixedLength MarginLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.MarginLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value), state); }
        }
        
        public UIForia.UIFixedLength BorderTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderTop, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderTop, value), state); }
        }
        
        public UIForia.UIFixedLength BorderRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRight, value), state); }
        }
        
        public UIForia.UIFixedLength BorderBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderBottom, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderBottom, value), state); }
        }
        
        public UIForia.UIFixedLength BorderLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderLeft, value), state); }
        }
        
        public UIForia.UIFixedLength BorderRadiusTopLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusTopLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopLeft, value), state); }
        }
        
        public UIForia.UIFixedLength BorderRadiusTopRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusTopRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopRight, value), state); }
        }
        
        public UIForia.UIFixedLength BorderRadiusBottomRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusBottomRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomRight, value), state); }
        }
        
        public UIForia.UIFixedLength BorderRadiusBottomLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.BorderRadiusBottomLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, value), state); }
        }
        
        public UIForia.UIFixedLength PaddingTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingTop, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingTop, value), state); }
        }
        
        public UIForia.UIFixedLength PaddingRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingRight, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingRight, value), state); }
        }
        
        public UIForia.UIFixedLength PaddingBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingBottom, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingBottom, value), state); }
        }
        
        public UIForia.UIFixedLength PaddingLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.PaddingLeft, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.PaddingLeft, value), state); }
        }
        
        public UnityEngine.Color TextColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextColor, value), state); }
        }
        
        public UIForia.FontAsset TextFontAsset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontAsset, state).AsFont; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, value), state); }
        }
        
        public UIForia.UIFixedLength TextFontSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontSize, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFontSize, value), state); }
        }
        
        public UIForia.Text.FontStyle TextFontStyle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFontStyle, state).AsFontStyle; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFontStyle, (int)value), state); }
        }
        
        public UIForia.Text.TextAlignment TextAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextAlignment, state).AsTextAlignment; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextAlignment, (int)value), state); }
        }
        
        public float TextOutlineWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextOutlineWidth, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextOutlineWidth, value), state); }
        }
        
        public UnityEngine.Color TextOutlineColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextOutlineColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextOutlineColor, value), state); }
        }
        
        public UnityEngine.Color CaretColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.CaretColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.CaretColor, value), state); }
        }
        
        public UnityEngine.Color SelectionBackgroundColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.SelectionBackgroundColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.SelectionBackgroundColor, value), state); }
        }
        
        public UnityEngine.Color SelectionTextColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.SelectionTextColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.SelectionTextColor, value), state); }
        }
        
        public float TextOutlineSoftness {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextOutlineSoftness, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextOutlineSoftness, value), state); }
        }
        
        public UnityEngine.Color TextGlowColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowColor, value), state); }
        }
        
        public float TextGlowOffset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowOffset, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowOffset, value), state); }
        }
        
        public float TextGlowInner {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowInner, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowInner, value), state); }
        }
        
        public float TextGlowOuter {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowOuter, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowOuter, value), state); }
        }
        
        public float TextGlowPower {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextGlowPower, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextGlowPower, value), state); }
        }
        
        public UnityEngine.Color TextUnderlayColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayColor, value), state); }
        }
        
        public float TextUnderlayX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayX, value), state); }
        }
        
        public float TextUnderlayY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayY, value), state); }
        }
        
        public float TextUnderlayDilate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayDilate, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayDilate, value), state); }
        }
        
        public float TextUnderlaySoftness {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlaySoftness, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlaySoftness, value), state); }
        }
        
        public float TextFaceDilate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextFaceDilate, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextFaceDilate, value), state); }
        }
        
        public UIForia.Rendering.UnderlayType TextUnderlayType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextUnderlayType, state).AsUnderlayType; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextUnderlayType, (int)value), state); }
        }
        
        public UIForia.Text.TextTransform TextTransform {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextTransform, state).AsTextTransform; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value), state); }
        }
        
        public UIForia.Text.WhitespaceMode TextWhitespaceMode {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TextWhitespaceMode, state).AsWhitespaceMode; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)value), state); }
        }
        
        public UIForia.OffsetMeasurement TransformPositionX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPositionX, state).AsOffsetMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value), state); }
        }
        
        public UIForia.OffsetMeasurement TransformPositionY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPositionY, state).AsOffsetMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value), state); }
        }
        
        public UIForia.UIFixedLength TransformPivotX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPivotX, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPivotX, value), state); }
        }
        
        public UIForia.UIFixedLength TransformPivotY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformPivotY, state).AsUIFixedLength; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformPivotY, value), state); }
        }
        
        public float TransformScaleX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformScaleX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformScaleX, value), state); }
        }
        
        public float TransformScaleY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformScaleY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformScaleY, value), state); }
        }
        
        public float TransformRotation {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.TransformRotation, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.TransformRotation, value), state); }
        }
        
        public UIForia.Layout.LayoutType LayoutType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.LayoutType, state).AsLayoutType; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.LayoutType, (int)value), state); }
        }
        
        public UIForia.Layout.LayoutBehavior LayoutBehavior {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.LayoutBehavior, state).AsLayoutBehavior; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.LayoutBehavior, (int)value), state); }
        }
        
        public UIForia.Layout.ScrollBehavior ScrollBehaviorX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollBehaviorX, state).AsScrollBehavior; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollBehaviorX, (int)value), state); }
        }
        
        public UIForia.Layout.ScrollBehavior ScrollBehaviorY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ScrollBehaviorY, state).AsScrollBehavior; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ScrollBehaviorY, (int)value), state); }
        }
        
        public int ZIndex {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ZIndex, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ZIndex, value), state); }
        }
        
        public int RenderLayerOffset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RenderLayerOffset, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RenderLayerOffset, value), state); }
        }
        
        public UIForia.Rendering.RenderLayer RenderLayer {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.RenderLayer, state).AsRenderLayer; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.RenderLayer, (int)value), state); }
        }
        
        public int Layer {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.Layer, state).AsInt; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.Layer, value), state); }
        }
        
        public UnityEngine.Color ShadowColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowColor, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowColor, value), state); }
        }
        
        public UnityEngine.Color ShadowTint {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowTint, state).AsColor; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowTint, value), state); }
        }
        
        public UIForia.OffsetMeasurement ShadowOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowOffsetX, state).AsOffsetMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetX, value), state); }
        }
        
        public UIForia.OffsetMeasurement ShadowOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowOffsetY, state).AsOffsetMeasurement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetY, value), state); }
        }
        
        public float ShadowSizeX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowSizeX, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowSizeX, value), state); }
        }
        
        public float ShadowSizeY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowSizeY, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowSizeY, value), state); }
        }
        
        public float ShadowIntensity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowIntensity, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowIntensity, value), state); }
        }
        
        public float ShadowOpacity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.ShadowOpacity, state).AsFloat; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(new StyleProperty(StylePropertyId.ShadowOpacity, value), state); }
        }
        
    }

    public partial class UIStyle {
    
        
        public UIForia.Rendering.Visibility Visibility {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.Visibility)FindEnumProperty(StylePropertyId.Visibility); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Visibility, (int)value)); }
        }
            
        public float Opacity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.Opacity); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Opacity, value)); }
        }
            
        public UIForia.Rendering.CursorStyle Cursor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.Cursor).AsCursorStyle; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Cursor, value)); }
        }
            
        public string Painter {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.Painter).AsString; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Painter, value)); }
        }
            
        public UIForia.Rendering.MaterialId Material {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindMaterialIdProperty(StylePropertyId.Material); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Material, value)); }
        }
            
        public UIForia.Rendering.MeshType MeshType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.MeshType)FindEnumProperty(StylePropertyId.MeshType); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MeshType, (int)value)); }
        }
            
        public UIForia.Rendering.MeshFillDirection MeshFillDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.MeshFillDirection)FindEnumProperty(StylePropertyId.MeshFillDirection); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MeshFillDirection, (int)value)); }
        }
            
        public UIForia.Rendering.MeshFillOrigin MeshFillOrigin {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.MeshFillOrigin)FindEnumProperty(StylePropertyId.MeshFillOrigin); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MeshFillOrigin, (int)value)); }
        }
            
        public float MeshFillAmount {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.MeshFillAmount); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MeshFillAmount, value)); }
        }
            
        public UIForia.Rendering.Overflow OverflowX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.Overflow)FindEnumProperty(StylePropertyId.OverflowX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.OverflowX, (int)value)); }
        }
            
        public UIForia.Rendering.Overflow OverflowY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.Overflow)FindEnumProperty(StylePropertyId.OverflowY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.OverflowY, (int)value)); }
        }
            
        public UIForia.Layout.ClipBehavior ClipBehavior {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.ClipBehavior)FindEnumProperty(StylePropertyId.ClipBehavior); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ClipBehavior, (int)value)); }
        }
            
        public UIForia.Rendering.ClipBounds ClipBounds {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.ClipBounds)FindEnumProperty(StylePropertyId.ClipBounds); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ClipBounds, (int)value)); }
        }
            
        public UIForia.UIInput.PointerEvents PointerEvents {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.UIInput.PointerEvents)FindEnumProperty(StylePropertyId.PointerEvents); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PointerEvents, (int)value)); }
        }
            
        public UnityEngine.Color BackgroundColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BackgroundColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value)); }
        }
            
        public UnityEngine.Color BackgroundTint {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BackgroundTint); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundTint, value)); }
        }
            
        public UIForia.UIFixedLength BackgroundImageOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageOffsetX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetX, value)); }
        }
            
        public UIForia.UIFixedLength BackgroundImageOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BackgroundImageOffsetY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetY, value)); }
        }
            
        public float BackgroundImageScaleX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundImageScaleX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleX, value)); }
        }
            
        public float BackgroundImageScaleY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundImageScaleY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleY, value)); }
        }
            
        public float BackgroundImageTileX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundImageTileX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileX, value)); }
        }
            
        public float BackgroundImageTileY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundImageTileY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileY, value)); }
        }
            
        public float BackgroundImageRotation {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.BackgroundImageRotation); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImageRotation, value)); }
        }
            
        public UnityEngine.Texture2D BackgroundImage {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.BackgroundImage).AsTexture2D; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, value)); }
        }
            
        public UIForia.Rendering.BackgroundFit BackgroundFit {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.BackgroundFit)FindEnumProperty(StylePropertyId.BackgroundFit); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BackgroundFit, (int)value)); }
        }
            
        public UnityEngine.Color BorderColorTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorTop); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorTop, value)); }
        }
            
        public UnityEngine.Color BorderColorRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorRight, value)); }
        }
            
        public UnityEngine.Color BorderColorBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorBottom); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorBottom, value)); }
        }
            
        public UnityEngine.Color BorderColorLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.BorderColorLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderColorLeft, value)); }
        }
            
        public UIForia.UIFixedLength CornerBevelTopLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.CornerBevelTopLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.CornerBevelTopLeft, value)); }
        }
            
        public UIForia.UIFixedLength CornerBevelTopRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.CornerBevelTopRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.CornerBevelTopRight, value)); }
        }
            
        public UIForia.UIFixedLength CornerBevelBottomRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.CornerBevelBottomRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.CornerBevelBottomRight, value)); }
        }
            
        public UIForia.UIFixedLength CornerBevelBottomLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.CornerBevelBottomLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.CornerBevelBottomLeft, value)); }
        }
            
        public int FlexItemGrow {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.FlexItemGrow); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexItemGrow, value)); }
        }
            
        public int FlexItemShrink {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.FlexItemShrink); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexItemShrink, value)); }
        }
            
        public UIForia.Layout.LayoutDirection FlexLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutDirection)FindEnumProperty(StylePropertyId.FlexLayoutDirection); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value)); }
        }
            
        public UIForia.Layout.LayoutWrap FlexLayoutWrap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutWrap)FindEnumProperty(StylePropertyId.FlexLayoutWrap); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)value)); }
        }
            
        public float FlexLayoutGapHorizontal {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.FlexLayoutGapHorizontal); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutGapHorizontal, value)); }
        }
            
        public float FlexLayoutGapVertical {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.FlexLayoutGapVertical); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FlexLayoutGapVertical, value)); }
        }
            
        public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridItemX).AsGridItemPlacement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemX, value)); }
        }
            
        public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridItemY).AsGridItemPlacement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemY, value)); }
        }
            
        public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridItemWidth).AsGridItemPlacement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemWidth, value)); }
        }
            
        public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridItemHeight).AsGridItemPlacement; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridItemHeight, value)); }
        }
            
        public UIForia.Layout.LayoutDirection GridLayoutDirection {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutDirection)FindEnumProperty(StylePropertyId.GridLayoutDirection); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutDirection, (int)value)); }
        }
            
        public UIForia.Layout.GridLayoutDensity GridLayoutDensity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.GridLayoutDensity)FindEnumProperty(StylePropertyId.GridLayoutDensity); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutDensity, (int)value)); }
        }
            
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutColTemplate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridLayoutColTemplate).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, value)); }
        }
            
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowTemplate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridLayoutRowTemplate).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, value)); }
        }
            
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutColAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridLayoutColAutoSize).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAutoSize, value)); }
        }
            
        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowAutoSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.GridLayoutRowAutoSize).AsGridTemplate; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, value)); }
        }
            
        public float GridLayoutColGap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.GridLayoutColGap); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColGap, value)); }
        }
            
        public float GridLayoutRowGap {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.GridLayoutRowGap); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowGap, value)); }
        }
            
        public UIForia.Layout.GridAxisAlignment GridLayoutColAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.GridAxisAlignment)FindEnumProperty(StylePropertyId.GridLayoutColAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)value)); }
        }
            
        public UIForia.Layout.GridAxisAlignment GridLayoutRowAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.GridAxisAlignment)FindEnumProperty(StylePropertyId.GridLayoutRowAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)value)); }
        }
            
        public float AlignItemsHorizontal {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.AlignItemsHorizontal); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignItemsHorizontal, value)); }
        }
            
        public float AlignItemsVertical {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.AlignItemsVertical); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignItemsVertical, value)); }
        }
            
        public UIForia.Layout.LayoutFit FitItemsVertical {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutFit)FindEnumProperty(StylePropertyId.FitItemsVertical); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FitItemsVertical, (int)value)); }
        }
            
        public UIForia.Layout.LayoutFit FitItemsHorizontal {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutFit)FindEnumProperty(StylePropertyId.FitItemsHorizontal); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.FitItemsHorizontal, (int)value)); }
        }
            
        public UIForia.Layout.SpaceDistribution DistributeExtraSpaceHorizontal {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.SpaceDistribution)FindEnumProperty(StylePropertyId.DistributeExtraSpaceHorizontal); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.DistributeExtraSpaceHorizontal, (int)value)); }
        }
            
        public UIForia.Layout.SpaceDistribution DistributeExtraSpaceVertical {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.SpaceDistribution)FindEnumProperty(StylePropertyId.DistributeExtraSpaceVertical); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.DistributeExtraSpaceVertical, (int)value)); }
        }
            
        public float RadialLayoutStartAngle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.RadialLayoutStartAngle); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RadialLayoutStartAngle, value)); }
        }
            
        public float RadialLayoutEndAngle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.RadialLayoutEndAngle); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RadialLayoutEndAngle, value)); }
        }
            
        public UIForia.UIFixedLength RadialLayoutRadius {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.RadialLayoutRadius); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RadialLayoutRadius, value)); }
        }
            
        public UIForia.Layout.AlignmentDirection AlignmentDirectionX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.AlignmentDirection)FindEnumProperty(StylePropertyId.AlignmentDirectionX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentDirectionX, (int)value)); }
        }
            
        public UIForia.Layout.AlignmentDirection AlignmentDirectionY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.AlignmentDirection)FindEnumProperty(StylePropertyId.AlignmentDirectionY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentDirectionY, (int)value)); }
        }
            
        public UIForia.Layout.AlignmentTarget AlignmentTargetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.AlignmentTarget)FindEnumProperty(StylePropertyId.AlignmentTargetX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetX, (int)value)); }
        }
            
        public UIForia.Layout.AlignmentTarget AlignmentTargetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.AlignmentTarget)FindEnumProperty(StylePropertyId.AlignmentTargetY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetY, (int)value)); }
        }
            
        public UIForia.OffsetMeasurement AlignmentOriginX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindOffsetMeasurementProperty(StylePropertyId.AlignmentOriginX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentOriginX, value)); }
        }
            
        public UIForia.OffsetMeasurement AlignmentOriginY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindOffsetMeasurementProperty(StylePropertyId.AlignmentOriginY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentOriginY, value)); }
        }
            
        public UIForia.OffsetMeasurement AlignmentOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindOffsetMeasurementProperty(StylePropertyId.AlignmentOffsetX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetX, value)); }
        }
            
        public UIForia.OffsetMeasurement AlignmentOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindOffsetMeasurementProperty(StylePropertyId.AlignmentOffsetY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetY, value)); }
        }
            
        public UIForia.Layout.AlignmentBoundary AlignmentBoundaryX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.AlignmentBoundary)FindEnumProperty(StylePropertyId.AlignmentBoundaryX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentBoundaryX, (int)value)); }
        }
            
        public UIForia.Layout.AlignmentBoundary AlignmentBoundaryY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.AlignmentBoundary)FindEnumProperty(StylePropertyId.AlignmentBoundaryY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.AlignmentBoundaryY, (int)value)); }
        }
            
        public UIForia.Layout.LayoutFit LayoutFitHorizontal {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutFit)FindEnumProperty(StylePropertyId.LayoutFitHorizontal); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.LayoutFitHorizontal, (int)value)); }
        }
            
        public UIForia.Layout.LayoutFit LayoutFitVertical {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutFit)FindEnumProperty(StylePropertyId.LayoutFitVertical); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.LayoutFitVertical, (int)value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MinWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MinWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MinWidth, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MaxWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MaxWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement PreferredWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.PreferredWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MinHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MinHeight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MinHeight, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement MaxHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.MaxHeight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value)); }
        }
            
        public UIForia.Rendering.UIMeasurement PreferredHeight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIMeasurementProperty(StylePropertyId.PreferredHeight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value)); }
        }
            
        public UIForia.UIFixedLength MarginTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.MarginTop); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginTop, value)); }
        }
            
        public UIForia.UIFixedLength MarginRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.MarginRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginRight, value)); }
        }
            
        public UIForia.UIFixedLength MarginBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.MarginBottom); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value)); }
        }
            
        public UIForia.UIFixedLength MarginLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.MarginLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value)); }
        }
            
        public UIForia.UIFixedLength BorderTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderTop); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderTop, value)); }
        }
            
        public UIForia.UIFixedLength BorderRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRight, value)); }
        }
            
        public UIForia.UIFixedLength BorderBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderBottom); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderBottom, value)); }
        }
            
        public UIForia.UIFixedLength BorderLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderLeft, value)); }
        }
            
        public UIForia.UIFixedLength BorderRadiusTopLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusTopLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopLeft, value)); }
        }
            
        public UIForia.UIFixedLength BorderRadiusTopRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusTopRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopRight, value)); }
        }
            
        public UIForia.UIFixedLength BorderRadiusBottomRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomRight, value)); }
        }
            
        public UIForia.UIFixedLength BorderRadiusBottomLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.BorderRadiusBottomLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, value)); }
        }
            
        public UIForia.UIFixedLength PaddingTop {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingTop); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingTop, value)); }
        }
            
        public UIForia.UIFixedLength PaddingRight {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingRight); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingRight, value)); }
        }
            
        public UIForia.UIFixedLength PaddingBottom {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingBottom); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingBottom, value)); }
        }
            
        public UIForia.UIFixedLength PaddingLeft {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.PaddingLeft); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.PaddingLeft, value)); }
        }
            
        public UnityEngine.Color TextColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextColor, value)); }
        }
            
        public UIForia.FontAsset TextFontAsset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return GetProperty(StylePropertyId.TextFontAsset).AsFont; }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, value)); }
        }
            
        public UIForia.UIFixedLength TextFontSize {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TextFontSize); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFontSize, value)); }
        }
            
        public UIForia.Text.FontStyle TextFontStyle {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Text.FontStyle)FindEnumProperty(StylePropertyId.TextFontStyle); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFontStyle, (int)value)); }
        }
            
        public UIForia.Text.TextAlignment TextAlignment {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Text.TextAlignment)FindEnumProperty(StylePropertyId.TextAlignment); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextAlignment, (int)value)); }
        }
            
        public float TextOutlineWidth {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextOutlineWidth); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextOutlineWidth, value)); }
        }
            
        public UnityEngine.Color TextOutlineColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextOutlineColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextOutlineColor, value)); }
        }
            
        public UnityEngine.Color CaretColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.CaretColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.CaretColor, value)); }
        }
            
        public UnityEngine.Color SelectionBackgroundColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.SelectionBackgroundColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.SelectionBackgroundColor, value)); }
        }
            
        public UnityEngine.Color SelectionTextColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.SelectionTextColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.SelectionTextColor, value)); }
        }
            
        public float TextOutlineSoftness {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextOutlineSoftness); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextOutlineSoftness, value)); }
        }
            
        public UnityEngine.Color TextGlowColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextGlowColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowColor, value)); }
        }
            
        public float TextGlowOffset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowOffset); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowOffset, value)); }
        }
            
        public float TextGlowInner {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowInner); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowInner, value)); }
        }
            
        public float TextGlowOuter {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowOuter); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowOuter, value)); }
        }
            
        public float TextGlowPower {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextGlowPower); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextGlowPower, value)); }
        }
            
        public UnityEngine.Color TextUnderlayColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.TextUnderlayColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayColor, value)); }
        }
            
        public float TextUnderlayX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlayX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayX, value)); }
        }
            
        public float TextUnderlayY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlayY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayY, value)); }
        }
            
        public float TextUnderlayDilate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlayDilate); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayDilate, value)); }
        }
            
        public float TextUnderlaySoftness {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextUnderlaySoftness); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlaySoftness, value)); }
        }
            
        public float TextFaceDilate {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TextFaceDilate); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextFaceDilate, value)); }
        }
            
        public UIForia.Rendering.UnderlayType TextUnderlayType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.UnderlayType)FindEnumProperty(StylePropertyId.TextUnderlayType); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextUnderlayType, (int)value)); }
        }
            
        public UIForia.Text.TextTransform TextTransform {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Text.TextTransform)FindEnumProperty(StylePropertyId.TextTransform); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value)); }
        }
            
        public UIForia.Text.WhitespaceMode TextWhitespaceMode {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Text.WhitespaceMode)FindEnumProperty(StylePropertyId.TextWhitespaceMode); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)value)); }
        }
            
        public UIForia.OffsetMeasurement TransformPositionX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindOffsetMeasurementProperty(StylePropertyId.TransformPositionX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value)); }
        }
            
        public UIForia.OffsetMeasurement TransformPositionY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindOffsetMeasurementProperty(StylePropertyId.TransformPositionY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value)); }
        }
            
        public UIForia.UIFixedLength TransformPivotX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TransformPivotX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPivotX, value)); }
        }
            
        public UIForia.UIFixedLength TransformPivotY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindUIFixedLengthProperty(StylePropertyId.TransformPivotY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformPivotY, value)); }
        }
            
        public float TransformScaleX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TransformScaleX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformScaleX, value)); }
        }
            
        public float TransformScaleY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TransformScaleY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformScaleY, value)); }
        }
            
        public float TransformRotation {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.TransformRotation); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.TransformRotation, value)); }
        }
            
        public UIForia.Layout.LayoutType LayoutType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutType)FindEnumProperty(StylePropertyId.LayoutType); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.LayoutType, (int)value)); }
        }
            
        public UIForia.Layout.LayoutBehavior LayoutBehavior {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.LayoutBehavior)FindEnumProperty(StylePropertyId.LayoutBehavior); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.LayoutBehavior, (int)value)); }
        }
            
        public UIForia.Layout.ScrollBehavior ScrollBehaviorX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.ScrollBehavior)FindEnumProperty(StylePropertyId.ScrollBehaviorX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollBehaviorX, (int)value)); }
        }
            
        public UIForia.Layout.ScrollBehavior ScrollBehaviorY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Layout.ScrollBehavior)FindEnumProperty(StylePropertyId.ScrollBehaviorY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ScrollBehaviorY, (int)value)); }
        }
            
        public int ZIndex {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.ZIndex); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ZIndex, value)); }
        }
            
        public int RenderLayerOffset {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.RenderLayerOffset); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RenderLayerOffset, value)); }
        }
            
        public UIForia.Rendering.RenderLayer RenderLayer {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (UIForia.Rendering.RenderLayer)FindEnumProperty(StylePropertyId.RenderLayer); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.RenderLayer, (int)value)); }
        }
            
        public int Layer {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindIntProperty(StylePropertyId.Layer); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.Layer, value)); }
        }
            
        public UnityEngine.Color ShadowColor {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ShadowColor); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowColor, value)); }
        }
            
        public UnityEngine.Color ShadowTint {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindColorProperty(StylePropertyId.ShadowTint); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowTint, value)); }
        }
            
        public UIForia.OffsetMeasurement ShadowOffsetX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindOffsetMeasurementProperty(StylePropertyId.ShadowOffsetX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetX, value)); }
        }
            
        public UIForia.OffsetMeasurement ShadowOffsetY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindOffsetMeasurementProperty(StylePropertyId.ShadowOffsetY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetY, value)); }
        }
            
        public float ShadowSizeX {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowSizeX); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowSizeX, value)); }
        }
            
        public float ShadowSizeY {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowSizeY); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowSizeY, value)); }
        }
            
        public float ShadowIntensity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowIntensity); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowIntensity, value)); }
        }
            
        public float ShadowOpacity {
            [System.Diagnostics.DebuggerStepThrough]
            get { return FindFloatProperty(StylePropertyId.ShadowOpacity); }
            [System.Diagnostics.DebuggerStepThrough]
            set { SetProperty(new StyleProperty(StylePropertyId.ShadowOpacity, value)); }
        }
            
        
    }

    public partial class UIStyleSet {
    
        

            public UIForia.Rendering.Visibility Visibility { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Visibility, out property)) return property.AsVisibility;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.Visibility), out property)) return property.AsVisibility;
                    return DefaultStyleValues_Generated.Visibility;
                }
            }

            public float Opacity { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Opacity, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.Opacity), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.Opacity;
                }
            }

            public UIForia.Rendering.CursorStyle Cursor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Cursor, out property)) return property.AsCursorStyle;
                    return DefaultStyleValues_Generated.Cursor;
                }
            }

            public string Painter { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Painter, out property)) return property.AsString;
                    return DefaultStyleValues_Generated.Painter;
                }
            }

            public UIForia.Rendering.MaterialId Material { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Material, out property)) return property.AsMaterialId;
                    return DefaultStyleValues_Generated.Material;
                }
            }

            public UIForia.Rendering.MeshType MeshType { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MeshType, out property)) return property.AsMeshType;
                    return DefaultStyleValues_Generated.MeshType;
                }
            }

            public UIForia.Rendering.MeshFillDirection MeshFillDirection { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MeshFillDirection, out property)) return property.AsMeshFillDirection;
                    return DefaultStyleValues_Generated.MeshFillDirection;
                }
            }

            public UIForia.Rendering.MeshFillOrigin MeshFillOrigin { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MeshFillOrigin, out property)) return property.AsMeshFillOrigin;
                    return DefaultStyleValues_Generated.MeshFillOrigin;
                }
            }

            public float MeshFillAmount { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MeshFillAmount, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.MeshFillAmount;
                }
            }

            public UIForia.Rendering.Overflow OverflowX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.OverflowX, out property)) return property.AsOverflow;
                    return DefaultStyleValues_Generated.OverflowX;
                }
            }

            public UIForia.Rendering.Overflow OverflowY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.OverflowY, out property)) return property.AsOverflow;
                    return DefaultStyleValues_Generated.OverflowY;
                }
            }

            public UIForia.Layout.ClipBehavior ClipBehavior { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ClipBehavior, out property)) return property.AsClipBehavior;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.ClipBehavior), out property)) return property.AsClipBehavior;
                    return DefaultStyleValues_Generated.ClipBehavior;
                }
            }

            public UIForia.Rendering.ClipBounds ClipBounds { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ClipBounds, out property)) return property.AsClipBounds;
                    return DefaultStyleValues_Generated.ClipBounds;
                }
            }

            public UIForia.UIInput.PointerEvents PointerEvents { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PointerEvents, out property)) return property.AsPointerEvents;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.PointerEvents), out property)) return property.AsPointerEvents;
                    return DefaultStyleValues_Generated.PointerEvents;
                }
            }

            public UnityEngine.Color BackgroundColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BackgroundColor;
                }
            }

            public UnityEngine.Color BackgroundTint { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundTint, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.BackgroundTint), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BackgroundTint;
                }
            }

            public UIForia.UIFixedLength BackgroundImageOffsetX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageOffsetX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageOffsetX;
                }
            }

            public UIForia.UIFixedLength BackgroundImageOffsetY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageOffsetY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BackgroundImageOffsetY;
                }
            }

            public float BackgroundImageScaleX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageScaleX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundImageScaleX;
                }
            }

            public float BackgroundImageScaleY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageScaleY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundImageScaleY;
                }
            }

            public float BackgroundImageTileX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageTileX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundImageTileX;
                }
            }

            public float BackgroundImageTileY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageTileY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundImageTileY;
                }
            }

            public float BackgroundImageRotation { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImageRotation, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.BackgroundImageRotation;
                }
            }

            public UnityEngine.Texture2D BackgroundImage { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundImage, out property)) return property.AsTexture2D;
                    return DefaultStyleValues_Generated.BackgroundImage;
                }
            }

            public UIForia.Rendering.BackgroundFit BackgroundFit { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BackgroundFit, out property)) return property.AsBackgroundFit;
                    return DefaultStyleValues_Generated.BackgroundFit;
                }
            }

            public UnityEngine.Color BorderColorTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorTop, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorTop;
                }
            }

            public UnityEngine.Color BorderColorRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorRight, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorRight;
                }
            }

            public UnityEngine.Color BorderColorBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorBottom, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorBottom;
                }
            }

            public UnityEngine.Color BorderColorLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderColorLeft, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.BorderColorLeft;
                }
            }

            public UIForia.UIFixedLength CornerBevelTopLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.CornerBevelTopLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.CornerBevelTopLeft;
                }
            }

            public UIForia.UIFixedLength CornerBevelTopRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.CornerBevelTopRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.CornerBevelTopRight;
                }
            }

            public UIForia.UIFixedLength CornerBevelBottomRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.CornerBevelBottomRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.CornerBevelBottomRight;
                }
            }

            public UIForia.UIFixedLength CornerBevelBottomLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.CornerBevelBottomLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.CornerBevelBottomLeft;
                }
            }

            public int FlexItemGrow { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexItemGrow, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemGrow;
                }
            }

            public int FlexItemShrink { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexItemShrink, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.FlexItemShrink;
                }
            }

            public UIForia.Layout.LayoutDirection FlexLayoutDirection { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutDirection, out property)) return property.AsLayoutDirection;
                    return DefaultStyleValues_Generated.FlexLayoutDirection;
                }
            }

            public UIForia.Layout.LayoutWrap FlexLayoutWrap { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutWrap, out property)) return property.AsLayoutWrap;
                    return DefaultStyleValues_Generated.FlexLayoutWrap;
                }
            }

            public float FlexLayoutGapHorizontal { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutGapHorizontal, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.FlexLayoutGapHorizontal;
                }
            }

            public float FlexLayoutGapVertical { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FlexLayoutGapVertical, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.FlexLayoutGapVertical;
                }
            }

            public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemX, out property)) return property.AsGridItemPlacement;
                    return DefaultStyleValues_Generated.GridItemX;
                }
            }

            public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemY, out property)) return property.AsGridItemPlacement;
                    return DefaultStyleValues_Generated.GridItemY;
                }
            }

            public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemWidth, out property)) return property.AsGridItemPlacement;
                    return DefaultStyleValues_Generated.GridItemWidth;
                }
            }

            public UIForia.Layout.LayoutTypes.GridItemPlacement GridItemHeight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridItemHeight, out property)) return property.AsGridItemPlacement;
                    return DefaultStyleValues_Generated.GridItemHeight;
                }
            }

            public UIForia.Layout.LayoutDirection GridLayoutDirection { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutDirection, out property)) return property.AsLayoutDirection;
                    return DefaultStyleValues_Generated.GridLayoutDirection;
                }
            }

            public UIForia.Layout.GridLayoutDensity GridLayoutDensity { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutDensity, out property)) return property.AsGridLayoutDensity;
                    return DefaultStyleValues_Generated.GridLayoutDensity;
                }
            }

            public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutColTemplate { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColTemplate, out property)) return property.AsGridTemplate;
                    return DefaultStyleValues_Generated.GridLayoutColTemplate;
                }
            }

            public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowTemplate { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowTemplate, out property)) return property.AsGridTemplate;
                    return DefaultStyleValues_Generated.GridLayoutRowTemplate;
                }
            }

            public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutColAutoSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColAutoSize, out property)) return property.AsGridTemplate;
                    return DefaultStyleValues_Generated.GridLayoutColAutoSize;
                }
            }

            public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GridLayoutRowAutoSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowAutoSize, out property)) return property.AsGridTemplate;
                    return DefaultStyleValues_Generated.GridLayoutRowAutoSize;
                }
            }

            public float GridLayoutColGap { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColGap, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.GridLayoutColGap;
                }
            }

            public float GridLayoutRowGap { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowGap, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.GridLayoutRowGap;
                }
            }

            public UIForia.Layout.GridAxisAlignment GridLayoutColAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutColAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridLayoutColAlignment;
                }
            }

            public UIForia.Layout.GridAxisAlignment GridLayoutRowAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.GridLayoutRowAlignment, out property)) return property.AsGridAxisAlignment;
                    return DefaultStyleValues_Generated.GridLayoutRowAlignment;
                }
            }

            public float AlignItemsHorizontal { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignItemsHorizontal, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.AlignItemsHorizontal;
                }
            }

            public float AlignItemsVertical { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignItemsVertical, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.AlignItemsVertical;
                }
            }

            public UIForia.Layout.LayoutFit FitItemsVertical { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FitItemsVertical, out property)) return property.AsLayoutFit;
                    return DefaultStyleValues_Generated.FitItemsVertical;
                }
            }

            public UIForia.Layout.LayoutFit FitItemsHorizontal { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.FitItemsHorizontal, out property)) return property.AsLayoutFit;
                    return DefaultStyleValues_Generated.FitItemsHorizontal;
                }
            }

            public UIForia.Layout.SpaceDistribution DistributeExtraSpaceHorizontal { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.DistributeExtraSpaceHorizontal, out property)) return property.AsSpaceDistribution;
                    return DefaultStyleValues_Generated.DistributeExtraSpaceHorizontal;
                }
            }

            public UIForia.Layout.SpaceDistribution DistributeExtraSpaceVertical { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.DistributeExtraSpaceVertical, out property)) return property.AsSpaceDistribution;
                    return DefaultStyleValues_Generated.DistributeExtraSpaceVertical;
                }
            }

            public float RadialLayoutStartAngle { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RadialLayoutStartAngle, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.RadialLayoutStartAngle;
                }
            }

            public float RadialLayoutEndAngle { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RadialLayoutEndAngle, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.RadialLayoutEndAngle;
                }
            }

            public UIForia.UIFixedLength RadialLayoutRadius { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RadialLayoutRadius, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.RadialLayoutRadius;
                }
            }

            public UIForia.Layout.AlignmentDirection AlignmentDirectionX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentDirectionX, out property)) return property.AsAlignmentDirection;
                    return DefaultStyleValues_Generated.AlignmentDirectionX;
                }
            }

            public UIForia.Layout.AlignmentDirection AlignmentDirectionY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentDirectionY, out property)) return property.AsAlignmentDirection;
                    return DefaultStyleValues_Generated.AlignmentDirectionY;
                }
            }

            public UIForia.Layout.AlignmentTarget AlignmentTargetX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentTargetX, out property)) return property.AsAlignmentTarget;
                    return DefaultStyleValues_Generated.AlignmentTargetX;
                }
            }

            public UIForia.Layout.AlignmentTarget AlignmentTargetY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentTargetY, out property)) return property.AsAlignmentTarget;
                    return DefaultStyleValues_Generated.AlignmentTargetY;
                }
            }

            public UIForia.OffsetMeasurement AlignmentOriginX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentOriginX, out property)) return property.AsOffsetMeasurement;
                    return DefaultStyleValues_Generated.AlignmentOriginX;
                }
            }

            public UIForia.OffsetMeasurement AlignmentOriginY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentOriginY, out property)) return property.AsOffsetMeasurement;
                    return DefaultStyleValues_Generated.AlignmentOriginY;
                }
            }

            public UIForia.OffsetMeasurement AlignmentOffsetX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentOffsetX, out property)) return property.AsOffsetMeasurement;
                    return DefaultStyleValues_Generated.AlignmentOffsetX;
                }
            }

            public UIForia.OffsetMeasurement AlignmentOffsetY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentOffsetY, out property)) return property.AsOffsetMeasurement;
                    return DefaultStyleValues_Generated.AlignmentOffsetY;
                }
            }

            public UIForia.Layout.AlignmentBoundary AlignmentBoundaryX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentBoundaryX, out property)) return property.AsAlignmentBoundary;
                    return DefaultStyleValues_Generated.AlignmentBoundaryX;
                }
            }

            public UIForia.Layout.AlignmentBoundary AlignmentBoundaryY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.AlignmentBoundaryY, out property)) return property.AsAlignmentBoundary;
                    return DefaultStyleValues_Generated.AlignmentBoundaryY;
                }
            }

            public UIForia.Layout.LayoutFit LayoutFitHorizontal { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.LayoutFitHorizontal, out property)) return property.AsLayoutFit;
                    return DefaultStyleValues_Generated.LayoutFitHorizontal;
                }
            }

            public UIForia.Layout.LayoutFit LayoutFitVertical { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.LayoutFitVertical, out property)) return property.AsLayoutFit;
                    return DefaultStyleValues_Generated.LayoutFitVertical;
                }
            }

            public UIForia.Rendering.UIMeasurement MinWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MinWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MinWidth;
                }
            }

            public UIForia.Rendering.UIMeasurement MaxWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MaxWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MaxWidth;
                }
            }

            public UIForia.Rendering.UIMeasurement PreferredWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PreferredWidth, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.PreferredWidth;
                }
            }

            public UIForia.Rendering.UIMeasurement MinHeight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MinHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MinHeight;
                }
            }

            public UIForia.Rendering.UIMeasurement MaxHeight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MaxHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.MaxHeight;
                }
            }

            public UIForia.Rendering.UIMeasurement PreferredHeight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PreferredHeight, out property)) return property.AsUIMeasurement;
                    return DefaultStyleValues_Generated.PreferredHeight;
                }
            }

            public UIForia.UIFixedLength MarginTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.MarginTop;
                }
            }

            public UIForia.UIFixedLength MarginRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.MarginRight;
                }
            }

            public UIForia.UIFixedLength MarginBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.MarginBottom;
                }
            }

            public UIForia.UIFixedLength MarginLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.MarginLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.MarginLeft;
                }
            }

            public UIForia.UIFixedLength BorderTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderTop;
                }
            }

            public UIForia.UIFixedLength BorderRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRight;
                }
            }

            public UIForia.UIFixedLength BorderBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderBottom;
                }
            }

            public UIForia.UIFixedLength BorderLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderLeft;
                }
            }

            public UIForia.UIFixedLength BorderRadiusTopLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusTopLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusTopLeft;
                }
            }

            public UIForia.UIFixedLength BorderRadiusTopRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusTopRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusTopRight;
                }
            }

            public UIForia.UIFixedLength BorderRadiusBottomRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusBottomRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusBottomRight;
                }
            }

            public UIForia.UIFixedLength BorderRadiusBottomLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.BorderRadiusBottomLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.BorderRadiusBottomLeft;
                }
            }

            public UIForia.UIFixedLength PaddingTop { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingTop, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingTop;
                }
            }

            public UIForia.UIFixedLength PaddingRight { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingRight, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingRight;
                }
            }

            public UIForia.UIFixedLength PaddingBottom { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingBottom, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingBottom;
                }
            }

            public UIForia.UIFixedLength PaddingLeft { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.PaddingLeft, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.PaddingLeft;
                }
            }

            public UnityEngine.Color TextColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextColor;
                }
            }

            public UIForia.FontAsset TextFontAsset { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFontAsset, out property)) return property.AsFont;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontAsset), out property)) return property.AsFont;
                    return DefaultStyleValues_Generated.TextFontAsset;
                }
            }

            public UIForia.UIFixedLength TextFontSize { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFontSize, out property)) return property.AsUIFixedLength;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontSize), out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TextFontSize;
                }
            }

            public UIForia.Text.FontStyle TextFontStyle { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFontStyle, out property)) return property.AsFontStyle;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFontStyle), out property)) return property.AsFontStyle;
                    return DefaultStyleValues_Generated.TextFontStyle;
                }
            }

            public UIForia.Text.TextAlignment TextAlignment { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextAlignment, out property)) return property.AsTextAlignment;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextAlignment), out property)) return property.AsTextAlignment;
                    return DefaultStyleValues_Generated.TextAlignment;
                }
            }

            public float TextOutlineWidth { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextOutlineWidth, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextOutlineWidth), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextOutlineWidth;
                }
            }

            public UnityEngine.Color TextOutlineColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextOutlineColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextOutlineColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextOutlineColor;
                }
            }

            public UnityEngine.Color CaretColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.CaretColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.CaretColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.CaretColor;
                }
            }

            public UnityEngine.Color SelectionBackgroundColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.SelectionBackgroundColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.SelectionBackgroundColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.SelectionBackgroundColor;
                }
            }

            public UnityEngine.Color SelectionTextColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.SelectionTextColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.SelectionTextColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.SelectionTextColor;
                }
            }

            public float TextOutlineSoftness { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextOutlineSoftness, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextOutlineSoftness), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextOutlineSoftness;
                }
            }

            public UnityEngine.Color TextGlowColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextGlowColor;
                }
            }

            public float TextGlowOffset { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowOffset, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowOffset), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowOffset;
                }
            }

            public float TextGlowInner { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowInner, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowInner), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowInner;
                }
            }

            public float TextGlowOuter { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowOuter, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowOuter), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowOuter;
                }
            }

            public float TextGlowPower { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextGlowPower, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextGlowPower), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextGlowPower;
                }
            }

            public UnityEngine.Color TextUnderlayColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayColor, out property)) return property.AsColor;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayColor), out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.TextUnderlayColor;
                }
            }

            public float TextUnderlayX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayX, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayX), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlayX;
                }
            }

            public float TextUnderlayY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayY, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayY), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlayY;
                }
            }

            public float TextUnderlayDilate { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayDilate, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayDilate), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlayDilate;
                }
            }

            public float TextUnderlaySoftness { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlaySoftness, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlaySoftness), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextUnderlaySoftness;
                }
            }

            public float TextFaceDilate { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextFaceDilate, out property)) return property.AsFloat;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextFaceDilate), out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TextFaceDilate;
                }
            }

            public UIForia.Rendering.UnderlayType TextUnderlayType { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextUnderlayType, out property)) return property.AsUnderlayType;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextUnderlayType), out property)) return property.AsUnderlayType;
                    return DefaultStyleValues_Generated.TextUnderlayType;
                }
            }

            public UIForia.Text.TextTransform TextTransform { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextTransform, out property)) return property.AsTextTransform;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextTransform), out property)) return property.AsTextTransform;
                    return DefaultStyleValues_Generated.TextTransform;
                }
            }

            public UIForia.Text.WhitespaceMode TextWhitespaceMode { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TextWhitespaceMode, out property)) return property.AsWhitespaceMode;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.TextWhitespaceMode), out property)) return property.AsWhitespaceMode;
                    return DefaultStyleValues_Generated.TextWhitespaceMode;
                }
            }

            public UIForia.OffsetMeasurement TransformPositionX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPositionX, out property)) return property.AsOffsetMeasurement;
                    return DefaultStyleValues_Generated.TransformPositionX;
                }
            }

            public UIForia.OffsetMeasurement TransformPositionY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPositionY, out property)) return property.AsOffsetMeasurement;
                    return DefaultStyleValues_Generated.TransformPositionY;
                }
            }

            public UIForia.UIFixedLength TransformPivotX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPivotX, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TransformPivotX;
                }
            }

            public UIForia.UIFixedLength TransformPivotY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformPivotY, out property)) return property.AsUIFixedLength;
                    return DefaultStyleValues_Generated.TransformPivotY;
                }
            }

            public float TransformScaleX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformScaleX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformScaleX;
                }
            }

            public float TransformScaleY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformScaleY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformScaleY;
                }
            }

            public float TransformRotation { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.TransformRotation, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.TransformRotation;
                }
            }

            public UIForia.Layout.LayoutType LayoutType { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.LayoutType, out property)) return property.AsLayoutType;
                    return DefaultStyleValues_Generated.LayoutType;
                }
            }

            public UIForia.Layout.LayoutBehavior LayoutBehavior { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.LayoutBehavior, out property)) return property.AsLayoutBehavior;
                    return DefaultStyleValues_Generated.LayoutBehavior;
                }
            }

            public UIForia.Layout.ScrollBehavior ScrollBehaviorX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ScrollBehaviorX, out property)) return property.AsScrollBehavior;
                    return DefaultStyleValues_Generated.ScrollBehaviorX;
                }
            }

            public UIForia.Layout.ScrollBehavior ScrollBehaviorY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ScrollBehaviorY, out property)) return property.AsScrollBehavior;
                    return DefaultStyleValues_Generated.ScrollBehaviorY;
                }
            }

            public int ZIndex { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ZIndex, out property)) return property.AsInt;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.ZIndex), out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.ZIndex;
                }
            }

            public int RenderLayerOffset { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RenderLayerOffset, out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.RenderLayerOffset;
                }
            }

            public UIForia.Rendering.RenderLayer RenderLayer { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.RenderLayer, out property)) return property.AsRenderLayer;
                    return DefaultStyleValues_Generated.RenderLayer;
                }
            }

            public int Layer { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.Layer, out property)) return property.AsInt;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.Layer), out property)) return property.AsInt;
                    return DefaultStyleValues_Generated.Layer;
                }
            }

            public UnityEngine.Color ShadowColor { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowColor, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ShadowColor;
                }
            }

            public UnityEngine.Color ShadowTint { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowTint, out property)) return property.AsColor;
                    return DefaultStyleValues_Generated.ShadowTint;
                }
            }

            public UIForia.OffsetMeasurement ShadowOffsetX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowOffsetX, out property)) return property.AsOffsetMeasurement;
                    return DefaultStyleValues_Generated.ShadowOffsetX;
                }
            }

            public UIForia.OffsetMeasurement ShadowOffsetY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowOffsetY, out property)) return property.AsOffsetMeasurement;
                    return DefaultStyleValues_Generated.ShadowOffsetY;
                }
            }

            public float ShadowSizeX { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowSizeX, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowSizeX;
                }
            }

            public float ShadowSizeY { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowSizeY, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowSizeY;
                }
            }

            public float ShadowIntensity { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowIntensity, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowIntensity;
                }
            }

            public float ShadowOpacity { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.ShadowOpacity, out property)) return property.AsFloat;
                    return DefaultStyleValues_Generated.ShadowOpacity;
                }
            }

        
        public void SetVisibility(in UIForia.Rendering.Visibility? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Visibility, (int)value), state);
        }

        public UIForia.Rendering.Visibility GetVisibility(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Visibility, state).AsVisibility;
        }
        
        public void SetOpacity(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Opacity, value), state);
        }

        public float GetOpacity(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Opacity, state).AsFloat;
        }
        
        public void SetCursor(UIForia.Rendering.CursorStyle value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Cursor, value), state);
        }

        public UIForia.Rendering.CursorStyle GetCursor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Cursor, state).AsCursorStyle;
        }
        
        public void SetPainter(string value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Painter, value), state);
        }

        public string GetPainter(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Painter, state).AsString;
        }
        
        public void SetMaterial(in UIForia.Rendering.MaterialId? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Material, value), state);
        }

        public UIForia.Rendering.MaterialId GetMaterial(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Material, state).AsMaterialId;
        }
        
        public void SetMeshType(in UIForia.Rendering.MeshType? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MeshType, (int)value), state);
        }

        public UIForia.Rendering.MeshType GetMeshType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MeshType, state).AsMeshType;
        }
        
        public void SetMeshFillDirection(in UIForia.Rendering.MeshFillDirection? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MeshFillDirection, (int)value), state);
        }

        public UIForia.Rendering.MeshFillDirection GetMeshFillDirection(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MeshFillDirection, state).AsMeshFillDirection;
        }
        
        public void SetMeshFillOrigin(in UIForia.Rendering.MeshFillOrigin? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MeshFillOrigin, (int)value), state);
        }

        public UIForia.Rendering.MeshFillOrigin GetMeshFillOrigin(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MeshFillOrigin, state).AsMeshFillOrigin;
        }
        
        public void SetMeshFillAmount(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MeshFillAmount, value), state);
        }

        public float GetMeshFillAmount(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MeshFillAmount, state).AsFloat;
        }
        
        public void SetOverflowX(in UIForia.Rendering.Overflow? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.OverflowX, (int)value), state);
        }

        public UIForia.Rendering.Overflow GetOverflowX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.OverflowX, state).AsOverflow;
        }
        
        public void SetOverflowY(in UIForia.Rendering.Overflow? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.OverflowY, (int)value), state);
        }

        public UIForia.Rendering.Overflow GetOverflowY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.OverflowY, state).AsOverflow;
        }
        
        public void SetClipBehavior(in UIForia.Layout.ClipBehavior? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ClipBehavior, (int)value), state);
        }

        public UIForia.Layout.ClipBehavior GetClipBehavior(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ClipBehavior, state).AsClipBehavior;
        }
        
        public void SetClipBounds(in UIForia.Rendering.ClipBounds? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ClipBounds, (int)value), state);
        }

        public UIForia.Rendering.ClipBounds GetClipBounds(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ClipBounds, state).AsClipBounds;
        }
        
        public void SetPointerEvents(in UIForia.UIInput.PointerEvents? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PointerEvents, (int)value), state);
        }

        public UIForia.UIInput.PointerEvents GetPointerEvents(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PointerEvents, state).AsPointerEvents;
        }
        
        public void SetBackgroundColor(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundColor, value), state);
        }

        public UnityEngine.Color GetBackgroundColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundColor, state).AsColor;
        }
        
        public void SetBackgroundTint(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundTint, value), state);
        }

        public UnityEngine.Color GetBackgroundTint(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundTint, state).AsColor;
        }
        
        public void SetBackgroundImageOffsetX(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetX, value), state);
        }

        public UIForia.UIFixedLength GetBackgroundImageOffsetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetX, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageOffsetY(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageOffsetY, value), state);
        }

        public UIForia.UIFixedLength GetBackgroundImageOffsetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageOffsetY, state).AsUIFixedLength;
        }
        
        public void SetBackgroundImageScaleX(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleX, value), state);
        }

        public float GetBackgroundImageScaleX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageScaleX, state).AsFloat;
        }
        
        public void SetBackgroundImageScaleY(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageScaleY, value), state);
        }

        public float GetBackgroundImageScaleY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageScaleY, state).AsFloat;
        }
        
        public void SetBackgroundImageTileX(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileX, value), state);
        }

        public float GetBackgroundImageTileX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageTileX, state).AsFloat;
        }
        
        public void SetBackgroundImageTileY(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageTileY, value), state);
        }

        public float GetBackgroundImageTileY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageTileY, state).AsFloat;
        }
        
        public void SetBackgroundImageRotation(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImageRotation, value), state);
        }

        public float GetBackgroundImageRotation(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImageRotation, state).AsFloat;
        }
        
        public void SetBackgroundImage(UnityEngine.Texture2D value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundImage, value), state);
        }

        public UnityEngine.Texture2D GetBackgroundImage(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundImage, state).AsTexture2D;
        }
        
        public void SetBackgroundFit(in UIForia.Rendering.BackgroundFit? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BackgroundFit, (int)value), state);
        }

        public UIForia.Rendering.BackgroundFit GetBackgroundFit(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BackgroundFit, state).AsBackgroundFit;
        }
        
        public void SetBorderColorTop(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorTop, value), state);
        }

        public UnityEngine.Color GetBorderColorTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorTop, state).AsColor;
        }
        
        public void SetBorderColorRight(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorRight, value), state);
        }

        public UnityEngine.Color GetBorderColorRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorRight, state).AsColor;
        }
        
        public void SetBorderColorBottom(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorBottom, value), state);
        }

        public UnityEngine.Color GetBorderColorBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorBottom, state).AsColor;
        }
        
        public void SetBorderColorLeft(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderColorLeft, value), state);
        }

        public UnityEngine.Color GetBorderColorLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderColorLeft, state).AsColor;
        }
        
        public void SetCornerBevelTopLeft(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.CornerBevelTopLeft, value), state);
        }

        public UIForia.UIFixedLength GetCornerBevelTopLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.CornerBevelTopLeft, state).AsUIFixedLength;
        }
        
        public void SetCornerBevelTopRight(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.CornerBevelTopRight, value), state);
        }

        public UIForia.UIFixedLength GetCornerBevelTopRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.CornerBevelTopRight, state).AsUIFixedLength;
        }
        
        public void SetCornerBevelBottomRight(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.CornerBevelBottomRight, value), state);
        }

        public UIForia.UIFixedLength GetCornerBevelBottomRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.CornerBevelBottomRight, state).AsUIFixedLength;
        }
        
        public void SetCornerBevelBottomLeft(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.CornerBevelBottomLeft, value), state);
        }

        public UIForia.UIFixedLength GetCornerBevelBottomLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.CornerBevelBottomLeft, state).AsUIFixedLength;
        }
        
        public void SetFlexItemGrow(in int? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexItemGrow, value), state);
        }

        public int GetFlexItemGrow(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexItemGrow, state).AsInt;
        }
        
        public void SetFlexItemShrink(in int? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexItemShrink, value), state);
        }

        public int GetFlexItemShrink(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexItemShrink, state).AsInt;
        }
        
        public void SetFlexLayoutDirection(in UIForia.Layout.LayoutDirection? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)value), state);
        }

        public UIForia.Layout.LayoutDirection GetFlexLayoutDirection(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutDirection, state).AsLayoutDirection;
        }
        
        public void SetFlexLayoutWrap(in UIForia.Layout.LayoutWrap? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)value), state);
        }

        public UIForia.Layout.LayoutWrap GetFlexLayoutWrap(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutWrap, state).AsLayoutWrap;
        }
        
        public void SetFlexLayoutGapHorizontal(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutGapHorizontal, value), state);
        }

        public float GetFlexLayoutGapHorizontal(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutGapHorizontal, state).AsFloat;
        }
        
        public void SetFlexLayoutGapVertical(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FlexLayoutGapVertical, value), state);
        }

        public float GetFlexLayoutGapVertical(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexLayoutGapVertical, state).AsFloat;
        }
        
        public void SetGridItemX(in UIForia.Layout.LayoutTypes.GridItemPlacement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemX, value), state);
        }

        public UIForia.Layout.LayoutTypes.GridItemPlacement GetGridItemX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemX, state).AsGridItemPlacement;
        }
        
        public void SetGridItemY(in UIForia.Layout.LayoutTypes.GridItemPlacement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemY, value), state);
        }

        public UIForia.Layout.LayoutTypes.GridItemPlacement GetGridItemY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemY, state).AsGridItemPlacement;
        }
        
        public void SetGridItemWidth(in UIForia.Layout.LayoutTypes.GridItemPlacement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemWidth, value), state);
        }

        public UIForia.Layout.LayoutTypes.GridItemPlacement GetGridItemWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemWidth, state).AsGridItemPlacement;
        }
        
        public void SetGridItemHeight(in UIForia.Layout.LayoutTypes.GridItemPlacement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridItemHeight, value), state);
        }

        public UIForia.Layout.LayoutTypes.GridItemPlacement GetGridItemHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemHeight, state).AsGridItemPlacement;
        }
        
        public void SetGridLayoutDirection(in UIForia.Layout.LayoutDirection? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutDirection, (int)value), state);
        }

        public UIForia.Layout.LayoutDirection GetGridLayoutDirection(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutDirection, state).AsLayoutDirection;
        }
        
        public void SetGridLayoutDensity(in UIForia.Layout.GridLayoutDensity? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutDensity, (int)value), state);
        }

        public UIForia.Layout.GridLayoutDensity GetGridLayoutDensity(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutDensity, state).AsGridLayoutDensity;
        }
        
        public void SetGridLayoutColTemplate(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColTemplate, value), state);
        }

        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GetGridLayoutColTemplate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColTemplate, state).AsGridTemplate;
        }
        
        public void SetGridLayoutRowTemplate(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowTemplate, value), state);
        }

        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GetGridLayoutRowTemplate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowTemplate, state).AsGridTemplate;
        }
        
        public void SetGridLayoutColAutoSize(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAutoSize, value), state);
        }

        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GetGridLayoutColAutoSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColAutoSize, state).AsGridTemplate;
        }
        
        public void SetGridLayoutRowAutoSize(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, value), state);
        }

        public System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize> GetGridLayoutRowAutoSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowAutoSize, state).AsGridTemplate;
        }
        
        public void SetGridLayoutColGap(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColGap, value), state);
        }

        public float GetGridLayoutColGap(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColGap, state).AsFloat;
        }
        
        public void SetGridLayoutRowGap(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowGap, value), state);
        }

        public float GetGridLayoutRowGap(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowGap, state).AsFloat;
        }
        
        public void SetGridLayoutColAlignment(in UIForia.Layout.GridAxisAlignment? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)value), state);
        }

        public UIForia.Layout.GridAxisAlignment GetGridLayoutColAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColAlignment, state).AsGridAxisAlignment;
        }
        
        public void SetGridLayoutRowAlignment(in UIForia.Layout.GridAxisAlignment? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)value), state);
        }

        public UIForia.Layout.GridAxisAlignment GetGridLayoutRowAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowAlignment, state).AsGridAxisAlignment;
        }
        
        public void SetAlignItemsHorizontal(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignItemsHorizontal, value), state);
        }

        public float GetAlignItemsHorizontal(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignItemsHorizontal, state).AsFloat;
        }
        
        public void SetAlignItemsVertical(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignItemsVertical, value), state);
        }

        public float GetAlignItemsVertical(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignItemsVertical, state).AsFloat;
        }
        
        public void SetFitItemsVertical(in UIForia.Layout.LayoutFit? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FitItemsVertical, (int)value), state);
        }

        public UIForia.Layout.LayoutFit GetFitItemsVertical(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FitItemsVertical, state).AsLayoutFit;
        }
        
        public void SetFitItemsHorizontal(in UIForia.Layout.LayoutFit? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.FitItemsHorizontal, (int)value), state);
        }

        public UIForia.Layout.LayoutFit GetFitItemsHorizontal(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FitItemsHorizontal, state).AsLayoutFit;
        }
        
        public void SetDistributeExtraSpaceHorizontal(in UIForia.Layout.SpaceDistribution? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.DistributeExtraSpaceHorizontal, (int)value), state);
        }

        public UIForia.Layout.SpaceDistribution GetDistributeExtraSpaceHorizontal(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.DistributeExtraSpaceHorizontal, state).AsSpaceDistribution;
        }
        
        public void SetDistributeExtraSpaceVertical(in UIForia.Layout.SpaceDistribution? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.DistributeExtraSpaceVertical, (int)value), state);
        }

        public UIForia.Layout.SpaceDistribution GetDistributeExtraSpaceVertical(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.DistributeExtraSpaceVertical, state).AsSpaceDistribution;
        }
        
        public void SetRadialLayoutStartAngle(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RadialLayoutStartAngle, value), state);
        }

        public float GetRadialLayoutStartAngle(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RadialLayoutStartAngle, state).AsFloat;
        }
        
        public void SetRadialLayoutEndAngle(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RadialLayoutEndAngle, value), state);
        }

        public float GetRadialLayoutEndAngle(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RadialLayoutEndAngle, state).AsFloat;
        }
        
        public void SetRadialLayoutRadius(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RadialLayoutRadius, value), state);
        }

        public UIForia.UIFixedLength GetRadialLayoutRadius(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RadialLayoutRadius, state).AsUIFixedLength;
        }
        
        public void SetAlignmentDirectionX(in UIForia.Layout.AlignmentDirection? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentDirectionX, (int)value), state);
        }

        public UIForia.Layout.AlignmentDirection GetAlignmentDirectionX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentDirectionX, state).AsAlignmentDirection;
        }
        
        public void SetAlignmentDirectionY(in UIForia.Layout.AlignmentDirection? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentDirectionY, (int)value), state);
        }

        public UIForia.Layout.AlignmentDirection GetAlignmentDirectionY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentDirectionY, state).AsAlignmentDirection;
        }
        
        public void SetAlignmentTargetX(in UIForia.Layout.AlignmentTarget? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetX, (int)value), state);
        }

        public UIForia.Layout.AlignmentTarget GetAlignmentTargetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentTargetX, state).AsAlignmentTarget;
        }
        
        public void SetAlignmentTargetY(in UIForia.Layout.AlignmentTarget? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentTargetY, (int)value), state);
        }

        public UIForia.Layout.AlignmentTarget GetAlignmentTargetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentTargetY, state).AsAlignmentTarget;
        }
        
        public void SetAlignmentOriginX(in UIForia.OffsetMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentOriginX, value), state);
        }

        public UIForia.OffsetMeasurement GetAlignmentOriginX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentOriginX, state).AsOffsetMeasurement;
        }
        
        public void SetAlignmentOriginY(in UIForia.OffsetMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentOriginY, value), state);
        }

        public UIForia.OffsetMeasurement GetAlignmentOriginY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentOriginY, state).AsOffsetMeasurement;
        }
        
        public void SetAlignmentOffsetX(in UIForia.OffsetMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetX, value), state);
        }

        public UIForia.OffsetMeasurement GetAlignmentOffsetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentOffsetX, state).AsOffsetMeasurement;
        }
        
        public void SetAlignmentOffsetY(in UIForia.OffsetMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentOffsetY, value), state);
        }

        public UIForia.OffsetMeasurement GetAlignmentOffsetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentOffsetY, state).AsOffsetMeasurement;
        }
        
        public void SetAlignmentBoundaryX(in UIForia.Layout.AlignmentBoundary? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentBoundaryX, (int)value), state);
        }

        public UIForia.Layout.AlignmentBoundary GetAlignmentBoundaryX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentBoundaryX, state).AsAlignmentBoundary;
        }
        
        public void SetAlignmentBoundaryY(in UIForia.Layout.AlignmentBoundary? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.AlignmentBoundaryY, (int)value), state);
        }

        public UIForia.Layout.AlignmentBoundary GetAlignmentBoundaryY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.AlignmentBoundaryY, state).AsAlignmentBoundary;
        }
        
        public void SetLayoutFitHorizontal(in UIForia.Layout.LayoutFit? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.LayoutFitHorizontal, (int)value), state);
        }

        public UIForia.Layout.LayoutFit GetLayoutFitHorizontal(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.LayoutFitHorizontal, state).AsLayoutFit;
        }
        
        public void SetLayoutFitVertical(in UIForia.Layout.LayoutFit? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.LayoutFitVertical, (int)value), state);
        }

        public UIForia.Layout.LayoutFit GetLayoutFitVertical(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.LayoutFitVertical, state).AsLayoutFit;
        }
        
        public void SetMinWidth(in UIForia.Rendering.UIMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MinWidth, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMinWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MinWidth, state).AsUIMeasurement;
        }
        
        public void SetMaxWidth(in UIForia.Rendering.UIMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MaxWidth, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMaxWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MaxWidth, state).AsUIMeasurement;
        }
        
        public void SetPreferredWidth(in UIForia.Rendering.UIMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PreferredWidth, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetPreferredWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PreferredWidth, state).AsUIMeasurement;
        }
        
        public void SetMinHeight(in UIForia.Rendering.UIMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MinHeight, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMinHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MinHeight, state).AsUIMeasurement;
        }
        
        public void SetMaxHeight(in UIForia.Rendering.UIMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MaxHeight, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetMaxHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MaxHeight, state).AsUIMeasurement;
        }
        
        public void SetPreferredHeight(in UIForia.Rendering.UIMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PreferredHeight, value), state);
        }

        public UIForia.Rendering.UIMeasurement GetPreferredHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PreferredHeight, state).AsUIMeasurement;
        }
        
        public void SetMarginTop(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginTop, value), state);
        }

        public UIForia.UIFixedLength GetMarginTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginTop, state).AsUIFixedLength;
        }
        
        public void SetMarginRight(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginRight, value), state);
        }

        public UIForia.UIFixedLength GetMarginRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginRight, state).AsUIFixedLength;
        }
        
        public void SetMarginBottom(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginBottom, value), state);
        }

        public UIForia.UIFixedLength GetMarginBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginBottom, state).AsUIFixedLength;
        }
        
        public void SetMarginLeft(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.MarginLeft, value), state);
        }

        public UIForia.UIFixedLength GetMarginLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginLeft, state).AsUIFixedLength;
        }
        
        public void SetBorderTop(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderTop, value), state);
        }

        public UIForia.UIFixedLength GetBorderTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderTop, state).AsUIFixedLength;
        }
        
        public void SetBorderRight(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRight, value), state);
        }

        public UIForia.UIFixedLength GetBorderRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRight, state).AsUIFixedLength;
        }
        
        public void SetBorderBottom(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderBottom, value), state);
        }

        public UIForia.UIFixedLength GetBorderBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderBottom, state).AsUIFixedLength;
        }
        
        public void SetBorderLeft(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderLeft, value), state);
        }

        public UIForia.UIFixedLength GetBorderLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderLeft, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusTopLeft(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopLeft, value), state);
        }

        public UIForia.UIFixedLength GetBorderRadiusTopLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusTopLeft, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusTopRight(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusTopRight, value), state);
        }

        public UIForia.UIFixedLength GetBorderRadiusTopRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusTopRight, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusBottomRight(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomRight, value), state);
        }

        public UIForia.UIFixedLength GetBorderRadiusBottomRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusBottomRight, state).AsUIFixedLength;
        }
        
        public void SetBorderRadiusBottomLeft(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, value), state);
        }

        public UIForia.UIFixedLength GetBorderRadiusBottomLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRadiusBottomLeft, state).AsUIFixedLength;
        }
        
        public void SetPaddingTop(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingTop, value), state);
        }

        public UIForia.UIFixedLength GetPaddingTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingTop, state).AsUIFixedLength;
        }
        
        public void SetPaddingRight(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingRight, value), state);
        }

        public UIForia.UIFixedLength GetPaddingRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingRight, state).AsUIFixedLength;
        }
        
        public void SetPaddingBottom(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingBottom, value), state);
        }

        public UIForia.UIFixedLength GetPaddingBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingBottom, state).AsUIFixedLength;
        }
        
        public void SetPaddingLeft(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.PaddingLeft, value), state);
        }

        public UIForia.UIFixedLength GetPaddingLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingLeft, state).AsUIFixedLength;
        }
        
        public void SetTextColor(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextColor, value), state);
        }

        public UnityEngine.Color GetTextColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextColor, state).AsColor;
        }
        
        public void SetTextFontAsset(UIForia.FontAsset value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontAsset, value), state);
        }

        public UIForia.FontAsset GetTextFontAsset(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontAsset, state).AsFont;
        }
        
        public void SetTextFontSize(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontSize, value), state);
        }

        public UIForia.UIFixedLength GetTextFontSize(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontSize, state).AsUIFixedLength;
        }
        
        public void SetTextFontStyle(in UIForia.Text.FontStyle? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFontStyle, (int)value), state);
        }

        public UIForia.Text.FontStyle GetTextFontStyle(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFontStyle, state).AsFontStyle;
        }
        
        public void SetTextAlignment(in UIForia.Text.TextAlignment? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextAlignment, (int)value), state);
        }

        public UIForia.Text.TextAlignment GetTextAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextAlignment, state).AsTextAlignment;
        }
        
        public void SetTextOutlineWidth(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextOutlineWidth, value), state);
        }

        public float GetTextOutlineWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextOutlineWidth, state).AsFloat;
        }
        
        public void SetTextOutlineColor(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextOutlineColor, value), state);
        }

        public UnityEngine.Color GetTextOutlineColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextOutlineColor, state).AsColor;
        }
        
        public void SetCaretColor(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.CaretColor, value), state);
        }

        public UnityEngine.Color GetCaretColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.CaretColor, state).AsColor;
        }
        
        public void SetSelectionBackgroundColor(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.SelectionBackgroundColor, value), state);
        }

        public UnityEngine.Color GetSelectionBackgroundColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.SelectionBackgroundColor, state).AsColor;
        }
        
        public void SetSelectionTextColor(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.SelectionTextColor, value), state);
        }

        public UnityEngine.Color GetSelectionTextColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.SelectionTextColor, state).AsColor;
        }
        
        public void SetTextOutlineSoftness(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextOutlineSoftness, value), state);
        }

        public float GetTextOutlineSoftness(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextOutlineSoftness, state).AsFloat;
        }
        
        public void SetTextGlowColor(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowColor, value), state);
        }

        public UnityEngine.Color GetTextGlowColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextGlowColor, state).AsColor;
        }
        
        public void SetTextGlowOffset(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowOffset, value), state);
        }

        public float GetTextGlowOffset(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextGlowOffset, state).AsFloat;
        }
        
        public void SetTextGlowInner(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowInner, value), state);
        }

        public float GetTextGlowInner(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextGlowInner, state).AsFloat;
        }
        
        public void SetTextGlowOuter(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowOuter, value), state);
        }

        public float GetTextGlowOuter(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextGlowOuter, state).AsFloat;
        }
        
        public void SetTextGlowPower(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextGlowPower, value), state);
        }

        public float GetTextGlowPower(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextGlowPower, state).AsFloat;
        }
        
        public void SetTextUnderlayColor(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayColor, value), state);
        }

        public UnityEngine.Color GetTextUnderlayColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayColor, state).AsColor;
        }
        
        public void SetTextUnderlayX(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayX, value), state);
        }

        public float GetTextUnderlayX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayX, state).AsFloat;
        }
        
        public void SetTextUnderlayY(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayY, value), state);
        }

        public float GetTextUnderlayY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayY, state).AsFloat;
        }
        
        public void SetTextUnderlayDilate(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayDilate, value), state);
        }

        public float GetTextUnderlayDilate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayDilate, state).AsFloat;
        }
        
        public void SetTextUnderlaySoftness(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlaySoftness, value), state);
        }

        public float GetTextUnderlaySoftness(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlaySoftness, state).AsFloat;
        }
        
        public void SetTextFaceDilate(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextFaceDilate, value), state);
        }

        public float GetTextFaceDilate(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextFaceDilate, state).AsFloat;
        }
        
        public void SetTextUnderlayType(in UIForia.Rendering.UnderlayType? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextUnderlayType, (int)value), state);
        }

        public UIForia.Rendering.UnderlayType GetTextUnderlayType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextUnderlayType, state).AsUnderlayType;
        }
        
        public void SetTextTransform(in UIForia.Text.TextTransform? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextTransform, (int)value), state);
        }

        public UIForia.Text.TextTransform GetTextTransform(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextTransform, state).AsTextTransform;
        }
        
        public void SetTextWhitespaceMode(in UIForia.Text.WhitespaceMode? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)value), state);
        }

        public UIForia.Text.WhitespaceMode GetTextWhitespaceMode(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TextWhitespaceMode, state).AsWhitespaceMode;
        }
        
        public void SetTransformPositionX(in UIForia.OffsetMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPositionX, value), state);
        }

        public UIForia.OffsetMeasurement GetTransformPositionX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPositionX, state).AsOffsetMeasurement;
        }
        
        public void SetTransformPositionY(in UIForia.OffsetMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPositionY, value), state);
        }

        public UIForia.OffsetMeasurement GetTransformPositionY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPositionY, state).AsOffsetMeasurement;
        }
        
        public void SetTransformPivotX(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPivotX, value), state);
        }

        public UIForia.UIFixedLength GetTransformPivotX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPivotX, state).AsUIFixedLength;
        }
        
        public void SetTransformPivotY(in UIForia.UIFixedLength? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformPivotY, value), state);
        }

        public UIForia.UIFixedLength GetTransformPivotY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPivotY, state).AsUIFixedLength;
        }
        
        public void SetTransformScaleX(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformScaleX, value), state);
        }

        public float GetTransformScaleX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformScaleX, state).AsFloat;
        }
        
        public void SetTransformScaleY(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformScaleY, value), state);
        }

        public float GetTransformScaleY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformScaleY, state).AsFloat;
        }
        
        public void SetTransformRotation(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.TransformRotation, value), state);
        }

        public float GetTransformRotation(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformRotation, state).AsFloat;
        }
        
        public void SetLayoutType(in UIForia.Layout.LayoutType? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.LayoutType, (int)value), state);
        }

        public UIForia.Layout.LayoutType GetLayoutType(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.LayoutType, state).AsLayoutType;
        }
        
        public void SetLayoutBehavior(in UIForia.Layout.LayoutBehavior? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.LayoutBehavior, (int)value), state);
        }

        public UIForia.Layout.LayoutBehavior GetLayoutBehavior(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.LayoutBehavior, state).AsLayoutBehavior;
        }
        
        public void SetScrollBehaviorX(in UIForia.Layout.ScrollBehavior? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollBehaviorX, (int)value), state);
        }

        public UIForia.Layout.ScrollBehavior GetScrollBehaviorX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollBehaviorX, state).AsScrollBehavior;
        }
        
        public void SetScrollBehaviorY(in UIForia.Layout.ScrollBehavior? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ScrollBehaviorY, (int)value), state);
        }

        public UIForia.Layout.ScrollBehavior GetScrollBehaviorY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ScrollBehaviorY, state).AsScrollBehavior;
        }
        
        public void SetZIndex(in int? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ZIndex, value), state);
        }

        public int GetZIndex(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ZIndex, state).AsInt;
        }
        
        public void SetRenderLayerOffset(in int? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RenderLayerOffset, value), state);
        }

        public int GetRenderLayerOffset(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RenderLayerOffset, state).AsInt;
        }
        
        public void SetRenderLayer(in UIForia.Rendering.RenderLayer? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.RenderLayer, (int)value), state);
        }

        public UIForia.Rendering.RenderLayer GetRenderLayer(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.RenderLayer, state).AsRenderLayer;
        }
        
        public void SetLayer(in int? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.Layer, value), state);
        }

        public int GetLayer(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.Layer, state).AsInt;
        }
        
        public void SetShadowColor(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowColor, value), state);
        }

        public UnityEngine.Color GetShadowColor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowColor, state).AsColor;
        }
        
        public void SetShadowTint(in UnityEngine.Color? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowTint, value), state);
        }

        public UnityEngine.Color GetShadowTint(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowTint, state).AsColor;
        }
        
        public void SetShadowOffsetX(in UIForia.OffsetMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetX, value), state);
        }

        public UIForia.OffsetMeasurement GetShadowOffsetX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowOffsetX, state).AsOffsetMeasurement;
        }
        
        public void SetShadowOffsetY(in UIForia.OffsetMeasurement? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowOffsetY, value), state);
        }

        public UIForia.OffsetMeasurement GetShadowOffsetY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowOffsetY, state).AsOffsetMeasurement;
        }
        
        public void SetShadowSizeX(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowSizeX, value), state);
        }

        public float GetShadowSizeX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowSizeX, state).AsFloat;
        }
        
        public void SetShadowSizeY(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowSizeY, value), state);
        }

        public float GetShadowSizeY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowSizeY, state).AsFloat;
        }
        
        public void SetShadowIntensity(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowIntensity, value), state);
        }

        public float GetShadowIntensity(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowIntensity, state).AsFloat;
        }
        
        public void SetShadowOpacity(in float? value, StyleState state) {
            SetProperty(new StyleProperty(StylePropertyId.ShadowOpacity, value), state);
        }

        public float GetShadowOpacity(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.ShadowOpacity, state).AsFloat;
        }
        

        public StyleProperty GetComputedStyleProperty(StylePropertyId propertyId) {
        			switch(propertyId) {
				case StylePropertyId.Visibility:
					 return new StyleProperty(StylePropertyId.Visibility, (int)Visibility);
				case StylePropertyId.Opacity:
					 return new StyleProperty(StylePropertyId.Opacity, Opacity);
				case StylePropertyId.Cursor:
					 return new StyleProperty(StylePropertyId.Cursor, Cursor);
				case StylePropertyId.Painter:
					 return new StyleProperty(StylePropertyId.Painter, Painter);
				case StylePropertyId.Material:
					 return new StyleProperty(StylePropertyId.Material, Material);
				case StylePropertyId.MeshType:
					 return new StyleProperty(StylePropertyId.MeshType, (int)MeshType);
				case StylePropertyId.MeshFillDirection:
					 return new StyleProperty(StylePropertyId.MeshFillDirection, (int)MeshFillDirection);
				case StylePropertyId.MeshFillOrigin:
					 return new StyleProperty(StylePropertyId.MeshFillOrigin, (int)MeshFillOrigin);
				case StylePropertyId.MeshFillAmount:
					 return new StyleProperty(StylePropertyId.MeshFillAmount, MeshFillAmount);
				case StylePropertyId.OverflowX:
					 return new StyleProperty(StylePropertyId.OverflowX, (int)OverflowX);
				case StylePropertyId.OverflowY:
					 return new StyleProperty(StylePropertyId.OverflowY, (int)OverflowY);
				case StylePropertyId.ClipBehavior:
					 return new StyleProperty(StylePropertyId.ClipBehavior, (int)ClipBehavior);
				case StylePropertyId.ClipBounds:
					 return new StyleProperty(StylePropertyId.ClipBounds, (int)ClipBounds);
				case StylePropertyId.PointerEvents:
					 return new StyleProperty(StylePropertyId.PointerEvents, (int)PointerEvents);
				case StylePropertyId.BackgroundColor:
					 return new StyleProperty(StylePropertyId.BackgroundColor, BackgroundColor);
				case StylePropertyId.BackgroundTint:
					 return new StyleProperty(StylePropertyId.BackgroundTint, BackgroundTint);
				case StylePropertyId.BackgroundImageOffsetX:
					 return new StyleProperty(StylePropertyId.BackgroundImageOffsetX, BackgroundImageOffsetX);
				case StylePropertyId.BackgroundImageOffsetY:
					 return new StyleProperty(StylePropertyId.BackgroundImageOffsetY, BackgroundImageOffsetY);
				case StylePropertyId.BackgroundImageScaleX:
					 return new StyleProperty(StylePropertyId.BackgroundImageScaleX, BackgroundImageScaleX);
				case StylePropertyId.BackgroundImageScaleY:
					 return new StyleProperty(StylePropertyId.BackgroundImageScaleY, BackgroundImageScaleY);
				case StylePropertyId.BackgroundImageTileX:
					 return new StyleProperty(StylePropertyId.BackgroundImageTileX, BackgroundImageTileX);
				case StylePropertyId.BackgroundImageTileY:
					 return new StyleProperty(StylePropertyId.BackgroundImageTileY, BackgroundImageTileY);
				case StylePropertyId.BackgroundImageRotation:
					 return new StyleProperty(StylePropertyId.BackgroundImageRotation, BackgroundImageRotation);
				case StylePropertyId.BackgroundImage:
					 return new StyleProperty(StylePropertyId.BackgroundImage, BackgroundImage);
				case StylePropertyId.BackgroundFit:
					 return new StyleProperty(StylePropertyId.BackgroundFit, (int)BackgroundFit);
				case StylePropertyId.BorderColorTop:
					 return new StyleProperty(StylePropertyId.BorderColorTop, BorderColorTop);
				case StylePropertyId.BorderColorRight:
					 return new StyleProperty(StylePropertyId.BorderColorRight, BorderColorRight);
				case StylePropertyId.BorderColorBottom:
					 return new StyleProperty(StylePropertyId.BorderColorBottom, BorderColorBottom);
				case StylePropertyId.BorderColorLeft:
					 return new StyleProperty(StylePropertyId.BorderColorLeft, BorderColorLeft);
				case StylePropertyId.CornerBevelTopLeft:
					 return new StyleProperty(StylePropertyId.CornerBevelTopLeft, CornerBevelTopLeft);
				case StylePropertyId.CornerBevelTopRight:
					 return new StyleProperty(StylePropertyId.CornerBevelTopRight, CornerBevelTopRight);
				case StylePropertyId.CornerBevelBottomRight:
					 return new StyleProperty(StylePropertyId.CornerBevelBottomRight, CornerBevelBottomRight);
				case StylePropertyId.CornerBevelBottomLeft:
					 return new StyleProperty(StylePropertyId.CornerBevelBottomLeft, CornerBevelBottomLeft);
				case StylePropertyId.FlexItemGrow:
					 return new StyleProperty(StylePropertyId.FlexItemGrow, FlexItemGrow);
				case StylePropertyId.FlexItemShrink:
					 return new StyleProperty(StylePropertyId.FlexItemShrink, FlexItemShrink);
				case StylePropertyId.FlexLayoutDirection:
					 return new StyleProperty(StylePropertyId.FlexLayoutDirection, (int)FlexLayoutDirection);
				case StylePropertyId.FlexLayoutWrap:
					 return new StyleProperty(StylePropertyId.FlexLayoutWrap, (int)FlexLayoutWrap);
				case StylePropertyId.FlexLayoutGapHorizontal:
					 return new StyleProperty(StylePropertyId.FlexLayoutGapHorizontal, FlexLayoutGapHorizontal);
				case StylePropertyId.FlexLayoutGapVertical:
					 return new StyleProperty(StylePropertyId.FlexLayoutGapVertical, FlexLayoutGapVertical);
				case StylePropertyId.GridItemX:
					 return new StyleProperty(StylePropertyId.GridItemX, GridItemX);
				case StylePropertyId.GridItemY:
					 return new StyleProperty(StylePropertyId.GridItemY, GridItemY);
				case StylePropertyId.GridItemWidth:
					 return new StyleProperty(StylePropertyId.GridItemWidth, GridItemWidth);
				case StylePropertyId.GridItemHeight:
					 return new StyleProperty(StylePropertyId.GridItemHeight, GridItemHeight);
				case StylePropertyId.GridLayoutDirection:
					 return new StyleProperty(StylePropertyId.GridLayoutDirection, (int)GridLayoutDirection);
				case StylePropertyId.GridLayoutDensity:
					 return new StyleProperty(StylePropertyId.GridLayoutDensity, (int)GridLayoutDensity);
				case StylePropertyId.GridLayoutColTemplate:
					 return new StyleProperty(StylePropertyId.GridLayoutColTemplate, GridLayoutColTemplate);
				case StylePropertyId.GridLayoutRowTemplate:
					 return new StyleProperty(StylePropertyId.GridLayoutRowTemplate, GridLayoutRowTemplate);
				case StylePropertyId.GridLayoutColAutoSize:
					 return new StyleProperty(StylePropertyId.GridLayoutColAutoSize, GridLayoutColAutoSize);
				case StylePropertyId.GridLayoutRowAutoSize:
					 return new StyleProperty(StylePropertyId.GridLayoutRowAutoSize, GridLayoutRowAutoSize);
				case StylePropertyId.GridLayoutColGap:
					 return new StyleProperty(StylePropertyId.GridLayoutColGap, GridLayoutColGap);
				case StylePropertyId.GridLayoutRowGap:
					 return new StyleProperty(StylePropertyId.GridLayoutRowGap, GridLayoutRowGap);
				case StylePropertyId.GridLayoutColAlignment:
					 return new StyleProperty(StylePropertyId.GridLayoutColAlignment, (int)GridLayoutColAlignment);
				case StylePropertyId.GridLayoutRowAlignment:
					 return new StyleProperty(StylePropertyId.GridLayoutRowAlignment, (int)GridLayoutRowAlignment);
				case StylePropertyId.AlignItemsHorizontal:
					 return new StyleProperty(StylePropertyId.AlignItemsHorizontal, AlignItemsHorizontal);
				case StylePropertyId.AlignItemsVertical:
					 return new StyleProperty(StylePropertyId.AlignItemsVertical, AlignItemsVertical);
				case StylePropertyId.FitItemsVertical:
					 return new StyleProperty(StylePropertyId.FitItemsVertical, (int)FitItemsVertical);
				case StylePropertyId.FitItemsHorizontal:
					 return new StyleProperty(StylePropertyId.FitItemsHorizontal, (int)FitItemsHorizontal);
				case StylePropertyId.DistributeExtraSpaceHorizontal:
					 return new StyleProperty(StylePropertyId.DistributeExtraSpaceHorizontal, (int)DistributeExtraSpaceHorizontal);
				case StylePropertyId.DistributeExtraSpaceVertical:
					 return new StyleProperty(StylePropertyId.DistributeExtraSpaceVertical, (int)DistributeExtraSpaceVertical);
				case StylePropertyId.RadialLayoutStartAngle:
					 return new StyleProperty(StylePropertyId.RadialLayoutStartAngle, RadialLayoutStartAngle);
				case StylePropertyId.RadialLayoutEndAngle:
					 return new StyleProperty(StylePropertyId.RadialLayoutEndAngle, RadialLayoutEndAngle);
				case StylePropertyId.RadialLayoutRadius:
					 return new StyleProperty(StylePropertyId.RadialLayoutRadius, RadialLayoutRadius);
				case StylePropertyId.AlignmentDirectionX:
					 return new StyleProperty(StylePropertyId.AlignmentDirectionX, (int)AlignmentDirectionX);
				case StylePropertyId.AlignmentDirectionY:
					 return new StyleProperty(StylePropertyId.AlignmentDirectionY, (int)AlignmentDirectionY);
				case StylePropertyId.AlignmentTargetX:
					 return new StyleProperty(StylePropertyId.AlignmentTargetX, (int)AlignmentTargetX);
				case StylePropertyId.AlignmentTargetY:
					 return new StyleProperty(StylePropertyId.AlignmentTargetY, (int)AlignmentTargetY);
				case StylePropertyId.AlignmentOriginX:
					 return new StyleProperty(StylePropertyId.AlignmentOriginX, AlignmentOriginX);
				case StylePropertyId.AlignmentOriginY:
					 return new StyleProperty(StylePropertyId.AlignmentOriginY, AlignmentOriginY);
				case StylePropertyId.AlignmentOffsetX:
					 return new StyleProperty(StylePropertyId.AlignmentOffsetX, AlignmentOffsetX);
				case StylePropertyId.AlignmentOffsetY:
					 return new StyleProperty(StylePropertyId.AlignmentOffsetY, AlignmentOffsetY);
				case StylePropertyId.AlignmentBoundaryX:
					 return new StyleProperty(StylePropertyId.AlignmentBoundaryX, (int)AlignmentBoundaryX);
				case StylePropertyId.AlignmentBoundaryY:
					 return new StyleProperty(StylePropertyId.AlignmentBoundaryY, (int)AlignmentBoundaryY);
				case StylePropertyId.LayoutFitHorizontal:
					 return new StyleProperty(StylePropertyId.LayoutFitHorizontal, (int)LayoutFitHorizontal);
				case StylePropertyId.LayoutFitVertical:
					 return new StyleProperty(StylePropertyId.LayoutFitVertical, (int)LayoutFitVertical);
				case StylePropertyId.MinWidth:
					 return new StyleProperty(StylePropertyId.MinWidth, MinWidth);
				case StylePropertyId.MaxWidth:
					 return new StyleProperty(StylePropertyId.MaxWidth, MaxWidth);
				case StylePropertyId.PreferredWidth:
					 return new StyleProperty(StylePropertyId.PreferredWidth, PreferredWidth);
				case StylePropertyId.MinHeight:
					 return new StyleProperty(StylePropertyId.MinHeight, MinHeight);
				case StylePropertyId.MaxHeight:
					 return new StyleProperty(StylePropertyId.MaxHeight, MaxHeight);
				case StylePropertyId.PreferredHeight:
					 return new StyleProperty(StylePropertyId.PreferredHeight, PreferredHeight);
				case StylePropertyId.MarginTop:
					 return new StyleProperty(StylePropertyId.MarginTop, MarginTop);
				case StylePropertyId.MarginRight:
					 return new StyleProperty(StylePropertyId.MarginRight, MarginRight);
				case StylePropertyId.MarginBottom:
					 return new StyleProperty(StylePropertyId.MarginBottom, MarginBottom);
				case StylePropertyId.MarginLeft:
					 return new StyleProperty(StylePropertyId.MarginLeft, MarginLeft);
				case StylePropertyId.BorderTop:
					 return new StyleProperty(StylePropertyId.BorderTop, BorderTop);
				case StylePropertyId.BorderRight:
					 return new StyleProperty(StylePropertyId.BorderRight, BorderRight);
				case StylePropertyId.BorderBottom:
					 return new StyleProperty(StylePropertyId.BorderBottom, BorderBottom);
				case StylePropertyId.BorderLeft:
					 return new StyleProperty(StylePropertyId.BorderLeft, BorderLeft);
				case StylePropertyId.BorderRadiusTopLeft:
					 return new StyleProperty(StylePropertyId.BorderRadiusTopLeft, BorderRadiusTopLeft);
				case StylePropertyId.BorderRadiusTopRight:
					 return new StyleProperty(StylePropertyId.BorderRadiusTopRight, BorderRadiusTopRight);
				case StylePropertyId.BorderRadiusBottomRight:
					 return new StyleProperty(StylePropertyId.BorderRadiusBottomRight, BorderRadiusBottomRight);
				case StylePropertyId.BorderRadiusBottomLeft:
					 return new StyleProperty(StylePropertyId.BorderRadiusBottomLeft, BorderRadiusBottomLeft);
				case StylePropertyId.PaddingTop:
					 return new StyleProperty(StylePropertyId.PaddingTop, PaddingTop);
				case StylePropertyId.PaddingRight:
					 return new StyleProperty(StylePropertyId.PaddingRight, PaddingRight);
				case StylePropertyId.PaddingBottom:
					 return new StyleProperty(StylePropertyId.PaddingBottom, PaddingBottom);
				case StylePropertyId.PaddingLeft:
					 return new StyleProperty(StylePropertyId.PaddingLeft, PaddingLeft);
				case StylePropertyId.TextColor:
					 return new StyleProperty(StylePropertyId.TextColor, TextColor);
				case StylePropertyId.TextFontAsset:
					 return new StyleProperty(StylePropertyId.TextFontAsset, TextFontAsset);
				case StylePropertyId.TextFontSize:
					 return new StyleProperty(StylePropertyId.TextFontSize, TextFontSize);
				case StylePropertyId.TextFontStyle:
					 return new StyleProperty(StylePropertyId.TextFontStyle, (int)TextFontStyle);
				case StylePropertyId.TextAlignment:
					 return new StyleProperty(StylePropertyId.TextAlignment, (int)TextAlignment);
				case StylePropertyId.TextOutlineWidth:
					 return new StyleProperty(StylePropertyId.TextOutlineWidth, TextOutlineWidth);
				case StylePropertyId.TextOutlineColor:
					 return new StyleProperty(StylePropertyId.TextOutlineColor, TextOutlineColor);
				case StylePropertyId.CaretColor:
					 return new StyleProperty(StylePropertyId.CaretColor, CaretColor);
				case StylePropertyId.SelectionBackgroundColor:
					 return new StyleProperty(StylePropertyId.SelectionBackgroundColor, SelectionBackgroundColor);
				case StylePropertyId.SelectionTextColor:
					 return new StyleProperty(StylePropertyId.SelectionTextColor, SelectionTextColor);
				case StylePropertyId.TextOutlineSoftness:
					 return new StyleProperty(StylePropertyId.TextOutlineSoftness, TextOutlineSoftness);
				case StylePropertyId.TextGlowColor:
					 return new StyleProperty(StylePropertyId.TextGlowColor, TextGlowColor);
				case StylePropertyId.TextGlowOffset:
					 return new StyleProperty(StylePropertyId.TextGlowOffset, TextGlowOffset);
				case StylePropertyId.TextGlowInner:
					 return new StyleProperty(StylePropertyId.TextGlowInner, TextGlowInner);
				case StylePropertyId.TextGlowOuter:
					 return new StyleProperty(StylePropertyId.TextGlowOuter, TextGlowOuter);
				case StylePropertyId.TextGlowPower:
					 return new StyleProperty(StylePropertyId.TextGlowPower, TextGlowPower);
				case StylePropertyId.TextUnderlayColor:
					 return new StyleProperty(StylePropertyId.TextUnderlayColor, TextUnderlayColor);
				case StylePropertyId.TextUnderlayX:
					 return new StyleProperty(StylePropertyId.TextUnderlayX, TextUnderlayX);
				case StylePropertyId.TextUnderlayY:
					 return new StyleProperty(StylePropertyId.TextUnderlayY, TextUnderlayY);
				case StylePropertyId.TextUnderlayDilate:
					 return new StyleProperty(StylePropertyId.TextUnderlayDilate, TextUnderlayDilate);
				case StylePropertyId.TextUnderlaySoftness:
					 return new StyleProperty(StylePropertyId.TextUnderlaySoftness, TextUnderlaySoftness);
				case StylePropertyId.TextFaceDilate:
					 return new StyleProperty(StylePropertyId.TextFaceDilate, TextFaceDilate);
				case StylePropertyId.TextUnderlayType:
					 return new StyleProperty(StylePropertyId.TextUnderlayType, (int)TextUnderlayType);
				case StylePropertyId.TextTransform:
					 return new StyleProperty(StylePropertyId.TextTransform, (int)TextTransform);
				case StylePropertyId.TextWhitespaceMode:
					 return new StyleProperty(StylePropertyId.TextWhitespaceMode, (int)TextWhitespaceMode);
				case StylePropertyId.TransformPositionX:
					 return new StyleProperty(StylePropertyId.TransformPositionX, TransformPositionX);
				case StylePropertyId.TransformPositionY:
					 return new StyleProperty(StylePropertyId.TransformPositionY, TransformPositionY);
				case StylePropertyId.TransformPivotX:
					 return new StyleProperty(StylePropertyId.TransformPivotX, TransformPivotX);
				case StylePropertyId.TransformPivotY:
					 return new StyleProperty(StylePropertyId.TransformPivotY, TransformPivotY);
				case StylePropertyId.TransformScaleX:
					 return new StyleProperty(StylePropertyId.TransformScaleX, TransformScaleX);
				case StylePropertyId.TransformScaleY:
					 return new StyleProperty(StylePropertyId.TransformScaleY, TransformScaleY);
				case StylePropertyId.TransformRotation:
					 return new StyleProperty(StylePropertyId.TransformRotation, TransformRotation);
				case StylePropertyId.LayoutType:
					 return new StyleProperty(StylePropertyId.LayoutType, (int)LayoutType);
				case StylePropertyId.LayoutBehavior:
					 return new StyleProperty(StylePropertyId.LayoutBehavior, (int)LayoutBehavior);
				case StylePropertyId.ScrollBehaviorX:
					 return new StyleProperty(StylePropertyId.ScrollBehaviorX, (int)ScrollBehaviorX);
				case StylePropertyId.ScrollBehaviorY:
					 return new StyleProperty(StylePropertyId.ScrollBehaviorY, (int)ScrollBehaviorY);
				case StylePropertyId.ZIndex:
					 return new StyleProperty(StylePropertyId.ZIndex, ZIndex);
				case StylePropertyId.RenderLayerOffset:
					 return new StyleProperty(StylePropertyId.RenderLayerOffset, RenderLayerOffset);
				case StylePropertyId.RenderLayer:
					 return new StyleProperty(StylePropertyId.RenderLayer, (int)RenderLayer);
				case StylePropertyId.Layer:
					 return new StyleProperty(StylePropertyId.Layer, Layer);
				case StylePropertyId.ShadowColor:
					 return new StyleProperty(StylePropertyId.ShadowColor, ShadowColor);
				case StylePropertyId.ShadowTint:
					 return new StyleProperty(StylePropertyId.ShadowTint, ShadowTint);
				case StylePropertyId.ShadowOffsetX:
					 return new StyleProperty(StylePropertyId.ShadowOffsetX, ShadowOffsetX);
				case StylePropertyId.ShadowOffsetY:
					 return new StyleProperty(StylePropertyId.ShadowOffsetY, ShadowOffsetY);
				case StylePropertyId.ShadowSizeX:
					 return new StyleProperty(StylePropertyId.ShadowSizeX, ShadowSizeX);
				case StylePropertyId.ShadowSizeY:
					 return new StyleProperty(StylePropertyId.ShadowSizeY, ShadowSizeY);
				case StylePropertyId.ShadowIntensity:
					 return new StyleProperty(StylePropertyId.ShadowIntensity, ShadowIntensity);
				case StylePropertyId.ShadowOpacity:
					 return new StyleProperty(StylePropertyId.ShadowOpacity, ShadowOpacity);
				default: throw new System.ArgumentOutOfRangeException(nameof(propertyId), propertyId, null);
				}  
        }

    }

    public static partial class StyleUtil {
        
        public static System.Type GetPropertyType(StylePropertyId propertyId) {
            switch (propertyId) {

                                case StylePropertyId.Visibility: return typeof(UIForia.Rendering.Visibility);
                    case StylePropertyId.Opacity: return typeof(System.Single);
                    case StylePropertyId.Cursor: return typeof(UIForia.Rendering.CursorStyle);
                    case StylePropertyId.Painter: return typeof(System.String);
                    case StylePropertyId.Material: return typeof(UIForia.Rendering.MaterialId);
                    case StylePropertyId.MeshType: return typeof(UIForia.Rendering.MeshType);
                    case StylePropertyId.MeshFillDirection: return typeof(UIForia.Rendering.MeshFillDirection);
                    case StylePropertyId.MeshFillOrigin: return typeof(UIForia.Rendering.MeshFillOrigin);
                    case StylePropertyId.MeshFillAmount: return typeof(System.Single);
                    case StylePropertyId.OverflowX: return typeof(UIForia.Rendering.Overflow);
                    case StylePropertyId.OverflowY: return typeof(UIForia.Rendering.Overflow);
                    case StylePropertyId.ClipBehavior: return typeof(UIForia.Layout.ClipBehavior);
                    case StylePropertyId.ClipBounds: return typeof(UIForia.Rendering.ClipBounds);
                    case StylePropertyId.PointerEvents: return typeof(UIForia.UIInput.PointerEvents);
                    case StylePropertyId.BackgroundColor: return typeof(UnityEngine.Color);
                    case StylePropertyId.BackgroundTint: return typeof(UnityEngine.Color);
                    case StylePropertyId.BackgroundImageOffsetX: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.BackgroundImageOffsetY: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.BackgroundImageScaleX: return typeof(System.Single);
                    case StylePropertyId.BackgroundImageScaleY: return typeof(System.Single);
                    case StylePropertyId.BackgroundImageTileX: return typeof(System.Single);
                    case StylePropertyId.BackgroundImageTileY: return typeof(System.Single);
                    case StylePropertyId.BackgroundImageRotation: return typeof(System.Single);
                    case StylePropertyId.BackgroundImage: return typeof(UnityEngine.Texture2D);
                    case StylePropertyId.BackgroundFit: return typeof(UIForia.Rendering.BackgroundFit);
                    case StylePropertyId.BorderColorTop: return typeof(UnityEngine.Color);
                    case StylePropertyId.BorderColorRight: return typeof(UnityEngine.Color);
                    case StylePropertyId.BorderColorBottom: return typeof(UnityEngine.Color);
                    case StylePropertyId.BorderColorLeft: return typeof(UnityEngine.Color);
                    case StylePropertyId.CornerBevelTopLeft: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.CornerBevelTopRight: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.CornerBevelBottomRight: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.CornerBevelBottomLeft: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.FlexItemGrow: return typeof(System.Int32);
                    case StylePropertyId.FlexItemShrink: return typeof(System.Int32);
                    case StylePropertyId.FlexLayoutDirection: return typeof(UIForia.Layout.LayoutDirection);
                    case StylePropertyId.FlexLayoutWrap: return typeof(UIForia.Layout.LayoutWrap);
                    case StylePropertyId.FlexLayoutGapHorizontal: return typeof(System.Single);
                    case StylePropertyId.FlexLayoutGapVertical: return typeof(System.Single);
                    case StylePropertyId.GridItemX: return typeof(UIForia.Layout.LayoutTypes.GridItemPlacement);
                    case StylePropertyId.GridItemY: return typeof(UIForia.Layout.LayoutTypes.GridItemPlacement);
                    case StylePropertyId.GridItemWidth: return typeof(UIForia.Layout.LayoutTypes.GridItemPlacement);
                    case StylePropertyId.GridItemHeight: return typeof(UIForia.Layout.LayoutTypes.GridItemPlacement);
                    case StylePropertyId.GridLayoutDirection: return typeof(UIForia.Layout.LayoutDirection);
                    case StylePropertyId.GridLayoutDensity: return typeof(UIForia.Layout.GridLayoutDensity);
                    case StylePropertyId.GridLayoutColTemplate: return typeof(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>);
                    case StylePropertyId.GridLayoutRowTemplate: return typeof(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>);
                    case StylePropertyId.GridLayoutColAutoSize: return typeof(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>);
                    case StylePropertyId.GridLayoutRowAutoSize: return typeof(System.Collections.Generic.IReadOnlyList<UIForia.Layout.LayoutTypes.GridTrackSize>);
                    case StylePropertyId.GridLayoutColGap: return typeof(System.Single);
                    case StylePropertyId.GridLayoutRowGap: return typeof(System.Single);
                    case StylePropertyId.GridLayoutColAlignment: return typeof(UIForia.Layout.GridAxisAlignment);
                    case StylePropertyId.GridLayoutRowAlignment: return typeof(UIForia.Layout.GridAxisAlignment);
                    case StylePropertyId.AlignItemsHorizontal: return typeof(System.Single);
                    case StylePropertyId.AlignItemsVertical: return typeof(System.Single);
                    case StylePropertyId.FitItemsVertical: return typeof(UIForia.Layout.LayoutFit);
                    case StylePropertyId.FitItemsHorizontal: return typeof(UIForia.Layout.LayoutFit);
                    case StylePropertyId.DistributeExtraSpaceHorizontal: return typeof(UIForia.Layout.SpaceDistribution);
                    case StylePropertyId.DistributeExtraSpaceVertical: return typeof(UIForia.Layout.SpaceDistribution);
                    case StylePropertyId.RadialLayoutStartAngle: return typeof(System.Single);
                    case StylePropertyId.RadialLayoutEndAngle: return typeof(System.Single);
                    case StylePropertyId.RadialLayoutRadius: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.AlignmentDirectionX: return typeof(UIForia.Layout.AlignmentDirection);
                    case StylePropertyId.AlignmentDirectionY: return typeof(UIForia.Layout.AlignmentDirection);
                    case StylePropertyId.AlignmentTargetX: return typeof(UIForia.Layout.AlignmentTarget);
                    case StylePropertyId.AlignmentTargetY: return typeof(UIForia.Layout.AlignmentTarget);
                    case StylePropertyId.AlignmentOriginX: return typeof(UIForia.OffsetMeasurement);
                    case StylePropertyId.AlignmentOriginY: return typeof(UIForia.OffsetMeasurement);
                    case StylePropertyId.AlignmentOffsetX: return typeof(UIForia.OffsetMeasurement);
                    case StylePropertyId.AlignmentOffsetY: return typeof(UIForia.OffsetMeasurement);
                    case StylePropertyId.AlignmentBoundaryX: return typeof(UIForia.Layout.AlignmentBoundary);
                    case StylePropertyId.AlignmentBoundaryY: return typeof(UIForia.Layout.AlignmentBoundary);
                    case StylePropertyId.LayoutFitHorizontal: return typeof(UIForia.Layout.LayoutFit);
                    case StylePropertyId.LayoutFitVertical: return typeof(UIForia.Layout.LayoutFit);
                    case StylePropertyId.MinWidth: return typeof(UIForia.Rendering.UIMeasurement);
                    case StylePropertyId.MaxWidth: return typeof(UIForia.Rendering.UIMeasurement);
                    case StylePropertyId.PreferredWidth: return typeof(UIForia.Rendering.UIMeasurement);
                    case StylePropertyId.MinHeight: return typeof(UIForia.Rendering.UIMeasurement);
                    case StylePropertyId.MaxHeight: return typeof(UIForia.Rendering.UIMeasurement);
                    case StylePropertyId.PreferredHeight: return typeof(UIForia.Rendering.UIMeasurement);
                    case StylePropertyId.MarginTop: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.MarginRight: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.MarginBottom: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.MarginLeft: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.BorderTop: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.BorderRight: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.BorderBottom: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.BorderLeft: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.BorderRadiusTopLeft: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.BorderRadiusTopRight: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.BorderRadiusBottomRight: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.BorderRadiusBottomLeft: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.PaddingTop: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.PaddingRight: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.PaddingBottom: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.PaddingLeft: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.TextColor: return typeof(UnityEngine.Color);
                    case StylePropertyId.TextFontAsset: return typeof(UIForia.FontAsset);
                    case StylePropertyId.TextFontSize: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.TextFontStyle: return typeof(UIForia.Text.FontStyle);
                    case StylePropertyId.TextAlignment: return typeof(UIForia.Text.TextAlignment);
                    case StylePropertyId.TextOutlineWidth: return typeof(System.Single);
                    case StylePropertyId.TextOutlineColor: return typeof(UnityEngine.Color);
                    case StylePropertyId.CaretColor: return typeof(UnityEngine.Color);
                    case StylePropertyId.SelectionBackgroundColor: return typeof(UnityEngine.Color);
                    case StylePropertyId.SelectionTextColor: return typeof(UnityEngine.Color);
                    case StylePropertyId.TextOutlineSoftness: return typeof(System.Single);
                    case StylePropertyId.TextGlowColor: return typeof(UnityEngine.Color);
                    case StylePropertyId.TextGlowOffset: return typeof(System.Single);
                    case StylePropertyId.TextGlowInner: return typeof(System.Single);
                    case StylePropertyId.TextGlowOuter: return typeof(System.Single);
                    case StylePropertyId.TextGlowPower: return typeof(System.Single);
                    case StylePropertyId.TextUnderlayColor: return typeof(UnityEngine.Color);
                    case StylePropertyId.TextUnderlayX: return typeof(System.Single);
                    case StylePropertyId.TextUnderlayY: return typeof(System.Single);
                    case StylePropertyId.TextUnderlayDilate: return typeof(System.Single);
                    case StylePropertyId.TextUnderlaySoftness: return typeof(System.Single);
                    case StylePropertyId.TextFaceDilate: return typeof(System.Single);
                    case StylePropertyId.TextUnderlayType: return typeof(UIForia.Rendering.UnderlayType);
                    case StylePropertyId.TextTransform: return typeof(UIForia.Text.TextTransform);
                    case StylePropertyId.TextWhitespaceMode: return typeof(UIForia.Text.WhitespaceMode);
                    case StylePropertyId.TransformPositionX: return typeof(UIForia.OffsetMeasurement);
                    case StylePropertyId.TransformPositionY: return typeof(UIForia.OffsetMeasurement);
                    case StylePropertyId.TransformPivotX: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.TransformPivotY: return typeof(UIForia.UIFixedLength);
                    case StylePropertyId.TransformScaleX: return typeof(System.Single);
                    case StylePropertyId.TransformScaleY: return typeof(System.Single);
                    case StylePropertyId.TransformRotation: return typeof(System.Single);
                    case StylePropertyId.LayoutType: return typeof(UIForia.Layout.LayoutType);
                    case StylePropertyId.LayoutBehavior: return typeof(UIForia.Layout.LayoutBehavior);
                    case StylePropertyId.ScrollBehaviorX: return typeof(UIForia.Layout.ScrollBehavior);
                    case StylePropertyId.ScrollBehaviorY: return typeof(UIForia.Layout.ScrollBehavior);
                    case StylePropertyId.ZIndex: return typeof(System.Int32);
                    case StylePropertyId.RenderLayerOffset: return typeof(System.Int32);
                    case StylePropertyId.RenderLayer: return typeof(UIForia.Rendering.RenderLayer);
                    case StylePropertyId.Layer: return typeof(System.Int32);
                    case StylePropertyId.ShadowColor: return typeof(UnityEngine.Color);
                    case StylePropertyId.ShadowTint: return typeof(UnityEngine.Color);
                    case StylePropertyId.ShadowOffsetX: return typeof(UIForia.OffsetMeasurement);
                    case StylePropertyId.ShadowOffsetY: return typeof(UIForia.OffsetMeasurement);
                    case StylePropertyId.ShadowSizeX: return typeof(System.Single);
                    case StylePropertyId.ShadowSizeY: return typeof(System.Single);
                    case StylePropertyId.ShadowIntensity: return typeof(System.Single);
                    case StylePropertyId.ShadowOpacity: return typeof(System.Single);

            }

            return null;

        }

      public static bool CanAnimate(StylePropertyId propertyId) {
                switch (propertyId) {
    
                    case StylePropertyId.Opacity: return true;
                    case StylePropertyId.Material: return true;
                    case StylePropertyId.MeshFillAmount: return true;
                    case StylePropertyId.BackgroundColor: return true;
                    case StylePropertyId.BackgroundTint: return true;
                    case StylePropertyId.BackgroundImageOffsetX: return true;
                    case StylePropertyId.BackgroundImageOffsetY: return true;
                    case StylePropertyId.BackgroundImageScaleX: return true;
                    case StylePropertyId.BackgroundImageScaleY: return true;
                    case StylePropertyId.BackgroundImageTileX: return true;
                    case StylePropertyId.BackgroundImageTileY: return true;
                    case StylePropertyId.BackgroundImageRotation: return true;
                    case StylePropertyId.BorderColorTop: return true;
                    case StylePropertyId.BorderColorRight: return true;
                    case StylePropertyId.BorderColorBottom: return true;
                    case StylePropertyId.BorderColorLeft: return true;
                    case StylePropertyId.CornerBevelTopLeft: return true;
                    case StylePropertyId.CornerBevelTopRight: return true;
                    case StylePropertyId.CornerBevelBottomRight: return true;
                    case StylePropertyId.CornerBevelBottomLeft: return true;
                    case StylePropertyId.FlexItemGrow: return true;
                    case StylePropertyId.FlexItemShrink: return true;
                    case StylePropertyId.FlexLayoutGapHorizontal: return true;
                    case StylePropertyId.FlexLayoutGapVertical: return true;
                    case StylePropertyId.GridLayoutColGap: return true;
                    case StylePropertyId.GridLayoutRowGap: return true;
                    case StylePropertyId.RadialLayoutStartAngle: return true;
                    case StylePropertyId.RadialLayoutEndAngle: return true;
                    case StylePropertyId.RadialLayoutRadius: return true;
                    case StylePropertyId.AlignmentOriginX: return true;
                    case StylePropertyId.AlignmentOriginY: return true;
                    case StylePropertyId.AlignmentOffsetX: return true;
                    case StylePropertyId.AlignmentOffsetY: return true;
                    case StylePropertyId.MinWidth: return true;
                    case StylePropertyId.MaxWidth: return true;
                    case StylePropertyId.PreferredWidth: return true;
                    case StylePropertyId.MinHeight: return true;
                    case StylePropertyId.MaxHeight: return true;
                    case StylePropertyId.PreferredHeight: return true;
                    case StylePropertyId.MarginTop: return true;
                    case StylePropertyId.MarginRight: return true;
                    case StylePropertyId.MarginBottom: return true;
                    case StylePropertyId.MarginLeft: return true;
                    case StylePropertyId.BorderTop: return true;
                    case StylePropertyId.BorderRight: return true;
                    case StylePropertyId.BorderBottom: return true;
                    case StylePropertyId.BorderLeft: return true;
                    case StylePropertyId.BorderRadiusTopLeft: return true;
                    case StylePropertyId.BorderRadiusTopRight: return true;
                    case StylePropertyId.BorderRadiusBottomRight: return true;
                    case StylePropertyId.BorderRadiusBottomLeft: return true;
                    case StylePropertyId.PaddingTop: return true;
                    case StylePropertyId.PaddingRight: return true;
                    case StylePropertyId.PaddingBottom: return true;
                    case StylePropertyId.PaddingLeft: return true;
                    case StylePropertyId.TextColor: return true;
                    case StylePropertyId.TextFontSize: return true;
                    case StylePropertyId.TextOutlineWidth: return true;
                    case StylePropertyId.TextOutlineColor: return true;
                    case StylePropertyId.CaretColor: return true;
                    case StylePropertyId.SelectionBackgroundColor: return true;
                    case StylePropertyId.SelectionTextColor: return true;
                    case StylePropertyId.TextOutlineSoftness: return true;
                    case StylePropertyId.TextGlowColor: return true;
                    case StylePropertyId.TextGlowOffset: return true;
                    case StylePropertyId.TextGlowInner: return true;
                    case StylePropertyId.TextGlowOuter: return true;
                    case StylePropertyId.TextGlowPower: return true;
                    case StylePropertyId.TextUnderlayColor: return true;
                    case StylePropertyId.TextUnderlayX: return true;
                    case StylePropertyId.TextUnderlayY: return true;
                    case StylePropertyId.TextUnderlayDilate: return true;
                    case StylePropertyId.TextUnderlaySoftness: return true;
                    case StylePropertyId.TextFaceDilate: return true;
                    case StylePropertyId.TransformPositionX: return true;
                    case StylePropertyId.TransformPositionY: return true;
                    case StylePropertyId.TransformPivotX: return true;
                    case StylePropertyId.TransformPivotY: return true;
                    case StylePropertyId.TransformScaleX: return true;
                    case StylePropertyId.TransformScaleY: return true;
                    case StylePropertyId.TransformRotation: return true;
                    case StylePropertyId.ZIndex: return true;
                    case StylePropertyId.RenderLayerOffset: return true;
                    case StylePropertyId.RenderLayer: return true;
                    case StylePropertyId.Layer: return true;
                    case StylePropertyId.ShadowColor: return true;
                    case StylePropertyId.ShadowTint: return true;
                    case StylePropertyId.ShadowOffsetX: return true;
                    case StylePropertyId.ShadowOffsetY: return true;
                    case StylePropertyId.ShadowSizeX: return true;
                    case StylePropertyId.ShadowSizeY: return true;
                    case StylePropertyId.ShadowIntensity: return true;
                    case StylePropertyId.ShadowOpacity: return true;

                }
    
                return false;
            }

        public static bool IsInherited(StylePropertyId propertyId) {
            switch (propertyId) {

                    case StylePropertyId.Visibility: return true;
                    case StylePropertyId.Opacity: return true;
                    case StylePropertyId.ClipBehavior: return true;
                    case StylePropertyId.PointerEvents: return true;
                    case StylePropertyId.BackgroundTint: return true;
                    case StylePropertyId.TextColor: return true;
                    case StylePropertyId.TextFontAsset: return true;
                    case StylePropertyId.TextFontSize: return true;
                    case StylePropertyId.TextFontStyle: return true;
                    case StylePropertyId.TextAlignment: return true;
                    case StylePropertyId.TextOutlineWidth: return true;
                    case StylePropertyId.TextOutlineColor: return true;
                    case StylePropertyId.CaretColor: return true;
                    case StylePropertyId.SelectionBackgroundColor: return true;
                    case StylePropertyId.SelectionTextColor: return true;
                    case StylePropertyId.TextOutlineSoftness: return true;
                    case StylePropertyId.TextGlowColor: return true;
                    case StylePropertyId.TextGlowOffset: return true;
                    case StylePropertyId.TextGlowInner: return true;
                    case StylePropertyId.TextGlowOuter: return true;
                    case StylePropertyId.TextGlowPower: return true;
                    case StylePropertyId.TextUnderlayColor: return true;
                    case StylePropertyId.TextUnderlayX: return true;
                    case StylePropertyId.TextUnderlayY: return true;
                    case StylePropertyId.TextUnderlayDilate: return true;
                    case StylePropertyId.TextUnderlaySoftness: return true;
                    case StylePropertyId.TextFaceDilate: return true;
                    case StylePropertyId.TextUnderlayType: return true;
                    case StylePropertyId.TextTransform: return true;
                    case StylePropertyId.TextWhitespaceMode: return true;
                    case StylePropertyId.ZIndex: return true;
                    case StylePropertyId.Layer: return true;

            }

            return false;
        }

    }

}