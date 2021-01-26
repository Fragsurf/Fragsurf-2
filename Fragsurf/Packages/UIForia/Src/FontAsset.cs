using System.Collections.Generic;
using TMPro;
using UIForia.Util;
using UnityEngine;
using UnityEngine.TextCore;

namespace UIForia {

    public class FontAsset {

        public readonly int id;
        public readonly string name;
        public readonly float gradientScale;
        public readonly float scaleX;
        public readonly float scaleY;
        public readonly Texture2D atlas;
        public readonly float weightNormal;
        public readonly float weightBold;
        public readonly float boldSpacing;
        public readonly FaceInfo faceInfo;
        public readonly IntMap<TextKerningPair> kerningDictionary;
        public readonly IntMap<TextGlyph> characterDictionary;
        public readonly float boldStyle;
        public readonly float normalStyle;
        public readonly float normalSpacingOffset;
        public readonly byte italicStyle;
        private static FontAsset defaultAsset;
        public TMP_FontAsset textMeshProFont;

        public FontAsset(TMP_FontAsset tmpFontAsset) {
            tmpFontAsset.ReadFontAssetDefinition();
            this.textMeshProFont = tmpFontAsset;
            this.name = tmpFontAsset.name;
            this.id = tmpFontAsset.GetInstanceID();
            this.faceInfo = tmpFontAsset.faceInfo;
            this.atlas = tmpFontAsset.atlasTexture;
            this.boldSpacing = tmpFontAsset.boldSpacing;
            this.gradientScale = tmpFontAsset.material.GetFloat(ShaderUtilities.ID_GradientScale);
            this.kerningDictionary = ConvertKerning(tmpFontAsset);
            this.characterDictionary = ConvertCharacters(tmpFontAsset);
            this.boldStyle = tmpFontAsset.boldStyle;
            this.normalStyle = tmpFontAsset.normalStyle;
            this.normalSpacingOffset = tmpFontAsset.normalSpacingOffset;
            this.italicStyle = tmpFontAsset.italicStyle;
            this.weightNormal = tmpFontAsset.normalStyle;
            this.weightBold = tmpFontAsset.boldStyle;
        }

        private static IntMap<TextKerningPair> ConvertKerning(TMP_FontAsset fontAsset) {
            IntMap<TextKerningPair> retn = new IntMap<TextKerningPair>(0); //tmpKerning.Count);

            // todo -- need to re-implement kerning
            
            // foreach (KeyValuePair<int, KerningPair> pair in tmpKerning) {
            //     TextKerningPair tkp = new TextKerningPair();
            //     tkp.firstGlyph = pair.Value.firstGlyph;
            //     tkp.firstGlyphAdjustments = pair.Value.firstGlyphAdjustments;
            //     tkp.secondGlyph = pair.Value.secondGlyph;
            //     tkp.secondGlyphAdjustments = pair.Value.secondGlyphAdjustments;
            //     retn.Add(pair.Key, tkp);
            // }

            return retn;
        }

        private static IntMap<TextGlyph> ConvertCharacters(TMP_FontAsset asset) {
            List<Glyph> glyphList = asset.glyphTable;
            IntMap<TextGlyph> retn = new IntMap<TextGlyph>(glyphList.Count);
            
            for (int i = 0; i < asset.characterTable.Count; i++) {

                TextGlyph glyph = new TextGlyph();
                Glyph tmpGlyph =  asset.characterTable[i].glyph;
                glyph.id = (int) tmpGlyph.index;
                glyph.height = tmpGlyph.metrics.height;
                glyph.width = tmpGlyph.metrics.width;
                glyph.x = tmpGlyph.glyphRect.x;
                glyph.y = tmpGlyph.glyphRect.y;
                glyph.scale = tmpGlyph.scale;
                glyph.xAdvance = tmpGlyph.metrics.horizontalAdvance;
                glyph.xOffset = tmpGlyph.metrics.horizontalBearingX;
                glyph.yOffset = tmpGlyph.metrics.horizontalBearingY;
                retn.Add((int) asset.characterTable[i].unicode, glyph);

            }

            return retn;
        }

        public static FontAsset defaultFontAsset {
            get {
                if (defaultAsset != null) return defaultAsset;
                defaultAsset = new FontAsset(TMP_Settings.defaultFontAsset); 
                return defaultAsset;
            }
        }

        public bool HasCharacter(int charPoint) {
            return characterDictionary.ContainsKey(charPoint);
        }

    }

}