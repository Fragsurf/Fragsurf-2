using UnityEditor;
using UnityEngine;

namespace UIForia.Editor {

    public class GUIRect {

        private Rect rect;

        public GUIRect(Rect rect = default(Rect)) {
            this.rect = rect;
        }

        public Rect GetFieldRect(int lineHeight = 1) {
            Rect retn = new Rect(rect);
            retn.height = lineHeight * EditorGUIUtility.singleLineHeight + 1;
            rect.y += (lineHeight * EditorGUIUtility.singleLineHeight + 1);
            rect.height -= (lineHeight * EditorGUIUtility.singleLineHeight + 1);
            return retn;
        }
        
        public Rect PeekFieldRect(int lineHeight = 1) {
            Rect retn = new Rect(rect);
            retn.height = lineHeight * EditorGUIUtility.singleLineHeight + 1;
            return retn;
        }

        public Rect SliceHeight(float height) {
            Rect retn = new Rect(rect);
            retn.height = height;
            rect.y += height;
            rect.height -= height;
            return retn;
        }

        public Rect GetRect() {
            return rect;
        }

        public void SetRect(Rect rect) {
            this.rect = rect;
        }

        public GUIRect[] SplitHorizontal(float percentage) {
            GUIRect[] retn = new GUIRect[2];
            retn[0] = new GUIRect(new Rect() {
                x = rect.x,
                y = rect.y,
                width = rect.width * percentage,
                height = rect.height
            });
            retn[1] = new GUIRect(new Rect() {
                x = rect.x + (rect.width * percentage),
                y = rect.y,
                height = rect.height,
                width = rect.width * percentage
            });
            return retn;
        }

        public void Indent(float width) {
            rect.x += width;
            rect.width -= width;
        }

    }

}