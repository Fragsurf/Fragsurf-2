using System;
using System.Collections.Generic;
using SVGX;
using TMPro;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Text {

    public class TextSpan {

        internal SVGXTextStyle textStyle;
        internal SVGXInheritedTextStyle inheritedStyle;
        internal float scaleRatioA;
        internal float scaleRatioB;
        internal float scaleRatioC;

        internal TextSpan parent;
        public bool inheritStyleProperties;

        internal TextSpan firstChild;
        internal TextSpan nextSibling;
        internal StructList<CharInfo> charInfoList;
        internal StructList<WordInfo> wordInfoList;
        internal StructList<TextGeometry> geometryList;
        internal TextInfo textInfo;

        internal char[] rawContent;
        internal int rawContentSize;
        internal char[] processedContent;

        internal RebuildFlag rebuildFlags;

        public int geometryVersion;
        public bool isEnabled;
        public float longestWordSize;
        public bool isInline;

        [Flags]
        internal enum RebuildFlag {

            Content = 1 << 0,
            UVCoords = 1 << 1,
            FontSize = 1 << 3,
            Glyphs = 1 << 4,
            Whitespace = 1 << 5,
            Casing = 1 << 6,
            Positions = Content | UVCoords | Glyphs | Whitespace | Casing | FontSize,
            All = Content | UVCoords | Glyphs | Whitespace | Casing | FontSize

        }

        internal TextSpan() {
            this.isEnabled = true;
            this.rebuildFlags = RebuildFlag.All;
        }

        public TextSpan(string content, in SVGXTextStyle textStyle, bool inheritStyleProperties = false) {
            this.textStyle = textStyle;
            this.isEnabled = true;
            this.rebuildFlags = RebuildFlag.All;
            this.inheritStyleProperties = inheritStyleProperties;
            SetRawContentFromString(content);
        }

        public void AddChild(TextSpan child) {
            if (child.parent == this) return;

            if (child.parent != null || child.textInfo != null) {
                return;
            }

            if (firstChild == null) {
                firstChild = child;
            }
            else {
                TextSpan ptr = firstChild;
                while (ptr.nextSibling != null) {
                    ptr = ptr.nextSibling;
                }

                ptr.nextSibling = child;
            }

            child.textInfo = textInfo;
            child.parent = this;
            textInfo.RebuildSpans();
            if (inheritStyleProperties) {
                child.inheritedStyle = Merge(inheritedStyle);
            }
        }

        public void InsertChild(TextSpan child, uint index) {
            throw new NotImplementedException();
        }

        public void RemoveChild(TextSpan child) {
            if (child == null || child.parent != this || firstChild == null) {
                return;
            }

            if (firstChild == child) {
                child.parent = null;
                firstChild = null;
            }
            else {
                TextSpan ptr = firstChild.nextSibling;
                TextSpan trail = firstChild;
                while (ptr != child && ptr != null) {
                    trail = ptr;
                    ptr = ptr.nextSibling;
                }

                if (ptr == child) {
                    trail.nextSibling = child.nextSibling;
                    child.nextSibling = null;
                }
            }

            child.textInfo = null;
            child.parent = null;
            textInfo.RebuildSpans();
        }


        // todo -- handle this later
//            for (int i = 0; i < characters.Length; i++) {
//                if (characters[i] == '&') {
//                    // c# provides System.Net.WebUtility.HtmlDecode("&eacute;");
//                    // returns a string not a character and allocates a lot, better not to use it
//                    int advance = TryParseSymbol(characters, i);
//                    if (advance != 0) {
//                        i += advance;
//                        // adjust buffer size? not sure how to handle these yet
//                    }
//                }
//            }


        // todo -- support this, need an Append buffer method, need to merge old words, can save on layout, etch
        public void AppendText(string text) { }

        // todo -- support this, need an Append buffer method, need to merge old words, can save on layout, etch
        public void AppendText(char[] text) { }

        public void SetText(IList<string> content) {
            int size = 0;

            for (int i = 0; i < content.Count; i++) {
                size += content[i].Length;
            }

            if (rawContent == null) {
                rawContent = new char[size];
            }
            else if (size > rawContent.Length) {
                Array.Resize(ref rawContent, size);
            }

            int idx = 0;
            for (int i = 0; i < content.Count; i++) {
                string current = content[i];
                for (int j = 0; j < current.Length; j++) {
                    rawContent[idx++] = current[j];
                }
            }

            rawContentSize = size;

            UpdateBuffers();
        }

        public void SetText(IList<char[]> content) {
            int size = 0;

            for (int i = 0; i < content.Count; i++) {
                size += content[i].Length;
            }

            if (rawContent == null) {
                rawContent = new char[size];
            }
            else if (size > rawContent.Length) {
                Array.Resize(ref rawContent, size);
            }

            int idx = 0;
            for (int i = 0; i < content.Count; i++) {
                char[] current = content[i];
                for (int j = 0; j < current.Length; j++) {
                    rawContent[idx++] = current[j];
                }
            }

            rawContentSize = size;

            UpdateBuffers();
        }

        public void SetText(string text) {
            
            if (text == null) {
                text = string.Empty;
            }
            
            if (rawContent == null) {
                rawContent = new char[text.Length];
            }
            else if (text.Length > rawContent.Length) {
                Array.Resize(ref rawContent, text.Length);
            }

            SetRawContentFromString(text);

            UpdateBuffers();
            
        }

        public void SetText(char[] characters) {
            if (rawContent == null) {
                rawContent = new char[characters.Length];
            }
            else if (characters.Length > rawContent.Length) {
                Array.Resize(ref rawContent, characters.Length);
            }

            Array.Copy(characters, 0, rawContent, 0, characters.Length);

            rawContentSize = characters.Length;

            UpdateBuffers();
        }

        private void UpdateBuffers() {
            int bufferSize = TextUtil.ProcessWhitespace(whitespaceMode, ref processedContent, rawContent, rawContentSize);

            if (bufferSize == 0 && geometryVersion != 0) {
                geometryVersion = 0;
            }
            
            // handle all kerning within a span as if there were no other spans. text info will handle 'gluing' spans together
            if (charInfoList == null) {
                charInfoList = new StructList<CharInfo>(bufferSize);
            }
            else {
                charInfoList.EnsureCapacity(bufferSize);
                charInfoList.size = bufferSize;
            }

            TextUtil.TransformText(textTransform, processedContent, bufferSize); // affect kerning & glyph size

            CharInfo[] charInfos = charInfoList.array;
            charInfoList.size = bufferSize;

            for (int i = 0; i < bufferSize; i++) {
                charInfos[i].visible = false;// technically safer to set to default but this faster
                charInfos[i].character = processedContent[i];
            }

            wordInfoList = TextUtil.BreakIntoWords(wordInfoList, processedContent, bufferSize); // affected by content, whitespace

            // if style changes but text does not, can skip this part, (if transform or whitespace changed we need to though)
            FindGlyphs(fontAsset, charInfos, bufferSize); // affected by text transform, content, font
            FindKerningInfo(fontAsset, charInfos, bufferSize); // affected by text transform, content, font

            ComputeWordAndCharacterSizes(charInfoList, wordInfoList); // affected by font size, font

            textInfo.RebuildSpans();
        }

        public void SetStyle(in SVGXTextStyle textStyle) {
            this.textStyle = textStyle;
            rebuildFlags = RebuildFlag.All;
            textInfo.RebuildSpans();
            // todo -- inherit!
        }

        private SVGXInheritedTextStyle Merge(in SVGXInheritedTextStyle style) {
            SVGXInheritedTextStyle retn;
            retn.fontSize = textStyle.fontSize ?? style.fontSize;
            retn.fontAsset = textStyle.fontAsset ?? style.fontAsset;
            retn.fontStyle = textStyle.fontStyle ?? style.fontStyle;
            retn.faceDilate = textStyle.faceDilate ?? style.faceDilate;

            retn.textTransform = textStyle.textTransform ?? style.textTransform;
            retn.whitespaceMode = textStyle.whitespaceMode ?? style.whitespaceMode;
            retn.alignment = textStyle.alignment ?? style.alignment;

            retn.outlineWidth = textStyle.outlineWidth ?? style.outlineWidth;
            retn.outlineSoftness = textStyle.outlineSoftness ?? style.outlineSoftness;
            retn.outlineColor = textStyle.outlineColor ?? style.outlineColor;

            retn.glowOffset = textStyle.glowOffset ?? style.glowOffset;
            retn.glowOuter = textStyle.glowOuter ?? style.glowOuter;

            retn.underlayDilate = textStyle.underlayDilate ?? style.underlayDilate;
            retn.underlayX = textStyle.underlayX ?? style.underlayX;
            retn.underlayY = textStyle.underlayY ?? style.underlayY;
            retn.underlaySoftness = textStyle.underlaySoftness ?? style.underlaySoftness;

            retn.textColor = textStyle.textColor ?? style.textColor;
            retn.glowColor = textStyle.glowColor ?? style.glowColor;
            retn.underlayColor = textStyle.underlayColor ?? style.underlayColor;

            return retn;
        }

        public TextTransform textTransform {
            get => textStyle.textTransform ?? inheritedStyle.textTransform;
        }

        public FontAsset fontAsset {
            get => textStyle.fontAsset ?? inheritedStyle.fontAsset;
        }

        public FontStyle fontStyle {
            get => textStyle.fontStyle ?? inheritedStyle.fontStyle;
        }

        public float fontSize {
            get => textStyle.fontSize ?? inheritedStyle.fontSize;
        }

        public WhitespaceMode whitespaceMode {
            get => textStyle.whitespaceMode ?? inheritedStyle.whitespaceMode;
        }

        public TextAlignment alignment {
            get => textStyle.alignment ?? inheritedStyle.alignment;
        }

        public float outlineWidth {
            get => textStyle.outlineWidth ?? inheritedStyle.outlineWidth;
        }

        public float outlineSoftness {
            get => textStyle.outlineSoftness ?? inheritedStyle.outlineSoftness;
        }

        public float faceDilate {
            get => textStyle.faceDilate ?? inheritedStyle.faceDilate;
        }

        public float glowOffset {
            get => textStyle.glowOffset ?? inheritedStyle.glowOffset;
        }

        public float glowOuter {
            get => textStyle.glowOuter ?? inheritedStyle.glowOuter;
        }

        public float underlayDilate {
            get => textStyle.underlayDilate ?? inheritedStyle.underlayDilate;
        }

        public float underlaySoftness {
            get => textStyle.underlaySoftness ?? inheritedStyle.underlaySoftness;
        }

        public float underlayX {
            get => textStyle.underlayX ?? inheritedStyle.underlayX;
        }

        public float underlayY {
            get => textStyle.underlayY ?? inheritedStyle.underlayY;
        }

        public Color32 textColor {
            get => textStyle.textColor ?? inheritedStyle.textColor;
        }

        public Color32 outlineColor {
            get => textStyle.outlineColor ?? inheritedStyle.outlineColor;
        }

        public Color32 underlayColor {
            get => textStyle.underlayColor ?? inheritedStyle.underlayColor;
        }

        public Color32 glowColor {
            get => textStyle.glowColor ?? inheritedStyle.glowColor;
        }

        private TextDisplayData GetTextDisplayData() {
            TextDisplayData textDisplayData;
            textDisplayData.fontAsset = fontAsset;
            textDisplayData.outlineWidth = outlineWidth;
            textDisplayData.outlineSoftness = outlineSoftness;
            textDisplayData.faceDilate = faceDilate;
            textDisplayData.glowOffset = glowOffset;
            textDisplayData.glowOuter = glowOuter;
            textDisplayData.underlayDilate = underlayDilate;
            textDisplayData.underlaySoftness = underlaySoftness;
            textDisplayData.underlayX = underlayX;
            textDisplayData.underlayY = underlayY;
            return textDisplayData;
        }

        // feature: word & character spacing modifier
        private void ComputeWordAndCharacterSizes(StructList<CharInfo> characterList, StructList<WordInfo> wordInfoList) {
            TextDisplayData textDisplayData = GetTextDisplayData();

            float smallCapsMultiplier = (textTransform == TextTransform.SmallCaps) ? 0.8f : 1f;
            float fontScale = fontSize * smallCapsMultiplier / fontAsset.faceInfo.pointSize * fontAsset.faceInfo.scale;

            Vector3 ratios = TextUtil.ComputeRatios(textDisplayData);

            scaleRatioA = ratios.x;
            scaleRatioB = ratios.y;
            scaleRatioC = ratios.z;

            float padding = TextUtil.GetPadding(textDisplayData, ratios);
            float gradientScale = fontAsset.gradientScale;
            float boldAdvanceMultiplier = 1;
            float stylePadding = 0;

            int atlasWidth = fontAsset.atlas.width;
            int atlasHeight = fontAsset.atlas.height;

            float fontAscender = fontAsset.faceInfo.ascentLine;
            float fontDescender = fontAsset.faceInfo.descentLine;

            if ((fontStyle & FontStyle.Bold) != 0) {
                stylePadding = fontAsset.boldStyle / 4.0f * gradientScale * scaleRatioA;
                if (stylePadding + padding > gradientScale) {
                    padding = gradientScale - stylePadding;
                }

                boldAdvanceMultiplier = 1 + fontAsset.boldSpacing * 0.01f;
            }
            else {
                stylePadding = stylePadding = fontAsset.normalStyle / 4.0f * gradientScale;
                if (stylePadding + padding > gradientScale) {
                    padding = gradientScale - stylePadding;
                }
            }

            float shear = (fontStyle & FontStyle.Italic) != 0 ? fontAsset.italicStyle * 0.01f : 0;

            float fontScaleMultiplier = textTransform == TextTransform.SuperScript || textTransform == TextTransform.SubScript
                ? fontAsset.faceInfo.subscriptSize
                : 1;

            FontAsset currentFontAsset = fontAsset;

            float fontBaseLineOffset = currentFontAsset.faceInfo.baseline * fontScale * fontScaleMultiplier * currentFontAsset.faceInfo.scale;

            // fontBaseLineOffset = 0; //+= currentFontAsset.faceInfo.SuperscriptOffset * fontScale * fontScaleMultiplier;

            // I am not sure if using a standard line height for words is correct. TMP does not use the font info, it uses max char ascender & descender for a line
            // seems to me that this would produce weird results with multiline text
            float lineHeight = (fontAsset.faceInfo.ascentLine - fontAsset.faceInfo.descentLine) * fontScale;

            CharInfo[] characters = characterList.array;
            WordInfo[] words = wordInfoList.array;
            int wordCount = wordInfoList.size;

            for (int w = 0; w < wordCount; w++) {
                WordInfo group = words[w];
                int start = group.charStart;
                int end = group.charEnd;
                float xAdvance = 0;

                for (int c = start; c < end; c++) {
                    Vector2 topLeft;
                    Vector2 bottomRight;
                    Vector2 topLeftUV;
                    Vector2 bottomRightUV;

                    ref CharInfo charInfo = ref characters[c];

                    // todo -- pull glyph & adjustments into their own struct array
                    TextGlyph glyph = charInfo.glyph;
                    if (glyph == null) {
                        // todo -- replace missing glyph with a question mark?
                        // Debug.Log($"Missing glyph for character '{(char)charInfo.character}' (char code {charInfo.character}) in font {fontAsset.name}");
                        continue;
                    }

                    GlyphValueRecord_Legacy glyphAdjustments = charInfo.glyphAdjustment;

                    float currentElementScale = fontScale * fontScaleMultiplier * glyph.scale;

                    topLeft.x = xAdvance + (glyph.xOffset - padding - stylePadding + glyphAdjustments.xPlacement) * currentElementScale;
                    topLeft.y = fontBaseLineOffset + ((fontAscender) - (glyph.yOffset + padding)) * currentElementScale;
                    bottomRight.x = topLeft.x + (glyph.width + padding * 2 + stylePadding * 2) * currentElementScale;
                    bottomRight.y = topLeft.y + (glyph.height + padding * 2) * currentElementScale;

                    float topShear = shear * ((glyph.yOffset + padding + stylePadding) * currentElementScale);
                    float bottomShear = shear * (((glyph.yOffset - glyph.height - padding - stylePadding)) * currentElementScale);

                    topLeftUV.x = (glyph.x - padding - stylePadding) / atlasWidth;
                    topLeftUV.y = 1 - (glyph.y + padding + stylePadding + glyph.height) / atlasHeight;
                    bottomRightUV.x = (glyph.x + padding + stylePadding + glyph.width) / atlasWidth;
                    bottomRightUV.y = 1 - (glyph.y - padding - stylePadding) / atlasHeight;

                    // maybe store geometry elsewhere
                    charInfo.topShear = topShear;
                    charInfo.bottomShear = bottomShear;
                    charInfo.topLeft = topLeft;
                    charInfo.bottomRight = bottomRight;
                    charInfo.topLeftUV = topLeftUV;
                    charInfo.bottomRightUV = bottomRightUV;
                    charInfo.scale = currentElementScale;
                
                    // maybe just store x advance per character since we need a word pass on xadvance any
                    xAdvance += (glyph.xAdvance
                                 * boldAdvanceMultiplier
                                 + currentFontAsset.normalSpacingOffset
                                 + glyphAdjustments.xAdvance) * currentElementScale;
                }

                words[w].width = xAdvance;
                words[w].height = lineHeight;
            }
        }

        private void SetChildInheritedFloat(TextStyleProperty property, float value) {
            if (!inheritStyleProperties) return;
            TextSpan ptr = firstChild;
            while (ptr != null) {
                ptr.SetInheritedFloatOrEnum(property, value);
                ptr = ptr.nextSibling;
            }
        }

        private void SetChildInheritedFont(FontAsset value) {
            if (!inheritStyleProperties) return;
            TextSpan ptr = firstChild;
            while (ptr != null) {
                ptr.SetInheritedFont(value);
                ptr = ptr.nextSibling;
            }
        }

        private void SetChildInheritedColor(TextStyleProperty property, Color32 value) {
            if (!inheritStyleProperties) return;
            TextSpan ptr = firstChild;
            while (ptr != null) {
                ptr.SetInheritedColor(property, value);
                ptr = ptr.nextSibling;
            }
        }

        private void SetInheritedFont(FontAsset value) {
            FontAsset currentValue = fontAsset;
            inheritedStyle.fontAsset = value;
            if (value != currentValue) {
                if (!textStyle.fontSize.HasValue) {
                    SetChildInheritedFont(value);
                    rebuildFlags |= RebuildFlag.All;
                    textInfo.RebuildSpans();
                }
            }
        }

        private static void FindGlyphs(FontAsset fontAsset, CharInfo[] charInfos, int count) {
            IntMap<TextGlyph> fontAssetCharacterDictionary = fontAsset.characterDictionary;
            // todo make a better struct based dictionary or make text glyph a class
            for (int i = 0; i < count; i++) {
                charInfos[i].glyph = fontAssetCharacterDictionary.GetOrDefault(charInfos[i].character);
            }
        }

        private static void FindKerningInfo(FontAsset fontAsset, CharInfo[] charInfos, int count) {
            IntMap<TextKerningPair> kerningDictionary = fontAsset.kerningDictionary;
            if (count < 2) {
                return;
            }

            GlyphValueRecord_Legacy glyphAdjustments = default;
            int idx = 0;

            glyphAdjustments = kerningDictionary.GetOrDefault(((charInfos[1].character) << 16) + charInfos[0].character).firstGlyphAdjustments;
            charInfos[idx++].glyphAdjustment = glyphAdjustments;

            for (int i = 1; i < count - 1; i++) {
                int current = charInfos[i].character;
                int next = charInfos[i + 1].character;
                int prev = charInfos[i - 1].character;

                glyphAdjustments = kerningDictionary.GetOrDefault((next << 16) + current).firstGlyphAdjustments;
                glyphAdjustments += kerningDictionary.GetOrDefault((current << 16) + prev).secondGlyphAdjustments;

                charInfos[idx++].glyphAdjustment = glyphAdjustments;
            }

            glyphAdjustments = kerningDictionary.GetOrDefault(((charInfos[count - 1].character) << 16) + charInfos[count - 2].character).firstGlyphAdjustments;
            charInfos[idx++].glyphAdjustment = glyphAdjustments;
        }

        public SVGXTextStyle GetStyle() {
            SVGXTextStyle retn;
            retn.fontAsset = fontAsset;
            retn.fontStyle = fontStyle;
            retn.fontSize = fontSize;
            retn.glowOffset = glowOffset;
            retn.glowOuter = glowOuter;
            retn.outlineWidth = outlineWidth;
            retn.outlineSoftness = outlineSoftness;
            retn.faceDilate = faceDilate;
            retn.underlayDilate = underlayDilate;
            retn.underlaySoftness = underlaySoftness;
            retn.underlayX = underlayX;
            retn.underlayY = underlayY;
            retn.underlayColor = underlayColor;
            retn.outlineColor = outlineColor;
            retn.textColor = textColor;
            retn.glowColor = glowColor;
            retn.textTransform = textTransform;
            retn.whitespaceMode = whitespaceMode;
            retn.alignment = alignment;
            return retn;
        }

        public void GetStyle(ref SVGXTextStyle retn) {
            retn.fontAsset = fontAsset;
            retn.fontStyle = fontStyle;
            retn.fontSize = fontSize;
            retn.glowOffset = glowOffset;
            retn.glowOuter = glowOuter;
            retn.outlineWidth = outlineWidth;
            retn.outlineSoftness = outlineSoftness;
            retn.faceDilate = faceDilate;
            retn.underlayDilate = underlayDilate;
            retn.underlaySoftness = underlaySoftness;
            retn.underlayX = underlayX;
            retn.underlayY = underlayY;
            retn.underlayColor = underlayColor;
            retn.outlineColor = outlineColor;
            retn.textColor = textColor;
            retn.glowColor = glowColor;
            retn.textTransform = textTransform;
            retn.whitespaceMode = whitespaceMode;
            retn.alignment = alignment;
        }


        private static bool Color32Equal(Color32 a, Color32 b) {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }

        private bool HasValue(TextStyleProperty property) {
            switch (property) {
                case TextStyleProperty.FaceDilate:
                    return textStyle.faceDilate.HasValue;

                case TextStyleProperty.FontSize:
                    return textStyle.fontSize.HasValue;

                case TextStyleProperty.GlowOffset:
                    return textStyle.glowOffset.HasValue;

                case TextStyleProperty.GlowOuter:
                    return textStyle.glowOuter.HasValue;

                case TextStyleProperty.UnderlayX:
                    return textStyle.underlayX.HasValue;

                case TextStyleProperty.UnderlayY:
                    return textStyle.underlayY.HasValue;

                case TextStyleProperty.UnderlayDilate:
                    return textStyle.underlayDilate.HasValue;

                case TextStyleProperty.UnderlaySoftness:
                    return textStyle.underlaySoftness.HasValue;

                case TextStyleProperty.OutlineWidth:
                    return textStyle.outlineWidth.HasValue;

                case TextStyleProperty.OutlineSoftness:
                    return textStyle.outlineSoftness.HasValue;

                case TextStyleProperty.WhitespaceMode:
                    return textStyle.whitespaceMode.HasValue;

                case TextStyleProperty.Alignment:
                    return textStyle.alignment.HasValue;

                case TextStyleProperty.TextTransform:
                    return textStyle.textTransform.HasValue;

                case TextStyleProperty.TextColor:
                    return textStyle.textColor.HasValue;

                case TextStyleProperty.GlowColor:
                    return textStyle.glowColor.HasValue;

                case TextStyleProperty.OutlineColor:
                    return textStyle.outlineColor.HasValue;

                case TextStyleProperty.UnderlayColor:
                    return textStyle.underlayColor.HasValue;

                case TextStyleProperty.FontStyle:
                    return textStyle.fontStyle.HasValue;

                case TextStyleProperty.FontAsset:
                    return textStyle.fontAsset != null;
            }

            return false;
        }

        private float GetFloat(TextStyleProperty property) {
            switch (property) {
                case TextStyleProperty.FaceDilate:
                    return faceDilate;

                case TextStyleProperty.FontSize:
                    return fontSize;

                case TextStyleProperty.GlowOffset:
                    return glowOffset;

                case TextStyleProperty.GlowOuter:
                    return glowOuter;

                case TextStyleProperty.UnderlayX:
                    return underlayX;

                case TextStyleProperty.UnderlayY:
                    return underlayY;

                case TextStyleProperty.UnderlayDilate:
                    return underlayDilate;

                case TextStyleProperty.UnderlaySoftness:
                    return underlaySoftness;

                case TextStyleProperty.OutlineWidth:
                    return outlineWidth;

                case TextStyleProperty.OutlineSoftness:
                    return outlineSoftness;

                case TextStyleProperty.WhitespaceMode:
                    return (int) whitespaceMode;

                case TextStyleProperty.Alignment:
                    return (int) alignment;

                case TextStyleProperty.TextTransform:
                    return (int) textTransform;

                case TextStyleProperty.FontStyle:
                    return (int) fontStyle;
            }

            return 0;
        }

        private void SetFloat(TextStyleProperty property, float? value) {
            switch (property) {
                case TextStyleProperty.FaceDilate:
                    textStyle.faceDilate = value;
                    break;

                case TextStyleProperty.FontSize:
                    textStyle.fontSize = value;
                    break;
                case TextStyleProperty.GlowOffset:
                    textStyle.glowOffset = value;
                    break;

                case TextStyleProperty.GlowOuter:
                    textStyle.glowOuter = value;
                    break;

                case TextStyleProperty.UnderlayX:
                    textStyle.underlayX = value;
                    break;

                case TextStyleProperty.UnderlayY:
                    textStyle.underlayY = value;
                    break;

                case TextStyleProperty.UnderlayDilate:
                    textStyle.underlayDilate = value;
                    break;

                case TextStyleProperty.UnderlaySoftness:
                    textStyle.underlaySoftness = value;
                    break;

                case TextStyleProperty.OutlineWidth:
                    textStyle.outlineWidth = value;
                    break;

                case TextStyleProperty.OutlineSoftness:
                    textStyle.outlineSoftness = value;
                    break;

                case TextStyleProperty.FontStyle:
                    if (value != null) {
                        textStyle.fontStyle = (FontStyle) value;
                    }
                    else {
                        textStyle.fontStyle = null;
                    }

                    break;

                case TextStyleProperty.WhitespaceMode:
                    if (value != null) {
                        textStyle.whitespaceMode = (WhitespaceMode) value;
                    }
                    else {
                        textStyle.whitespaceMode = null;
                    }

                    break;

                case TextStyleProperty.Alignment:
                    if (value != null) {
                        textStyle.alignment = (TextAlignment) value;
                    }
                    else {
                        textStyle.alignment = null;
                    }

                    break;

                case TextStyleProperty.TextTransform:
                    if (value != null) {
                        textStyle.textTransform = (TextTransform) value;
                    }
                    else {
                        textStyle.textTransform = null;
                    }

                    break;
            }
        }

        private void SetInheritedColor(TextStyleProperty property, Color32 color) {
            switch (property) {
                case TextStyleProperty.GlowColor:
                    inheritedStyle.glowColor = color;
                    break;

                case TextStyleProperty.TextColor:
                    inheritedStyle.textColor = color;
                    break;

                case TextStyleProperty.OutlineColor:
                    inheritedStyle.outlineColor = color;
                    break;

                case TextStyleProperty.UnderlayColor:
                    inheritedStyle.underlayColor = color;
                    break;
            }
        }

        private void SetInheritedFloatOrEnum(TextStyleProperty property, float value) {
            float before = GetFloat(property);
            switch (property) {
                case TextStyleProperty.FaceDilate:
                    inheritedStyle.faceDilate = value;
                    break;

                case TextStyleProperty.FontSize:
                    inheritedStyle.fontSize = value;
                    break;
                case TextStyleProperty.GlowOffset:
                    inheritedStyle.glowOffset = value;
                    break;

                case TextStyleProperty.GlowOuter:
                    inheritedStyle.glowOuter = value;
                    break;

                case TextStyleProperty.UnderlayX:
                    inheritedStyle.underlayX = value;
                    break;

                case TextStyleProperty.UnderlayY:
                    inheritedStyle.underlayY = value;
                    break;

                case TextStyleProperty.UnderlayDilate:
                    inheritedStyle.underlayDilate = value;
                    break;

                case TextStyleProperty.UnderlaySoftness:
                    inheritedStyle.underlaySoftness = value;
                    break;

                case TextStyleProperty.OutlineWidth:
                    inheritedStyle.outlineWidth = value;
                    break;

                case TextStyleProperty.OutlineSoftness:
                    inheritedStyle.outlineSoftness = value;
                    break;

                case TextStyleProperty.WhitespaceMode:
                    inheritedStyle.whitespaceMode = (WhitespaceMode) value;
                    break;

                case TextStyleProperty.Alignment:
                    inheritedStyle.alignment = (TextAlignment) value;
                    break;

                case TextStyleProperty.TextTransform:
                    inheritedStyle.textTransform = (TextTransform) value;
                    break;

                case TextStyleProperty.FontStyle:
                    inheritedStyle.fontStyle = (FontStyle) value;
                    break;
            }

            float after = GetFloat(property);
            if (before != after) {
                SetRebuildFlags(property);
            }
        }

        private void SetRebuildFlags(TextStyleProperty property) {
            bool shouldRebuild = false;
            switch (property) {
                case TextStyleProperty.FontAsset:
                    rebuildFlags |= RebuildFlag.Glyphs | RebuildFlag.Positions | RebuildFlag.UVCoords;
                    shouldRebuild = true;
                    break;

                case TextStyleProperty.FontSize:
                    rebuildFlags |= RebuildFlag.Positions;
                    shouldRebuild = true;
                    break;

                case TextStyleProperty.TextTransform:
                    rebuildFlags |= RebuildFlag.Casing;
                    shouldRebuild = true;
                    break;

                case TextStyleProperty.TextColor:
                case TextStyleProperty.OutlineColor:
                case TextStyleProperty.UnderlayColor:
                case TextStyleProperty.GlowColor:
                    break;

                case TextStyleProperty.FaceDilate:
                case TextStyleProperty.GlowOffset:
                case TextStyleProperty.GlowOuter:
                case TextStyleProperty.UnderlayX:
                case TextStyleProperty.UnderlayY:
                case TextStyleProperty.UnderlayDilate:
                case TextStyleProperty.UnderlaySoftness:
                case TextStyleProperty.OutlineWidth:
                case TextStyleProperty.OutlineSoftness:
                    rebuildFlags |= RebuildFlag.Positions | RebuildFlag.UVCoords;
                    shouldRebuild = true;
                    break;

                case TextStyleProperty.WhitespaceMode:
                    rebuildFlags |= RebuildFlag.Whitespace;
                    shouldRebuild = true;
                    break;

                case TextStyleProperty.Alignment:
                    rebuildFlags |= RebuildFlag.Positions;
                    shouldRebuild = true;
                    break;

                case TextStyleProperty.FontStyle:
                    rebuildFlags |= RebuildFlag.Positions | RebuildFlag.UVCoords;
                    shouldRebuild = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(property), property, null);
            }

            if (shouldRebuild) {
                textInfo.RebuildSpans();
            }
        }

        private void SetColor(TextStyleProperty property, Color32? color) {
            switch (property) {
                case TextStyleProperty.GlowColor:
                    textStyle.glowColor = color;
                    break;

                case TextStyleProperty.TextColor:
                    textStyle.textColor = color;
                    break;

                case TextStyleProperty.OutlineColor:
                    textStyle.outlineColor = color;
                    break;

                case TextStyleProperty.UnderlayColor:
                    textStyle.underlayColor = color;
                    break;
            }
        }

        private Color32 GetColor(TextStyleProperty property) {
            switch (property) {
                case TextStyleProperty.TextColor:
                    return textColor;

                case TextStyleProperty.OutlineColor:
                    return outlineColor;

                case TextStyleProperty.UnderlayColor:
                    return underlayColor;

                case TextStyleProperty.GlowColor:
                    return glowColor;
            }

            return default;
        }

        public void SetFontSize(float? value) {
            SetFloatValue(TextStyleProperty.FontSize, value);
        }

        public void SetOutlineWidth(float? value) {
            SetFloatValue(TextStyleProperty.OutlineWidth, value);
        }

        public void SetOutlineSoftness(float? value) {
            SetFloatValue(TextStyleProperty.OutlineSoftness, value);
        }

        public void SetGlowOuter(float? value) {
            SetFloatValue(TextStyleProperty.GlowOuter, value);
        }

        public void SetGlowOffset(float? value) {
            SetFloatValue(TextStyleProperty.GlowOffset, value);
        }

        public void SetFaceDilate(float? value) {
            SetFloatValue(TextStyleProperty.FaceDilate, value);
        }

        public void SetUnderlayDilate(float? value) {
            SetFloatValue(TextStyleProperty.UnderlayDilate, value);
        }

        public void SetUnderlayX(float? value) {
            SetFloatValue(TextStyleProperty.UnderlayX, value);
        }

        public void SetUnderlayY(float? value) {
            SetFloatValue(TextStyleProperty.UnderlayY, value);
        }

        public void SetUnderlaySoftness(float? value) {
            SetFloatValue(TextStyleProperty.UnderlaySoftness, value);
        }

        public void SetFontStyle(FontStyle? value) {
            if (value == null) {
                SetFloatValue(TextStyleProperty.FontStyle, null);
            }
            else {
                SetFloatValue(TextStyleProperty.FontStyle, (int) value.Value);
            }
        }

        public void SetTextAlignment(TextAlignment? value) {
            if (value == null) {
                SetFloatValue(TextStyleProperty.Alignment, null);
            }
            else {
                SetFloatValue(TextStyleProperty.Alignment, (int) value.Value);
            }
        }

        public void SetTextTransform(TextTransform? value) {
            if (value == null) {
                SetFloatValue(TextStyleProperty.TextTransform, null);
            }
            else {
                SetFloatValue(TextStyleProperty.TextTransform, (int) value.Value);
            }
        }

        public void SetWhitespaceMode(WhitespaceMode? value) {
            if (value == null) {
                SetFloatValue(TextStyleProperty.WhitespaceMode, null);
            }
            else {
                SetFloatValue(TextStyleProperty.WhitespaceMode, (int) value.Value);
            }
        }

        public void SetTextColor(Color32? value) {
            SetColorValue(TextStyleProperty.TextColor, value);
        }

        public void SetOutlineColor(Color32? value) {
            SetColorValue(TextStyleProperty.OutlineColor, value);
        }

        public void SetGlowColor(Color32? value) {
            SetColorValue(TextStyleProperty.GlowColor, value);
        }

        public void SetUnderlayColor(Color32? value) {
            SetColorValue(TextStyleProperty.UnderlayColor, value);
        }

        public void SetFont(FontAsset font) {
            FontAsset current = fontAsset;
            textStyle.fontAsset = font;
            FontAsset newValue = fontAsset;
            if (newValue != current) {
                SetRebuildFlags(TextStyleProperty.FontAsset);
                SetChildInheritedFont(newValue);
            }
        }

        private void SetColorValue(TextStyleProperty property, Color32? value) {
            Color32 current = GetColor(property);
            SetColor(property, value);
            Color32 newValue = GetColor(property);
            if (!Color32Equal(current, newValue)) {
                SetRebuildFlags(property);
                SetChildInheritedColor(property, newValue);
            }
        }

        private void SetFloatValue(TextStyleProperty property, float? value) {
            float current = GetFloat(property);
            SetFloat(property, value);
            float newValue = GetFloat(property);
            if (current != newValue) {
                SetRebuildFlags(property);
                SetChildInheritedFloat(property, newValue);
            }
        }

        [Flags]
        protected enum TextStyleProperty {

            FontAsset = 1 << 0,
            FontSize = 1 << 1,
            TextTransform = 1 << 2,
            TextColor = 1 << 3,
            OutlineColor = 1 << 4,
            UnderlayColor = 1 << 5,
            GlowColor = 1 << 6,
            FaceDilate = 1 << 7,
            GlowOffset = 1 << 8,
            GlowOuter = 1 << 9,
            UnderlayX = 1 << 10,
            UnderlayY = 1 << 11,
            UnderlayDilate = 1 << 12,
            UnderlaySoftness = 1 << 13,
            OutlineWidth = 1 << 14,
            OutlineSoftness = 1 << 15,
            WhitespaceMode = 1 << 16,
            Alignment = 1 << 17,
            FontStyle = 1 << 18,

            All = int.MaxValue

        }

        private void SetRawContentFromString(string input) {
            if (rawContent.Length < input.Length) {
                Array.Resize(ref rawContent, input.Length);
            }

            for (int i = 0; i < input.Length; i++) {
                rawContent[i] = input[i];
            }

            rawContentSize = input.Length;
        }

        public void Rebuild() {
            if (rebuildFlags == 0) return;

            int bufferSize = charInfoList.size;

            if ((rebuildFlags & RebuildFlag.Whitespace) != 0) {
                bufferSize = TextUtil.ProcessWhitespace(whitespaceMode, ref processedContent, rawContent, rawContentSize);
                charInfoList.EnsureCapacity(bufferSize);
            }

            if ((rebuildFlags & RebuildFlag.Casing) != 0) {
                TextUtil.TransformText(textTransform, processedContent, bufferSize);
            }

            CharInfo[] charInfos = charInfoList.array;

            if ((rebuildFlags & RebuildFlag.Casing) != 0 || (rebuildFlags & RebuildFlag.Whitespace) != 0) {
                charInfoList.size = bufferSize;

                for (int i = 0; i < bufferSize; i++) {
                    charInfos[i].character = processedContent[i];
                }

                wordInfoList = TextUtil.BreakIntoWords(wordInfoList, processedContent, bufferSize); // affected by content, whitespace
            }

            if ((rebuildFlags & RebuildFlag.Glyphs) != 0) {
                FindGlyphs(fontAsset, charInfos, bufferSize); // affected by text transform, content, font
                FindKerningInfo(fontAsset, charInfos, bufferSize); // affected by text transform, content, font
            }

            if ((rebuildFlags & RebuildFlag.Positions) != 0) {
                ComputeWordAndCharacterSizes(charInfoList, wordInfoList); // affected by font size, font
                textInfo.RebuildSpans();
            }

            geometryVersion++;

            rebuildFlags = 0;
        }

    }

}