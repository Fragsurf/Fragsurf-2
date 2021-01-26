using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.UIInput;
using UIForia.Util;
using UnityEditor;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia.Editor {

    public enum InheritanceType {

        NotInherited,
        Inherited

    }

    public static class CodeGen {

        internal static readonly PropertyGenerator[] properties = {
            
            // Misc
            new PropertyGenerator<Visibility>(StylePropertyId.Visibility, Visibility.Visible, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.Opacity, 1, InheritanceType.Inherited),
            new PropertyGenerator<CursorStyle>(StylePropertyId.Cursor, null),
            new PropertyGenerator<string>(StylePropertyId.Painter, string.Empty),
            new AnimatedPropertyGenerator<MaterialId>(StylePropertyId.Material, default, InheritanceType.NotInherited, "(MaterialId)0"), 
            
            // Mesh
            new PropertyGenerator<MeshType>(StylePropertyId.MeshType, MeshType.Simple), 
            new PropertyGenerator<MeshFillDirection>(StylePropertyId.MeshFillDirection, MeshFillDirection.Clockwise), 
            new PropertyGenerator<MeshFillOrigin>(StylePropertyId.MeshFillOrigin, 0),
            new AnimatedPropertyGenerator<float>(StylePropertyId.MeshFillAmount, 1f), 
            
            // Overflow
            new PropertyGenerator<Overflow>(StylePropertyId.OverflowX, Overflow.Visible),
            new PropertyGenerator<Overflow>(StylePropertyId.OverflowY, Overflow.Visible),
            new PropertyGenerator<ClipBehavior>(StylePropertyId.ClipBehavior, ClipBehavior.Normal, InheritanceType.Inherited),
            new PropertyGenerator<ClipBounds>(StylePropertyId.ClipBounds, ClipBounds.BorderBox), // InheritanceType.Inherited),

            new PropertyGenerator<PointerEvents>(StylePropertyId.PointerEvents, PointerEvents.Normal, InheritanceType.Inherited),

            // Background
            new AnimatedPropertyGenerator<Color>(StylePropertyId.BackgroundColor, ColorUtil.UnsetValue),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.BackgroundTint, ColorUtil.UnsetValue, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BackgroundImageOffsetX, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BackgroundImageOffsetY, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundImageScaleX, 1),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundImageScaleY, 1),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundImageTileX, 1),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundImageTileY, 1),
            new AnimatedPropertyGenerator<float>(StylePropertyId.BackgroundImageRotation, 0),
            new PropertyGenerator<Texture2D>(StylePropertyId.BackgroundImage, null),
            new PropertyGenerator<BackgroundFit>(StylePropertyId.BackgroundFit, BackgroundFit.Fill),

            // Border
            new AnimatedPropertyGenerator<Color>(StylePropertyId.BorderColorTop, ColorUtil.UnsetValue),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.BorderColorRight, ColorUtil.UnsetValue),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.BorderColorBottom, ColorUtil.UnsetValue),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.BorderColorLeft, ColorUtil.UnsetValue),
            
            // Corner
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.CornerBevelTopLeft, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.CornerBevelTopRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.CornerBevelBottomRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.CornerBevelBottomLeft, new UIFixedLength(0)),
            
            // Flex Item
            new AnimatedPropertyGenerator<int>(StylePropertyId.FlexItemGrow, 0),
            new AnimatedPropertyGenerator<int>(StylePropertyId.FlexItemShrink, 0),

            // Flex Layout
            new PropertyGenerator<LayoutDirection>(StylePropertyId.FlexLayoutDirection, LayoutDirection.Vertical),
            new PropertyGenerator<LayoutWrap>(StylePropertyId.FlexLayoutWrap, LayoutWrap.None),
            new AnimatedPropertyGenerator<float>(StylePropertyId.FlexLayoutGapHorizontal, 0),
            new AnimatedPropertyGenerator<float>(StylePropertyId.FlexLayoutGapVertical, 0),

            // Grid Item
            new PropertyGenerator<GridItemPlacement>(StylePropertyId.GridItemX, new GridItemPlacement(-1)),
            new PropertyGenerator<GridItemPlacement>(StylePropertyId.GridItemY, new GridItemPlacement(-1)),
            new PropertyGenerator<GridItemPlacement>(StylePropertyId.GridItemWidth, new GridItemPlacement(1)),
            new PropertyGenerator<GridItemPlacement>(StylePropertyId.GridItemHeight, new GridItemPlacement(1)),

            // Grid Layout
            new PropertyGenerator<LayoutDirection>(StylePropertyId.GridLayoutDirection, LayoutDirection.Horizontal),
            new PropertyGenerator<GridLayoutDensity>(StylePropertyId.GridLayoutDensity, GridLayoutDensity.Sparse),
            new PropertyGenerator<IReadOnlyList<GridTrackSize>>(StylePropertyId.GridLayoutColTemplate, ListPool<GridTrackSize>.Empty, InheritanceType.NotInherited, "ListPool<GridTrackSize>.Empty"),
            new PropertyGenerator<IReadOnlyList<GridTrackSize>>(StylePropertyId.GridLayoutRowTemplate, ListPool<GridTrackSize>.Empty, InheritanceType.NotInherited, "ListPool<GridTrackSize>.Empty"),
            new PropertyGenerator<IReadOnlyList<GridTrackSize>>(StylePropertyId.GridLayoutColAutoSize, ListPool<GridTrackSize>.Empty, InheritanceType.NotInherited, "new List<GridTrackSize>() {GridTrackSize.Default}"),
            new PropertyGenerator<IReadOnlyList<GridTrackSize>>(StylePropertyId.GridLayoutRowAutoSize, ListPool<GridTrackSize>.Empty, InheritanceType.NotInherited, "new List<GridTrackSize>() {GridTrackSize.Default}"),
            new AnimatedPropertyGenerator<float>(StylePropertyId.GridLayoutColGap, 0),
            new AnimatedPropertyGenerator<float>(StylePropertyId.GridLayoutRowGap, 0),
            new PropertyGenerator<GridAxisAlignment>(StylePropertyId.GridLayoutColAlignment, GridAxisAlignment.Grow),
            new PropertyGenerator<GridAxisAlignment>(StylePropertyId.GridLayoutRowAlignment, GridAxisAlignment.Grow),
            
            // Layout Alignment
            new PropertyGenerator<float>(StylePropertyId.AlignItemsHorizontal, 0),
            new PropertyGenerator<float>(StylePropertyId.AlignItemsVertical, 0),
            new PropertyGenerator<LayoutFit>(StylePropertyId.FitItemsVertical, 0),
            new PropertyGenerator<LayoutFit>(StylePropertyId.FitItemsHorizontal, 0),
            new PropertyGenerator<SpaceDistribution>(StylePropertyId.DistributeExtraSpaceHorizontal, SpaceDistribution.AfterContent),
            new PropertyGenerator<SpaceDistribution>(StylePropertyId.DistributeExtraSpaceVertical, SpaceDistribution.AfterContent),

            // Radial Layout
            new AnimatedPropertyGenerator<float>(StylePropertyId.RadialLayoutStartAngle, 0f),
            new AnimatedPropertyGenerator<float>(StylePropertyId.RadialLayoutEndAngle, 360f),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.RadialLayoutRadius, new UIFixedLength(0.5f, UIFixedUnit.Percent)),

            // Alignment
            new PropertyGenerator<AlignmentDirection>(StylePropertyId.AlignmentDirectionX, AlignmentDirection.Start),
            new PropertyGenerator<AlignmentDirection>(StylePropertyId.AlignmentDirectionY, AlignmentDirection.Start),
            new PropertyGenerator<AlignmentTarget>(StylePropertyId.AlignmentTargetX, AlignmentTarget.Unset),
            new PropertyGenerator<AlignmentTarget>(StylePropertyId.AlignmentTargetY, AlignmentTarget.Unset),
            new AnimatedPropertyGenerator<OffsetMeasurement>(StylePropertyId.AlignmentOriginX, new OffsetMeasurement(0)),
            new AnimatedPropertyGenerator<OffsetMeasurement>(StylePropertyId.AlignmentOriginY, new OffsetMeasurement(0)),
            new AnimatedPropertyGenerator<OffsetMeasurement>(StylePropertyId.AlignmentOffsetX, new OffsetMeasurement(0)),
            new AnimatedPropertyGenerator<OffsetMeasurement>(StylePropertyId.AlignmentOffsetY, new OffsetMeasurement(0)),
            new PropertyGenerator<AlignmentBoundary>(StylePropertyId.AlignmentBoundaryX, AlignmentBoundary.Unset),
            new PropertyGenerator<AlignmentBoundary>(StylePropertyId.AlignmentBoundaryY, AlignmentBoundary.Unset),

            // Fit
            new PropertyGenerator<LayoutFit>(StylePropertyId.LayoutFitHorizontal, LayoutFit.Unset),
            new PropertyGenerator<LayoutFit>(StylePropertyId.LayoutFitVertical, LayoutFit.Unset),

            // Size
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MinWidth, new UIMeasurement(0)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MaxWidth, new UIMeasurement(float.MaxValue)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.PreferredWidth, UIMeasurement.Content100),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MinHeight, new UIMeasurement(0)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.MaxHeight, new UIMeasurement(float.MaxValue)),
            new AnimatedPropertyGenerator<UIMeasurement>(StylePropertyId.PreferredHeight, UIMeasurement.Content100),

            // Margin
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.MarginTop, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.MarginRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.MarginBottom, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.MarginLeft, new UIFixedLength(0)),

            // Border
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderTop, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderBottom, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderLeft, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderRadiusTopLeft, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderRadiusTopRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderRadiusBottomRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.BorderRadiusBottomLeft, new UIFixedLength(0)),

            // Padding
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.PaddingTop, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.PaddingRight, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.PaddingBottom, new UIFixedLength(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.PaddingLeft, new UIFixedLength(0)),

            // Text
            new AnimatedPropertyGenerator<Color>(StylePropertyId.TextColor, Color.black, InheritanceType.Inherited),
            new PropertyGenerator<FontAsset>(StylePropertyId.TextFontAsset, FontAsset.defaultFontAsset, InheritanceType.Inherited, "FontAsset.defaultFontAsset"),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.TextFontSize, 18, InheritanceType.Inherited),
            new PropertyGenerator<FontStyle>(StylePropertyId.TextFontStyle, FontStyle.Normal, InheritanceType.Inherited),
            new PropertyGenerator<TextAlignment>(StylePropertyId.TextAlignment, TextAlignment.Left, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextOutlineWidth, 0, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.TextOutlineColor, Color.black, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.CaretColor, Color.black, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.SelectionBackgroundColor, new Color32(184, 255, 255,  255), InheritanceType.Inherited),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.SelectionTextColor, Color.black, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextOutlineSoftness, 0f, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.TextGlowColor, ColorUtil.UnsetValue, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextGlowOffset, 0, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextGlowInner, 0, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextGlowOuter, 0, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextGlowPower, 0, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.TextUnderlayColor, ColorUtil.UnsetValue, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextUnderlayX, 0, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextUnderlayY, 0, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextUnderlayDilate, 0f, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextUnderlaySoftness, 0f, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TextFaceDilate, 0f, InheritanceType.Inherited),
            new PropertyGenerator<UnderlayType>(StylePropertyId.TextUnderlayType, UnderlayType.Unset, InheritanceType.Inherited),
            new PropertyGenerator<TextTransform>(StylePropertyId.TextTransform, TextTransform.None, InheritanceType.Inherited),
            new PropertyGenerator<WhitespaceMode>(StylePropertyId.TextWhitespaceMode, WhitespaceMode.CollapseWhitespace | WhitespaceMode.Trim, InheritanceType.Inherited),

            // Transform
            new AnimatedPropertyGenerator<OffsetMeasurement>(StylePropertyId.TransformPositionX, new OffsetMeasurement(0)),
            new AnimatedPropertyGenerator<OffsetMeasurement>(StylePropertyId.TransformPositionY, new OffsetMeasurement(0)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.TransformPivotX, new UIFixedLength(0.5f, UIFixedUnit.Percent)),
            new AnimatedPropertyGenerator<UIFixedLength>(StylePropertyId.TransformPivotY, new UIFixedLength(0.5f, UIFixedUnit.Percent)),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TransformScaleX, 1),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TransformScaleY, 1),
            new AnimatedPropertyGenerator<float>(StylePropertyId.TransformRotation, 0),

            // Layout
            new PropertyGenerator<LayoutType>(StylePropertyId.LayoutType, LayoutType.Flex),
            new PropertyGenerator<LayoutBehavior>(StylePropertyId.LayoutBehavior, LayoutBehavior.Normal),
            new PropertyGenerator<ScrollBehavior>(StylePropertyId.ScrollBehaviorX, ScrollBehavior.Default), 
            new PropertyGenerator<ScrollBehavior>(StylePropertyId.ScrollBehaviorY, ScrollBehavior.Default),
            new AnimatedPropertyGenerator<int>(StylePropertyId.ZIndex, 0, InheritanceType.Inherited),
            new AnimatedPropertyGenerator<int>(StylePropertyId.RenderLayerOffset, 0),
            new AnimatedPropertyGenerator<RenderLayer>(StylePropertyId.RenderLayer, RenderLayer.Default),
            new AnimatedPropertyGenerator<int>(StylePropertyId.Layer, 0, InheritanceType.Inherited),

            // Shadow
//            new PropertyGenerator<UnderlayType>(StylePropertyId.ShadowType, UnderlayType.Unset),
            new AnimatedPropertyGenerator<Color>(StylePropertyId.ShadowColor, ColorUtil.UnsetValue), 
            new AnimatedPropertyGenerator<Color>(StylePropertyId.ShadowTint, ColorUtil.UnsetValue), 
            new AnimatedPropertyGenerator<OffsetMeasurement>(StylePropertyId.ShadowOffsetX, 0),
            new AnimatedPropertyGenerator<OffsetMeasurement>(StylePropertyId.ShadowOffsetY, 0),
            new AnimatedPropertyGenerator<float>(StylePropertyId.ShadowSizeX, 0f),
            new AnimatedPropertyGenerator<float>(StylePropertyId.ShadowSizeY, 0f),
            new AnimatedPropertyGenerator<float>(StylePropertyId.ShadowIntensity, 10),
            new AnimatedPropertyGenerator<float>(StylePropertyId.ShadowOpacity, 1f),
        };

        [MenuItem("UIForia Dev/Regenerate Style Stuff (Don't!)")]
        public static void GenerateStyleProxies() {
            string generatedPath = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../Packages/UIForia/Src/_Generated1.cs"));
            string generatedPath3 = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../Packages/UIForia/Src/_Generated3.cs"));

            string template = @"
using UIForia.Util;

namespace UIForia.Rendering {
    
    public partial struct UIStyleSetStateProxy {
        __REPLACE__UIStyleSetStateProxy__
    }

    public partial class UIStyle {
    
        __REPLACE__UIStyle
        
    }

    public partial class UIStyleSet {
    
        __REPLACE__UIStyleSet_Properties

        __REPLACE__UIStyleSet_Methods

        public StyleProperty GetComputedStyleProperty(StylePropertyId propertyId) {
        __REPLACE_UIStyleSet_GetComputed  
        }

    }

    public static partial class StyleUtil {
        
        public static System.Type GetPropertyType(StylePropertyId propertyId) {
            switch (propertyId) {

            __REPLACE_StyleUtil_GetPropertyType
            }

            return null;

        }

      public static bool CanAnimate(StylePropertyId propertyId) {
                switch (propertyId) {
    
__REPLACE_StyleUtil__CanAnimate
                }
    
                return false;
            }

        public static bool IsInherited(StylePropertyId propertyId) {
            switch (propertyId) {

__REPLACE_StyleUtil__IsInherited
            }

            return false;
        }

    }

}";
            

            string retn = "";
            for (int i = 0; i < properties.Length; i++) {
                retn += InflatePropertyTemplate(properties[i]);
            }

            template = template.Replace("__REPLACE__UIStyleSetStateProxy__", retn);
            retn = "";

            for (int i = 0; i < properties.Length; i++) {
                retn += InflateUIStyleSetProperties(properties[i]);
            }

            template = template.Replace("__REPLACE__UIStyleSet_Properties", retn);
            retn = "";

            for (int i = 0; i < properties.Length; i++) {
                retn += InflateStyleSetMethods(properties[i]);
            }

            template = template.Replace("__REPLACE__UIStyleSet_Methods", retn);
            retn = GetComputedStyle();

            template = template.Replace("__REPLACE_UIStyleSet_GetComputed", retn);
            retn = "";

            for (int i = 0; i < properties.Length; i++) {
                if (properties[i].inheritanceType == InheritanceType.Inherited) {
                    retn += $"                    case StylePropertyId.{properties[i].propertyIdName}: return true;\n";
                }
            }
            template = template.Replace("__REPLACE_StyleUtil__IsInherited", retn);
            retn = "";
            
            for (int i = 0; i < properties.Length; i++) {
                if (!properties[i].type.IsGenericType) {
                    retn += $"                    case StylePropertyId.{properties[i].propertyIdName}: return typeof({properties[i].type});\n";
                }
                else {
                    retn += $"                    case StylePropertyId.{properties[i].propertyIdName}: return typeof(System.Collections.Generic.IReadOnlyList<{typeof(GridTrackSize)}>);\n";
                }
            }
            
            template = template.Replace("__REPLACE_StyleUtil_GetPropertyType", retn);
            retn = "";

            for (int i = 0; i < properties.Length; i++) {
                if (properties[i] is AnimatedPropertyGenerator) {
                    retn += $"                    case StylePropertyId.{properties[i].propertyIdName}: return true;\n";
                }
            }

            template = template.Replace("__REPLACE_StyleUtil__CanAnimate", retn);
            retn = "";

            for (int i = 0; i < properties.Length; i++) {
                retn += UIStyle_Property(properties[i]);
            }

            template = template.Replace("__REPLACE__UIStyle", retn);

            File.WriteAllText(generatedPath, template);
            

            string code = @"using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using System.Collections.Generic;
using UnityEngine;
using UIForia.Util;
using UIForia.Text;
using UIForia.UIInput;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

// Do not edit this file. See CodeGen.cs instead.

namespace UIForia.Rendering {

    public static class DefaultStyleValues_Generated {

";

            for (int i = 0; i < properties.Length; i++) {
                code += "\t\t" + DefaultValue(properties[i]);
            }

            code += DefaultGetValue();

            code += " \n}\n}";

            File.WriteAllText(generatedPath3, code);
        }



        private static string GetComputedStyle() {
            string code = "\t\t\tswitch(propertyId) {\n";

            for (int i = 0; i < properties.Length; i++) {
                code += $"\t\t\t\tcase {nameof(StylePropertyId)}.{properties[i].propertyIdName}:\n";
                code += $"\t\t\t\t\t return {properties[i].StyleSetGetComputed};\n";
            }

            code += "\t\t\t\tdefault: throw new System.ArgumentOutOfRangeException(nameof(propertyId), propertyId, null);\n";
            code += "\t\t\t\t}";
            return code;
        }

        private static string InflateStyleSetMethods(PropertyGenerator propertyGenerator) {
            bool isStruct = propertyGenerator.type.IsValueType;
            if (isStruct) {
                return $@"
        public void Set{propertyGenerator.propertyIdName}(in {propertyGenerator.GetFullTypeName()}? value, {nameof(StyleState)} state) {{
            {propertyGenerator.GetStyleSetSetter()};
        }}

        public {propertyGenerator.GetFullTypeName()} Get{propertyGenerator.propertyIdName}({nameof(StyleState)} state) {{
            return {propertyGenerator.GetStyleSetGetter()};
        }}
        ";
            }

            return $@"
        public void Set{propertyGenerator.propertyIdName}({propertyGenerator.GetFullTypeName()} value, {nameof(StyleState)} state) {{
            {propertyGenerator.GetStyleSetSetter()};
        }}

        public {propertyGenerator.GetFullTypeName()} Get{propertyGenerator.propertyIdName}({nameof(StyleState)} state) {{
            return {propertyGenerator.GetStyleSetGetter()};
        }}
        ";
        }

        private static string InflatePropertyTemplate(PropertyGenerator propertyGenerator) {
            const string propertyTemplate = @"
        public __TYPE__ __NAME__ {
            [System.Diagnostics.DebuggerStepThrough]
            get { return m_StyleSet.GetPropertyValueInState(StylePropertyId.__NAME__, state).As__CAST_TYPE__; }
            [System.Diagnostics.DebuggerStepThrough]
            set { m_StyleSet.SetProperty(__CONSTRUCTOR__, state); }
        }
        ";

            return propertyTemplate.Replace("__TYPE__", propertyGenerator.GetFullTypeName()).Replace("__NAME__", propertyGenerator.propertyIdName).Replace("__CAST_TYPE__", propertyGenerator.GetCastAccessor()).Replace("__CONSTRUCTOR__", propertyGenerator.StylePropertyConstructor);
        }

        private static string InflateUIStyleSetProperties(PropertyGenerator propertyGenerator) {
            string propertyTemplate;

            if (propertyGenerator.inheritanceType == InheritanceType.Inherited) {
                propertyTemplate = @"

            public __TYPE__ __NAME__ { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.__NAME__, out property)) return property.As__CAST_TYPE__;
                    if (propertyMap.TryGetValue(BitUtil.SetHighLowBits(1, (int) StylePropertyId.__NAME__), out property)) return property.As__CAST_TYPE__;
                    return DefaultStyleValues_Generated.__NAME__;
                }
            }";
            }
            else {
                propertyTemplate = @"

            public __TYPE__ __NAME__ { 
                [System.Diagnostics.DebuggerStepThrough]
                get { 
                    StyleProperty property;
                    if (propertyMap.TryGetValue((int) StylePropertyId.__NAME__, out property)) return property.As__CAST_TYPE__;
                    return DefaultStyleValues_Generated.__NAME__;
                }
            }";
            }

            return propertyTemplate.Replace("__TYPE__", propertyGenerator.GetFullTypeName()).Replace("__NAME__", propertyGenerator.propertyIdName).Replace("__CAST_TYPE__", propertyGenerator.GetCastAccessor()).Replace("__CONSTRUCTOR__", propertyGenerator.StylePropertyConstructor);
        }

        private static string DefaultGetValue() {
            string code = "\t\tpublic static StyleProperty GetPropertyValue(StylePropertyId propertyId) {\n\n";

            code += "\t\t\tswitch(propertyId) {\n";

            for (int i = 0; i < properties.Length; i++) {
                code += $"\t\t\t\tcase {nameof(StylePropertyId)}.{properties[i].propertyIdName}:\n";
                code += $"\t\t\t\t\t return {properties[i].AsStyleProperty};\n";
            }

            code += "\t\t\t\tdefault: throw new System.ArgumentOutOfRangeException(nameof(propertyId), propertyId, null);\n";
            code += "\t\t\t\t}\n}";
            return code;
        }

        private static string UIStyle_Property(PropertyGenerator propertyGenerator) {
            string template = $@"
        public __TYPE__ __NAME__ {{
            [System.Diagnostics.DebuggerStepThrough]
            get {{ return {UIStyle_GetReaderType(propertyGenerator)} }}
            [System.Diagnostics.DebuggerStepThrough]
            set {{ SetProperty({propertyGenerator.StylePropertyConstructor}); }}
        }}
            ";
            return template.Replace("__TYPE__", propertyGenerator.GetFullTypeName()).Replace("__NAME__", propertyGenerator.propertyIdName);
        }

        private static string DefaultValue(PropertyGenerator propertyGenerator) {
            if (propertyGenerator.type.IsEnum || typeof(int) == propertyGenerator.type || typeof(float) == propertyGenerator.type) {
                return $"public const {propertyGenerator.GetTypeName()} {propertyGenerator.propertyIdName} = {propertyGenerator.GetDefaultValue()};\n";
            }
            else {
                return $"public static readonly {propertyGenerator.GetTypeName()} {propertyGenerator.propertyIdName} = {propertyGenerator.GetDefaultValue()};\n";
            }
        }

        public static string UIStyle_GetReaderType(PropertyGenerator propertyGenerator) {
            if (propertyGenerator.type.IsEnum) {
                return $"({propertyGenerator.GetFullTypeName()})FindEnumProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }

            if (typeof(MaterialId) == propertyGenerator.type) {
                return $"FindMaterialIdProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }
            
            if (typeof(int) == propertyGenerator.type) {
                return $"FindIntProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }

            if (typeof(float) == propertyGenerator.type) {
                return $"FindFloatProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }

            if (typeof(Color) == propertyGenerator.type) {
                return $"FindColorProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }

            if (typeof(UIFixedLength) == propertyGenerator.type) {
                return $"FindUIFixedLengthProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }

            if (typeof(UIMeasurement) == propertyGenerator.type) {
                return $"FindUIMeasurementProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }

            if (typeof(OffsetMeasurement) == propertyGenerator.type) {
                return $"Find{nameof(OffsetMeasurement)}Property(StylePropertyId.{propertyGenerator.propertyIdName});";
            }

            if (typeof(GridTrackSize) == propertyGenerator.type) {
                return $"FindGridTrackSizeProperty(StylePropertyId.{propertyGenerator.propertyIdName});";
            }

            if (typeof(FontAsset) == propertyGenerator.type) {
                return $"GetProperty(StylePropertyId.{propertyGenerator.propertyIdName}).AsFont;";
            }

            if (typeof(Texture2D) == propertyGenerator.type) {
                return $"GetProperty(StylePropertyId.{propertyGenerator.propertyIdName}).AsTexture2D;";
            }

            if (typeof(IReadOnlyList<GridTrackSize>) == propertyGenerator.type) {
                return $"GetProperty(StylePropertyId.{propertyGenerator.propertyIdName}).AsGridTemplate;";
            }

            if (typeof(CursorStyle) == propertyGenerator.type) {
                return $"GetProperty(StylePropertyId.{propertyGenerator.propertyIdName}).AsCursorStyle;";
            }

            if (typeof(string) == propertyGenerator.type) {
                return $"GetProperty(StylePropertyId.{propertyGenerator.propertyIdName}).AsString;";
            }

            if (typeof(GridItemPlacement) == propertyGenerator.type) {
                return $"GetProperty(StylePropertyId.{propertyGenerator.propertyIdName}).AsGridItemPlacement;";
            }

            throw new ArgumentOutOfRangeException($"Don't know what to do with {propertyGenerator.type}.");
        }

    }

}