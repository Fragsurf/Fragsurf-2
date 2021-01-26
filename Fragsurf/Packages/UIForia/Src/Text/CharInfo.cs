using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace UIForia.Text {

    [DebuggerDisplay("Char = {(char)character}")]
    public struct CharInfo {

        // word (sic!) space
        public Vector2 topLeft;
        public Vector2 bottomRight;
        
        /// <summary>
        /// character sdf scale
        /// </summary>
        public float scale;
        
        /// <summary>
        /// point code
        /// </summary>
        public int character;
        // todo -- pull glyph & adjustment into their own data structure, not part of charinfo

        public TextGlyph glyph;
        public GlyphValueRecord_Legacy glyphAdjustment;

        // for sampling the font texture 
        public Vector2 topLeftUV;
        public Vector2 bottomRightUV;

        // italic style
        public float topShear;
        public float bottomShear;

        // local space, relative to parent element 
        public float wordLayoutX;
        public float wordLayoutY;

        public int lineIndex;
        public bool visible;

        public float LayoutX => wordLayoutX + topLeft.x;
        public float LayoutY => wordLayoutY + topLeft.y;
        
        public float MaxX => wordLayoutX + topLeft.x + (bottomRight.x - topLeft.x);
    }

}