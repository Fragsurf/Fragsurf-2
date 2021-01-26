using System;
using System.Text;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Text {

    public abstract class TextLayoutPolygon {

        public abstract bool LineCast(float y, out Vector2 intersection);

        public abstract Rect GetBounds();

    }

    public class TextInfo {

        private static int s_SpanIdGenerator;

        internal StructList<LineInfo> lineInfoList;
        internal LightList<TextSpan> spanList;
        internal TextSpan rootSpan;
        public event Action onTextLayoutRequired;

        private Size metrics;
        private bool requiresSpanListRebuild;

        private IntrinsicSizes intrinsics;
        private bool requiresLayout;

        public TextInfo(string content, in SVGXTextStyle style = default, bool inheritStyleProperties = false) {
            this.rootSpan = new TextSpan();
            this.rootSpan.textInfo = this;
            this.spanList = new LightList<TextSpan>();
            this.lineInfoList = StructList<LineInfo>.Get();
            this.requiresSpanListRebuild = true;
            this.requiresLayout = true;

            rootSpan.inheritedStyle = new SVGXInheritedTextStyle() {
                alignment = TextAlignment.Left,
                textTransform = TextTransform.None,
                whitespaceMode = WhitespaceMode.CollapseWhitespace,
                textColor = new Color32(0, 0, 0, 255),
                faceDilate = 0,
                fontAsset = FontAsset.defaultFontAsset,
                fontSize = 18,
                fontStyle = FontStyle.Normal,
                glowColor = new Color32(0, 0, 0, 0),
                underlayColor = new Color32(0, 0, 0, 0),
                glowOffset = 0,
                underlayX = 0,
                underlayY = 0,
                underlayDilate = 0,
                underlaySoftness = 0,
                glowOuter = 0,
                outlineColor = new Color32(0, 0, 0, 0),
                outlineSoftness = 0,
                outlineWidth = 0
            };
            rootSpan.inheritStyleProperties = inheritStyleProperties;
            rootSpan.textStyle = style;
            rootSpan.SetText(content);
        }

        public bool EqualsRawCharacters(char[] cmp, int length) {
            int totalSize = 0;

            for (int i = 0; i < spanList.size; i++) {
                int size = spanList.array[i].rawContentSize;
                totalSize += size;
            }

            if (totalSize != length) {
                return false;
            }

            int rangeStart = 0;
            int rangeEnd = length;
            
            for (int i = 0; i < spanList.size; i++) {
                int size = spanList.array[i].rawContentSize;
                char[] rawChars = spanList.array[i].rawContent;
                if (!StringUtil.EqualsRangeUnsafe(cmp, rangeStart, rawChars, 0, size)) {
                    return false;
                }

                rangeStart += size;
            }

            return true;
        }

        public void ForceLayout() {
            requiresLayout = true;
        }

        internal void RebuildSpans() {
            requiresSpanListRebuild = true;
            onTextLayoutRequired?.Invoke();
        }

        private void RebuildSpanList() {
            if (!requiresSpanListRebuild) return;

            requiresLayout = true;
            requiresSpanListRebuild = false;
            spanList.QuickClear();

            LightStack<TextSpan> stack = LightStack<TextSpan>.Get();

            stack.Push(rootSpan);

            while (stack.size > 0) {
                TextSpan span = stack.PopUnchecked();
                spanList.Add(span);
                span.Rebuild();

                TextSpan ptr = span.firstChild;

                // todo -- might be backwards
                while (ptr != null) {
                    stack.Push(ptr);
                    ptr = ptr.nextSibling;
                }
            }

            UpdateIntrinsics();

            LightStack<TextSpan>.Release(ref stack);
        }

        // todo -- optimize for change types where possible and do partial layout for affect spans only
        public Size Layout(Vector2 offset, float width) {
            RebuildSpanList();
            if (requiresLayout) {
                requiresLayout = false;
                RunLayout(lineInfoList, width);

                if (lineInfoList.size == 0) {
                    metrics = new Size(0, 0);
                    return metrics;
                }

                LineInfo lastLine = lineInfoList[lineInfoList.size - 1];
                float maxWidth = 0;

                LineInfo[] lineInfos = lineInfoList.array;
                for (int i = 0; i < lineInfoList.size; i++) {
                    lineInfos[i].x += offset.x;
                    lineInfos[i].y += offset.y;
                    maxWidth = Mathf.Max(maxWidth, lineInfos[i].width);
                }

                float height = lastLine.y + lastLine.height;
                ApplyLineAndWordOffsets(width, rootSpan.alignment);
                metrics = new Size(maxWidth, height);
            }

            return metrics;
        }

        private void ApplyLineAndWordOffsets(float totalWidth, TextAlignment alignment, int lineStart = 0) {
            // todo -- this can be done partially in the case where we append or insert text 

            LineInfo[] lines = lineInfoList.array;
            int lineCount = lineInfoList.size;

            TextSpan[] spans = spanList.array;

            float lineOffsetY = lines[0].y;

            int charIndex = 0;

            for (int lineIndex = lineStart; lineIndex < lineCount; lineIndex++) {
                int spanStart = lines[lineIndex].spanStart;
                int spanEnd = lines[lineIndex].spanEnd;

                lines[lineIndex].globalCharacterStartIndex = charIndex;

                float lineOffsetX = lines[lineIndex].x;
                switch (alignment) {
                    case TextAlignment.Unset:
                    case TextAlignment.Left:
                        break;
                    case TextAlignment.Right:
                        lineOffsetX = totalWidth - lines[lineIndex].width;
                        break;
                    case TextAlignment.Center:
                        lineOffsetX = (totalWidth - lines[lineIndex].width) * 0.5f;
                        break;
                }

                float wordOffsetX = lineOffsetX;

                for (int s = spanStart; s < spanEnd; s++) {
                    TextSpan span = spans[s];

                    span.geometryVersion++;

                    WordInfo[] words = span.wordInfoList.array;
                    CharInfo[] chars = span.charInfoList.array;

                    int wordStart = s == spanStart ? lines[lineIndex].wordStart : 0;
                    int wordEnd = s == spanEnd - 1 ? lines[lineIndex].wordEnd : span.wordInfoList.size;

                    for (int w = wordStart; w < wordEnd; w++) {
                        ref WordInfo wordInfo = ref words[w];
                        bool visible = wordInfo.type == WordType.Normal;

                        if (wordInfo.type == WordType.Normal || wordInfo.type == WordType.Whitespace) {
                            int charStart = wordInfo.charStart;
                            int charEnd = wordInfo.charEnd;

                            for (int c = charStart; c < charEnd; c++) {
                                ref CharInfo charInfo = ref chars[c];
                                charInfo.wordLayoutX = wordOffsetX;
                                charInfo.wordLayoutY = wordInfo.yOffset + lineOffsetY;
                                charInfo.lineIndex = lineIndex;
                                charInfo.visible = visible;
                            }

                            charIndex += charEnd - charStart;
                        }

                        wordOffsetX += wordInfo.width;
                    }
                }

                lines[lineIndex].globalCharacterEndIndex = charIndex;
                lineOffsetY += lines[lineIndex].height;
            }
        }

        private void RunSizingHeightLayout(float width) {
            throw new NotImplementedException();
        }

        // todo -- introduce faster version that just outputs size and not a filled line info list
        private StructList<LineInfo> RunLayout(StructList<LineInfo> lines, float width) {
            lines.size = 0;

            LineInfo currentLine = new LineInfo();

            // cast line through shape from top bottom and center of current line
            // find right-most intersection point use that as result

            currentLine.spanStart = 0;
            currentLine.wordStart = 0;

            int spanCount = spanList.size;
            TextSpan[] spans = spanList.array;

            for (int spanIndex = 0; spanIndex < spanCount; spanIndex++) {
                TextSpan span = spans[spanIndex];
                WordInfo[] wordInfos = span.wordInfoList.array;
                bool allowWrapping = (span.textStyle.whitespaceMode & WhitespaceMode.NoWrap) == 0;

                // todo -- if text set to nowrap or pre-wrap need different layout algorithm
                // todo -- use different algorithm for text with blocking spans in it

                // float baseLineHeight = span.textStyle.fontAsset.faceInfo.LineHeight;
                // float scaledSize = span.fontSize / span.textStyle.fontAsset.faceInfo.PointSize;
                // float lh = baseLineHeight * scaledSize;

                float lineOffset = 0;
                int end = span.wordInfoList.size;
                for (int w = 0; w < end; w++) {
                    ref WordInfo wordInfo = ref wordInfos[w];

                    switch (wordInfo.type) {
                        case WordType.Whitespace:
                            if (allowWrapping && currentLine.width + wordInfo.width > width) {
                                currentLine.spanEnd = spanIndex;
                                currentLine.wordEnd = w;
                                lines.Add(currentLine);
                                lineOffset += currentLine.height;
                                currentLine = new LineInfo(spanIndex, w + 1);
                                currentLine.y = lineOffset;
                            }
                            else {
                                if (currentLine.wordCount != 0) {
                                    currentLine.wordCount++;
                                    currentLine.width += wordInfo.width;
                                }
                                else if ((span.textStyle.whitespaceMode & WhitespaceMode.TrimLineStart) != 0 && currentLine.wordStart == w) {
                                    currentLine.wordStart++;
                                }
                            }

                            break;

                        case WordType.NewLine:
                            currentLine.wordEnd = w;
                            currentLine.spanEnd = spanIndex;
                            lines.Add(currentLine);
                            lineOffset += currentLine.height;
                            currentLine = new LineInfo(spanIndex, w + 1);
                            currentLine.height = wordInfo.height; // or LineHeight?
                            currentLine.y = lineOffset;
                            break;

                        case WordType.Normal:
                            // if word is longer than the line, put on its own line
                            if (allowWrapping && wordInfo.width > width) {
                                if (currentLine.wordCount > 0) {
                                    currentLine.spanEnd = spanIndex;
                                    currentLine.wordEnd = w;
                                    lineOffset += currentLine.height;
                                    lines.Add(currentLine);
                                }

                                currentLine = new LineInfo(spanIndex, w, wordInfo.width);
                                currentLine.y = lineOffset;
                                currentLine.wordCount = 1;
                                currentLine.spanEnd = spanIndex;
                                currentLine.wordEnd = w + 1;
                                if (wordInfo.height > currentLine.height) currentLine.height = wordInfo.height;

                                lineOffset += currentLine.height;

                                lines.Add(currentLine);
                                currentLine = new LineInfo(spanIndex, w + 1);
                                currentLine.y = lineOffset;
                            }
                            // if word is too long for the current line, break to next line
                            else if (allowWrapping && wordInfo.width + currentLine.width > width + 0.5f) {
                                currentLine.spanEnd = spanIndex;
                                currentLine.wordEnd = w;
                                lines.Add(currentLine);
                                lineOffset += currentLine.height;
                                currentLine = new LineInfo(spanIndex, w, wordInfo.width);
                                currentLine.y = lineOffset;
                                currentLine.wordCount = 1;
                                if (wordInfo.height > currentLine.height) currentLine.height = wordInfo.height;
                            }
                            else {
                                currentLine.width += wordInfo.width;
                                currentLine.wordCount++;
                                if (wordInfo.height > currentLine.height) currentLine.height = wordInfo.height;
                            }

                            break;

                        case WordType.SoftHyphen:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            if (currentLine.wordCount > 0) {
                currentLine.spanEnd = spanCount - 1;
                currentLine.wordEnd = spans[spanCount - 1].wordInfoList.size;
                lines.Add(currentLine);
            }

            // todo -- something is weird with too many lines here check it out

            LineInfo[] linearray = lines.array;
            for (int i = 0; i < lines.size; i++) {
                ref LineInfo lineInfo = ref linearray[i];
                lineInfo.spanEnd++;
                // align via baseline
                if (lineInfo.spanStart != lineInfo.spanEnd) {
                    // add difference between word baseline & line baseline (which will always be >=) to the y positions of wordInfos
                    for (int j = lineInfo.spanStart; j < lineInfo.spanEnd; j++) {
                        TextSpan span = spans[j];

                        int wordStart = 0;
                        int wordEnd = span.wordInfoList.size;

                        if (j == lineInfo.spanStart) {
                            wordStart = lineInfo.wordStart;
                        }

                        if (j == lineInfo.spanEnd - 1) {
                            wordEnd = lineInfo.wordEnd;
                        }

                        for (int w = wordStart; w < wordEnd; w++) {
                            // todo -- not working properly
                            // todo -- really want this in a different place, maybe only when creating geometry
                            span.wordInfoList.array[w].yOffset += (lineInfo.height - span.wordInfoList[w].height);
                        }
                    }
                }
            }

            return lines;
        }

        public void SetStyle(in SVGXTextStyle style) {
            rootSpan.SetStyle(style);
        }

        public void SetOutlineWidth(float? outlineWidth) {
            rootSpan.SetOutlineWidth(outlineWidth);
        }

        public void SetOutlineSoftness(float? outlineSoftness) {
            rootSpan.SetOutlineSoftness(outlineSoftness);
        }

        public void SetFontSize(float? fontSize) {
            rootSpan.SetFontSize(fontSize);
        }

        public void SetTextColor(Color32? textColor) {
            rootSpan.SetTextColor(textColor);
        }

        public void SetOutlineColor(Color32? outlineColor) {
            rootSpan.SetOutlineColor(outlineColor);
        }

        public void SetGlowColor(Color32? glowColor) {
            rootSpan.SetGlowColor(glowColor);
        }

        public void SetUnderlayColor(Color32? underlayColor) {
            rootSpan.SetUnderlayColor(underlayColor);
        }

        public void SetFaceDilate(float? faceDilate) {
            rootSpan.SetFaceDilate(faceDilate);
        }

        public void SetUnderlayX(float? underlayX) {
            rootSpan.SetUnderlayX(underlayX);
        }

        public void SetUnderlayY(float? underlayY) {
            rootSpan.SetUnderlayY(underlayY);
        }

        public void SetUnderlayDilate(float? dilate) {
            rootSpan.SetUnderlayDilate(dilate);
        }

        public void SetUnderlaySoftness(float? softness) {
            rootSpan.SetUnderlaySoftness(softness);
        }

        public void SetFontStyle(FontStyle? fontStyle) {
            rootSpan.SetFontStyle(fontStyle);
        }

        public void SetAlignment(TextAlignment? alignment) {
            rootSpan.SetTextAlignment(alignment);
        }

        public void SetFont(FontAsset font) {
            rootSpan.SetFont(font);
        }

        public void SetTextTransform(TextTransform? transform) {
            rootSpan.SetTextTransform(transform);
        }

        public void SetWhitespaceMode(WhitespaceMode? whitespaceMode) {
            rootSpan.SetWhitespaceMode(whitespaceMode);
        }

        public TextSpan InsertSpan(string text, SVGXTextStyle getTextStyle) {
            throw new NotImplementedException();
        }

        private float ComputeIntrinsicMinWidth() {
            int spanCount = spanList.size;
            TextSpan[] spans = spanList.array;

            float maxWidth = 0;

            for (int spanIndex = 0; spanIndex < spanCount; spanIndex++) {
                TextSpan span = spans[spanIndex];
                WordInfo[] wordInfos = span.wordInfoList.array;

                int end = span.wordInfoList.size;

                for (int w = 0; w < end; w++) {
                    ref WordInfo wordInfo = ref wordInfos[w];

                    switch (wordInfo.type) {
                        case WordType.Whitespace:
                        case WordType.NewLine:
                            break;

                        case WordType.SoftHyphen:
                        case WordType.Normal:
                            if (wordInfo.width > maxWidth) {
                                maxWidth = wordInfo.width;
                            }

                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return maxWidth;
        }

        private void UpdateIntrinsics() {
            intrinsics.minWidth = ComputeIntrinsicMinWidth();
            StructList<LineInfo> lines = StructList<LineInfo>.Get();
            RunLayout(lines, intrinsics.minWidth);

            if (lines.size == 0) {
                intrinsics.minHeight = 0;
                intrinsics.minWidth = 0;
                intrinsics.prefHeight = 0;
                intrinsics.prefWidth = 0;
                StructList<LineInfo>.Release(ref lines);
                return;
            }

            intrinsics.minHeight = lines[lines.size - 1].y + lines[lines.size - 1].height;
            lines.size = 0;
            RunLayout(lines, float.MaxValue);

            float maxWidth = 0;
            for (int i = 0; i < lines.size; i++) {
                if (maxWidth < lines.array[i].width) {
                    maxWidth = lines.array[i].width;
                }
            }

            intrinsics.prefWidth = maxWidth;
            // the lines can be empty after layout if the text contains only of whitespace characters (including line breaks)
            if (lines.size > 0) {
                intrinsics.prefHeight = lines[lines.size - 1].y + lines[lines.size - 1].height;
            }

            StructList<LineInfo>.Release(ref lines);
        }

        public float GetIntrinsicWidth() {
            if (requiresSpanListRebuild) {
                RebuildSpanList();
            }

            return intrinsics.prefWidth;
        }

        public float GetIntrinsicHeight() {
            if (requiresSpanListRebuild) {
                RebuildSpanList();
            }

            return intrinsics.prefHeight;
        }

        public float GetIntrinsicMinWidth() {
            if (requiresSpanListRebuild) {
                RebuildSpanList();
            }

            return intrinsics.minWidth;
        }

        public float GetIntrinsicMinHeight() {
            if (requiresSpanListRebuild) {
                RebuildSpanList();
            }

            return intrinsics.minHeight;
        }

        public float ComputeHeightForWidth(float width) {
            // todo -- if has span content that is not text we need to use block width & height to resolve their sizes

            // can't use intrinsics here if we have content that is not text

            if (requiresSpanListRebuild) {
                RebuildSpanList();
            }

            if (Mathf.Approximately(width, intrinsics.minWidth)) {
                return intrinsics.minHeight;
            }

            if (Mathf.Approximately(width, intrinsics.prefWidth)) {
                return intrinsics.prefHeight;
            }

            StructList<LineInfo> lines = StructList<LineInfo>.Get();

            RunLayout(lines, width);

            if (lines.size == 0) {
                lines.Release();
                return 0;
            }

            float retn = lines[lines.size - 1].y + lines[lines.size - 1].height;

            return retn;
        }

        // todo -- if any span has content need to use block width to resolve it since it will be a layout box most likely
        public float ComputeContentWidth(float blockWidth) {
            return GetIntrinsicWidth();
        }

        public struct IntrinsicSizes {

            public float minWidth;
            public float prefWidth;
            public float minHeight;
            public float prefHeight;

        }

        private static readonly StringBuilder s_StringBuilder = new StringBuilder(128);

        public string GetSelectedString(SelectionRange selectionRange) {
            // todo -- not at all optimized, searches every character right now and adds 1 by 1
            int idx = 0;
            int min = Mathf.Min(selectionRange.cursorIndex, selectionRange.selectIndex);
            int max = Mathf.Max(selectionRange.cursorIndex, selectionRange.selectIndex);
            for (int i = 0; i < spanList.size; i++) {
                int size = spanList.array[i].charInfoList.size;
                CharInfo[] charInfos = spanList.array[i].charInfoList.array;
                for (int c = 0; c < size; c++) {
                    if (idx >= min && idx < max) {
                        s_StringBuilder.Append((char) charInfos[c].character);
                    }

                    idx++;
                }
            }

            string retn = s_StringBuilder.ToString();
            s_StringBuilder.Clear();
            return retn;
        }

        public Vector2 GetSelectionPosition(SelectionRange selectionRange) {
            return GetCursorPosition(selectionRange.selectIndex);
        }

        private bool IsFirstOnLine(int idx) {
            StructList<CharInfo> charInfoList = rootSpan.charInfoList;
            StructList<WordInfo> wordInfoList = rootSpan.wordInfoList;

            int lineIndex = charInfoList.array[idx].lineIndex;
            int wordIndex = lineInfoList.array[lineIndex].wordStart;
            int startChar = wordInfoList.array[wordIndex].charStart;
            return idx == startChar;
        }

        private bool IsLastOnLine(int idx) {
            StructList<CharInfo> charInfoList = rootSpan.charInfoList;
            StructList<WordInfo> wordInfoList = rootSpan.wordInfoList;
            int lineIndex = charInfoList.array[idx].lineIndex;
            int wordIndex = lineInfoList.array[lineIndex].LastWordIndex;
            int endChar = wordInfoList.array[wordIndex].LastCharacterIndex;
            return idx == endChar;
        }

        private int GetPreviousWordEdge(int characterIdx) {
            if (IsFirstOnLine(characterIdx)) {
                return characterIdx;
            }

            StructList<CharInfo> charInfoList = rootSpan.charInfoList;

            int lineIndex = charInfoList.array[characterIdx].lineIndex;
            LineInfo lineInfo = lineInfoList.array[lineIndex];

            for (int i = lineInfo.spanEnd - 1; i >= lineInfo.spanStart; i--) {
                StructList<WordInfo> wordList = spanList[i].wordInfoList;
                for (int j = wordList.size - 1; j >= 0; j--) {
                    if (wordList[j].charEnd >= characterIdx && wordList[j].charStart <= characterIdx) {
                        return wordList[j > 0 ? j - 1 : 0].charStart;
                    }
                }
            }

            return characterIdx;
        }

        private int GetNextWordEdge(int characterIdx) {
            if (IsLastOnLine(characterIdx)) {
                // one more for the right edge
                return characterIdx + 1;
            }

            StructList<CharInfo> charInfoList = rootSpan.charInfoList;

            int lineIndex = charInfoList.array[characterIdx].lineIndex;
            LineInfo lineInfo = lineInfoList.array[lineIndex];

            for (int i = 0; i < lineInfo.spanEnd; i++) {
                StructList<WordInfo> wordList = spanList[i].wordInfoList;
                for (int j = 0; j < wordList.size; j++) {
                    if (wordList[j].charStart <= characterIdx && wordList[j].charEnd >= characterIdx) {
                        return wordList[j == wordList.size - 1 ? j : j + 1].charEnd;
                    }
                }
            }

            return characterIdx;
        }

        public Vector2 GetCursorPosition(int cursorIndex) {
            StructList<CharInfo> charInfoList = rootSpan.charInfoList;
            int charCount = charInfoList.size;
            if (charCount == 0) {
                return new Vector2(2, 0); // 2 is temp
            }

            if (charCount == 1) {
                if (cursorIndex == charCount) {
                    return new Vector2(charInfoList.array[cursorIndex - 1].MaxX, 0);
                }

                return new Vector2(2, 0); // using the left edge of character makes caret hard to see if border is present
            }

            if (cursorIndex < 0) {
                return new Vector2(charInfoList.array[0].LayoutX, lineInfoList.array[0].y);
            }

            if (cursorIndex >= charCount) {
                if (lineInfoList.size == 0) {
                    return new Vector2(2, 0);
                }

                return new Vector2(charInfoList.array[charCount - 1].MaxX, lineInfoList.array[lineInfoList.size - 1].y);
            }

            CharInfo charInfo = charInfoList.array[cursorIndex];
            LineInfo lineInfo = lineInfoList.array[charInfo.lineIndex];

            return new Vector2(charInfo.LayoutX, lineInfo.y);

            // if (cursorIndex < charCount) {
            //     // use average of previous right and current left
            //     CharInfo prev = charInfoList.array[cursorIndex - 1];
            //     float prevX = prev.MaxX;
            //     float x = charInfo.layoutX;
            //     float half = (prevX + x) * 0.5f;
            //     return new Vector2(half, lineInfo.y);
            // }
            // else {
            //     // use average of previous right and current left
            //     CharInfo next = charInfoList.array[cursorIndex + 1];
            //     float nextX = next.MaxX;
            //     float x = charInfo.MaxX;
            //     float half = (nextX + x) * 0.5f;
            //     return new Vector2(half, lineInfo.y);
            // }
        }

        public int GetIndexAtPoint(Vector2 point) {
            if (rootSpan.charInfoList.size == 0) {
                return 0;
            }

            int lineIndex = FindNearestLine(point);
            if (lineInfoList.size == 0) {
                return 0;
            }

            LineInfo lineInfo = lineInfoList.array[lineIndex];

            int closestIndex = lineInfo.globalCharacterStartIndex;

            float closestDistance = float.MaxValue;
            int totalChars = closestIndex;

            for (int s = lineInfo.spanStart; s < lineInfo.spanEnd; s++) {
                TextSpan span = spanList.array[s];

                for (int c = 0; c < span.charInfoList.size; c++) {
                    ref CharInfo charInfo = ref span.charInfoList.array[c];
                    float x1 = charInfo.LayoutX;
                    float x2 = charInfo.MaxX;

                    if (point.x >= x1 && point.x <= x2) {
                        if (x1 + (x2 - x1) * 0.5f > point.x) {
                            return totalChars;
                        }

                        return totalChars + 1;
                    }

                    float distToX1 = Mathf.Abs(point.x - x1);
                    float distToX2 = Mathf.Abs(point.x - x2);
                    if (distToX1 < closestDistance) {
                        closestIndex = totalChars;
                        closestDistance = distToX1;
                    }

                    if (distToX2 < closestDistance) {
                        closestIndex = totalChars + 1;
                        closestDistance = distToX2;
                    }

                    // check if mouse is in character
                    // if it is,
                    totalChars++;
                }
            }

            return closestIndex;

            // find y for mouse 
            // find word for x
            // find char for x
            // return index
        }

        private int FindNearestLine(Vector2 point) {
            int lineCount = lineInfoList.size;
            LineInfo[] lineInfos = lineInfoList.array;
            if (lineCount == 0) {
                return -1;
            }

            if (point.y <= lineInfos[0].y) {
                return 0;
            }

            if (point.y >= lineInfos[lineCount - 1].y) {
                return lineCount - 1;
            }

            float closestDistance = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < lineCount; i++) {
                LineInfo line = lineInfos[i];
                float y1 = line.y;
                float y2 = y1 + line.height;

                if (point.y >= y1 && point.y <= y2) {
                    return i;
                }

                float distToY1 = Mathf.Abs(point.y - y1);
                float distToY2 = Mathf.Abs(point.y - y2);
                if (distToY1 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToY1;
                }

                if (distToY2 < closestDistance) {
                    closestIndex = i;
                    closestDistance = distToY2;
                }
            }

            return closestIndex;
        }

        public Rect GetLineRectFromCharacterIndex(int characterIdx) {
            if (characterIdx > rootSpan.charInfoList.size - 1) {
                return default;
            }

            StructList<CharInfo> charInfoList = rootSpan.charInfoList;
            int lineIndex = charInfoList.array[characterIdx].lineIndex;
            if (lineIndex < 0) {
                return default;
            }

            LineInfo lineInfo = lineInfoList[lineIndex];
            return new Rect(lineInfo.x, lineInfo.y, lineInfo.width, lineInfo.height);
        }

        public Rect GetLineRect(int lineRangeStart) {
            return lineInfoList[lineRangeStart].LineRect;
        }

        public SelectionRange SelectWordAtPoint(Vector2 point) {
            int nearestLine = FindNearestLine(point);
            if (nearestLine < 0) {
                return default;
            }

            int closestIndex = 0;
            int spanIndex = -1;
            float closestDistance = float.MaxValue;
            LineInfo line = lineInfoList.array[nearestLine];

            for (int lineSpanIdx = line.spanStart; lineSpanIdx < line.spanEnd; lineSpanIdx++) {
                WordInfo[] wordInfos = spanList[lineSpanIdx].wordInfoList.array;
                float wordX = line.x;
                for (int i = line.wordStart; i < line.wordStart + line.wordCount; i++) {
                    WordInfo word = wordInfos[i];
                    float x1 = wordX;
                    wordX += word.width;
                    float x2 = wordX;

                    if (point.x >= x1 && point.x <= x2) {
                        int wordCharEnd = word.charEnd;
                        if (IsLastOnLine(wordCharEnd)) {
                            // this is how we select up until the right edge
                            wordCharEnd++;
                        }

                        return new SelectionRange(wordCharEnd, word.charStart);
                    }

                    float distToX1 = Mathf.Abs(point.x - x1);
                    float distToX2 = Mathf.Abs(point.x - x2);
                    if (distToX1 < closestDistance) {
                        closestIndex = i;
                        spanIndex = lineSpanIdx;
                        closestDistance = distToX1;
                    }

                    if (distToX2 < closestDistance) {
                        closestIndex = i;
                        spanIndex = lineSpanIdx;
                        closestDistance = distToX2;
                    }
                }
            }

            int charEndIdx = spanList[spanIndex].wordInfoList[closestIndex].charEnd;
            int charStartIdx = spanList[spanIndex].wordInfoList[closestIndex].charStart;

            if (IsLastOnLine(charEndIdx)) {
                // this is how we select up until the right edge
                return new SelectionRange(
                    charEndIdx + 1,
                    charStartIdx
                );
            }

            return new SelectionRange(
                charEndIdx,
                charStartIdx
            );
        }

        public SelectionRange SelectLineAtPoint(Vector2 point) {
            int idx = FindNearestLine(point);
            if (idx < 0) {
                return default;
            }

            return new SelectionRange(
                lineInfoList.array[idx].globalCharacterEndIndex + 1, // might be wrong for multi line
                lineInfoList.array[idx].globalCharacterStartIndex
            );
        }

        public SelectionRange MoveCursorLeft(SelectionRange range, bool maintainSelection, bool word) {
            int selectionIndex = range.selectIndex;

            if (maintainSelection) {
                if (selectionIndex == -1) {
                    selectionIndex = range.cursorIndex;
                }
            }
            else {
                selectionIndex = -1;
            }

            int cursorIndex;
            if (maintainSelection) {
                cursorIndex = word ? GetPreviousWordEdge(range.cursorIndex) : range.cursorIndex - 1;
            }
            else {
                cursorIndex = Mathf.Min(range.cursorIndex, range.selectIndex > -1 ? range.selectIndex : range.cursorIndex);
                if (!range.HasSelection && !word) {
                    cursorIndex--;
                }
                else if (word) {
                    cursorIndex = GetPreviousWordEdge(cursorIndex);
                }
            }

            if (cursorIndex < 0) cursorIndex = 0;

            return new SelectionRange(cursorIndex, selectionIndex);
        }

        public SelectionRange MoveCursorRight(SelectionRange range, bool select, bool word) {
            int selectionIndex = range.selectIndex;

            if (select) {
                if (selectionIndex == -1) {
                    selectionIndex = range.cursorIndex;
                }
            }
            else {
                selectionIndex = -1;
            }

            int cursorIndex;
            if (select) {
                cursorIndex = word ? GetNextWordEdge(range.cursorIndex) : range.cursorIndex + 1;
            }
            else {
                cursorIndex = Mathf.Max(range.cursorIndex, range.selectIndex > -1 ? range.selectIndex : range.cursorIndex);
                if (!range.HasSelection && !word) {
                    cursorIndex++;
                }
                else if (word) {
                    cursorIndex = GetNextWordEdge(cursorIndex);
                }
            }

            int totalCount = GetRenderedCharacterCount();
            if (cursorIndex > totalCount) cursorIndex = totalCount;

            return new SelectionRange(cursorIndex, selectionIndex);
        }

        public int GetRenderedCharacterCount() {
            int count = 0;
            for (int i = 0; i < spanList.size; i++) {
                count += spanList.array[i].charInfoList.size;
            }

            return count;
        }

        public SelectionRange MoveToStartOfLine(SelectionRange selectionRange, bool select) {
            if (selectionRange.cursorIndex <= 0) {
                return new SelectionRange(selectionRange.cursorIndex, select ? selectionRange.selectIndex : -1);
            }

            for (int i = 0; i < lineInfoList.size; i++) {
                LineInfo lineInfo = lineInfoList.array[i];
                if (selectionRange.cursorIndex >= lineInfo.globalCharacterStartIndex && selectionRange.cursorIndex < lineInfo.globalCharacterEndIndex) {
                    if (select) {
                        return new SelectionRange(lineInfo.globalCharacterStartIndex, Mathf.Max(selectionRange.selectIndex, selectionRange.cursorIndex));
                    }

                    return new SelectionRange(lineInfo.globalCharacterStartIndex);
                }
            }

            // index is int.max or content size changed / we need to check for out of bounds 
            LineInfo lastLine = lineInfoList.array[lineInfoList.size - 1];
            if (select) {
                return new SelectionRange(lastLine.globalCharacterStartIndex, selectionRange.selectIndex > -1 ? Mathf.Min(selectionRange.selectIndex, lastLine.globalCharacterEndIndex) : lastLine.globalCharacterEndIndex);
            }

            return new SelectionRange(lastLine.globalCharacterStartIndex);
        }

        public SelectionRange MoveToEndOfLine(SelectionRange selectionRange, bool select) {
            if (selectionRange.cursorIndex <= 0) {
                return new SelectionRange(lineInfoList.array[0].globalCharacterEndIndex, select ? Mathf.Max(0, selectionRange.selectIndex) : -1);
            }

            for (int i = 0; i < lineInfoList.size; i++) {
                LineInfo lineInfo = lineInfoList.array[i];
                if (selectionRange.cursorIndex >= lineInfo.globalCharacterStartIndex && selectionRange.cursorIndex < lineInfo.globalCharacterEndIndex) {
                    if (select) {
                        return new SelectionRange(lineInfo.globalCharacterEndIndex, Mathf.Max(selectionRange.selectIndex, selectionRange.cursorIndex));
                    }

                    return new SelectionRange(lineInfo.globalCharacterEndIndex);
                }
            }

            if (select) {
                return new SelectionRange(lineInfoList.array[lineInfoList.size - 1].globalCharacterEndIndex, selectionRange.selectIndex);
            }

            return new SelectionRange(lineInfoList.array[lineInfoList.size - 1].globalCharacterEndIndex);
        }

    }

}