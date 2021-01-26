using System;
using UIForia;
using UIForia.Text;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace SVGX {

    internal struct SVGXInheritedTextStyle {

        public FontAsset fontAsset;

        public FontStyle fontStyle;
        public TextAlignment alignment;
        public TextTransform textTransform;
        public WhitespaceMode whitespaceMode;

        public Color32 textColor;
        public Color32 outlineColor;
        public Color32 glowColor;
        public Color32 underlayColor;

        public float fontSize;
        public float outlineWidth;
        public float outlineSoftness;
        public float glowOuter;
        public float glowOffset;
        public float underlayX;
        public float underlayY;
        public float underlayDilate;
        public float underlaySoftness;
        public float faceDilate;

    }

    public struct SVGXTextStyle {

        public FontAsset fontAsset;

        // todo -- merge values where possible
        public FontStyle? fontStyle;
        public TextAlignment? alignment;
        public TextTransform? textTransform;
        public WhitespaceMode? whitespaceMode;

        public Color32? textColor;
        public Color32? outlineColor;
        public Color32? glowColor;
        public Color32? underlayColor;

        // todo -- use byte for things that can fit in a byte
        public float? fontSize;
        public float? outlineWidth;
        public float? outlineSoftness;
        public float? glowOuter;
        public float? glowOffset;
        public float? underlayX;
        public float? underlayY;
        public float? underlayDilate;
        public float? underlaySoftness;
        public float? faceDilate;

        // todo use these instead of nullables
        [Flags]
        public enum SetFlags {

            FontAsset = 1 << 0,
            FontStyle = 1 << 1,
            Alignment = 1 << 2,

        }

        public SVGXTextStyle(SVGXTextStyle toClone) {
            this.fontSize = toClone.fontSize;
            this.fontStyle = toClone.fontStyle;
            this.alignment = toClone.alignment;
            this.textTransform = toClone.textTransform;
            this.whitespaceMode = toClone.whitespaceMode;
            this.textColor = toClone.textColor;
            this.outlineColor = toClone.outlineColor;
            this.outlineWidth = toClone.outlineWidth;
            this.outlineSoftness = toClone.outlineSoftness;
            this.glowOuter = toClone.glowOuter;
            this.glowOffset = toClone.glowOffset;
            this.glowColor = toClone.glowColor;
            this.underlayColor = toClone.underlayColor;
            this.underlayX = toClone.underlayX;
            this.underlayY = toClone.underlayY;
            this.underlayDilate = toClone.underlayDilate;
            this.underlaySoftness = toClone.underlaySoftness;
            this.fontAsset = toClone.fontAsset;
            this.faceDilate = toClone.faceDilate;
        }

    }

}