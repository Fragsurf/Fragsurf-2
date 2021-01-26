using UnityEngine;

namespace UIForia.Text {

    public static class SelectionRangeUtil {

        public static string InsertText(string source, ref SelectionRange selectionRange, string characters) {
            string retn = null;

            if (string.IsNullOrEmpty(characters)) {
                return source;
            }

            if (string.IsNullOrEmpty(source)) {
                selectionRange = new SelectionRange(characters.Length);
                return characters;
            }

            if (selectionRange.HasSelection) {
                source = DeleteTextForwards(source, ref selectionRange);
            }

            if (string.IsNullOrEmpty(source)) {
                selectionRange = new SelectionRange(characters.Length);
                return characters;
            }

            int cursorIndex = source.Length > 0 ? Mathf.Clamp(selectionRange.cursorIndex, 0, source.Length) : 0;
            if (cursorIndex == 0) {
                retn = characters + source;
                selectionRange = new SelectionRange(characters.Length);
            }
            else if (cursorIndex == source.Length) {
                retn = source + characters;
                selectionRange = new SelectionRange(retn.Length);
            }
            else {
                retn = $"{source.Substring(0, selectionRange.cursorIndex)}{characters}{source.Substring(selectionRange.cursorIndex)}";
                selectionRange = new SelectionRange(selectionRange.cursorIndex + characters.Length);
            }

            return retn;
        }

        public static string DeleteTextForwards(string source, ref SelectionRange selectionRange) {
            string retn = null;

            if (source.Length == 0) {
                return string.Empty;
            }

            int cursorIndex = Mathf.Clamp(selectionRange.cursorIndex, 0, source.Length);

            if (selectionRange.HasSelection) {
                int min = Mathf.Clamp((selectionRange.cursorIndex < selectionRange.selectIndex ? selectionRange.cursorIndex : selectionRange.selectIndex), 0, source.Length - 1);
                int max = (selectionRange.cursorIndex > selectionRange.selectIndex ? selectionRange.cursorIndex : selectionRange.selectIndex);

                if (cursorIndex == source.Length) {
                    retn = source.Substring(0, min);
                    selectionRange = new SelectionRange(retn.Length);
                }
                else {
                    string part0 = source.Substring(0, min);
                    string part1 = source.Substring(Mathf.Min(max, source.Length));
                    retn = part0 + part1;
                    selectionRange = new SelectionRange(part0.Length);
                }

                return retn;
            }

            if (cursorIndex == source.Length) {
                return source;
            }

            if (cursorIndex == source.Length - 1) {
                retn = source.Remove(source.Length - 1);
                selectionRange = new SelectionRange(retn.Length);
            }
            else {
                string part0 = source.Substring(0, cursorIndex);
                string part1 = source.Substring(cursorIndex + 1);
                retn = part0 + part1;
                selectionRange = new SelectionRange(cursorIndex);
            }

            return retn;
        }

        public static string DeleteTextBackwards(string source, ref SelectionRange range) {
            if (string.IsNullOrEmpty(source)) {
                range = new SelectionRange(0);
                return string.Empty;
            }

            int cursorIndex = range.cursorIndex;

            if (range.HasSelection) {
                int min = (range.cursorIndex < range.selectIndex ? range.cursorIndex : range.selectIndex);
                int max = (range.cursorIndex > range.selectIndex ? range.cursorIndex : range.selectIndex);

                if (max - min >= source.Length) {
                    range = new SelectionRange(0);
                    return string.Empty;
                }

                if (max >= source.Length) {
                    range = new SelectionRange(min);
                    return source.Substring(0, min);    
                }

                if (min == 0) {
                    range = new SelectionRange(0);
                    return source.Substring(max);
                }
                
                string part0 = source.Substring(0, min);
                string part1 = source.Substring(max);
                range = new SelectionRange(min);
                return part0 + part1;
            }
            else {
                if (cursorIndex == 0) {
                    return source;
                }

                cursorIndex = Mathf.Max(0, cursorIndex - 1);

                if (cursorIndex == 0) {
                    range = new SelectionRange(cursorIndex);
                    return source.Substring(1);
                }

                if (cursorIndex >= source.Length) {
                    range = new SelectionRange(source.Length);
                    return source.Substring(0, source.Length);
                }

                string part0 = source.Substring(0, cursorIndex);
                string part1 = source.Substring(cursorIndex + 1);
                range = new SelectionRange(cursorIndex);
                return part0 + part1;
            }
        }

    }

}