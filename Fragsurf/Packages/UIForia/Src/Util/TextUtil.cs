using System.Text;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Text {
    
    public static class TextUtil {
        
        public static StringBuilder StringBuilder = new StringBuilder(1024);

        // whitespace processing needs to happen in two phases. the first is where we collapse whitespace and handle new lines
        // the second is what to do with trailing space and wrapping.
        public static int ProcessWhitespace(WhitespaceMode whitespaceMode, ref char[] buffer, char[] input, int inputSize = -1) {
            if (inputSize < 0) inputSize = input.Length;

            if (inputSize == 0) {
                return 0;
            }
            
            bool collapseSpaceAndTab = (whitespaceMode & WhitespaceMode.CollapseWhitespace) != 0;
            bool preserveNewLine = (whitespaceMode & WhitespaceMode.PreserveNewLines) != 0;
            bool trimStart = (whitespaceMode & WhitespaceMode.TrimStart) != 0;
            bool trimEnd = (whitespaceMode & WhitespaceMode.TrimEnd) != 0;

            bool collapsing = false;

            if (buffer == null) {
                buffer = ArrayPool<char>.GetMinSize(inputSize);
            }

            if (buffer.Length < inputSize) {
                ArrayPool<char>.Resize(ref buffer, inputSize);
            }

            int writeIndex = 0;
            int start = 0;
            int end = inputSize;

            if (trimStart) {
                for (int i = 0; i < end; i++) {
                    char c = input[i];

                    bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                    if (!isWhiteSpace) {
                        start = i;
                        break;
                    }
                }
            }

            if (trimEnd) {
                for (int i = end - 1; i >= start; i--) {
                    char c = input[i];

                    bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                    if (!isWhiteSpace) {
                        end = i + 1;
                        break;
                    }
                }
            }

            for (int i = start; i < end; i++) {
                char c = input[i];

                if (c == '\n' && !preserveNewLine) {
                    continue;
                }

                bool isWhiteSpace = c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085');

                if (c == '\n') {
                    if (preserveNewLine) {
                        buffer[writeIndex++] = c;
                        continue;
                    }
                }

                if (collapsing) {
                    if (!isWhiteSpace) {
                        buffer[writeIndex++] = c;
                        collapsing = false;
                    }
                }
                else if (isWhiteSpace) {
                    collapsing = collapseSpaceAndTab;
                    buffer[writeIndex++] = ' ';
                }
                else {
                    buffer[writeIndex++] = c;
                }
            }


            return writeIndex;
        }

        public static StructList<WordInfo> BreakIntoWords( char[] buffer, int bufferSize = -1) {
            return BreakIntoWords(StructList<WordInfo>.Get(), buffer, bufferSize);
        }
        
        public static StructList<WordInfo> BreakIntoWords(StructList<WordInfo> retn, char[] buffer, int bufferSize = -1) {
            if (retn == null) {
                retn = new StructList<WordInfo>();
            }
            
            if (bufferSize < 0) bufferSize = buffer.Length;
            retn.size = 0;

            if (bufferSize == 0) {
                return retn;
            }
            
            WordInfo currentWord = new WordInfo();
            WordType previousType = WordType.Normal;

            char c = buffer[0];

            if (c == '\n') {
                previousType = WordType.NewLine;
            }
            else if (c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085')) {
                previousType = WordType.Whitespace;
            }
            else if (c == 0xAD) {
                previousType = WordType.SoftHyphen;
            }
            else {
                previousType = WordType.Normal;
            }

            currentWord.type = previousType;
            currentWord.charStart = 0;
            currentWord.charEnd = 1;

            for (int i = 1; i < bufferSize; i++) {
                c = buffer[i];

                WordType type = WordType.Normal;

                if (c == '\n') {
                    type = WordType.NewLine;
                }
                else if (c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085')) {
                    type = WordType.Whitespace;
                }
                else if (c == 0xAD) {
                    type = WordType.SoftHyphen;
                }

                if (type == previousType) {
                    if (type == WordType.NewLine) {
                        retn.Add(currentWord);
                        currentWord.type = type;
                        currentWord.charStart = i;
                        currentWord.charEnd = i + 1;
                    }
                    else {
                        currentWord.charEnd++;
                    }
                }
                else {
                    retn.Add(currentWord);
                    currentWord.type = type;
                    currentWord.charStart = i;
                    currentWord.charEnd = i + 1;
                }

                previousType = type;
            }

            if (currentWord.charEnd > 0) {
                retn.Add(currentWord);
            }

            return retn;
        }

        public static void TransformText(TextTransform transform, char[] buffer, int count = -1) {
            if (count < 0) count = buffer.Length;

            switch (transform) {
                case TextTransform.UpperCase:
                case TextTransform.SmallCaps:
                    for (int i = 0; i < count; i++) {
                        buffer[i] = char.ToUpper(buffer[i]);
                    }

                    break;
                case TextTransform.LowerCase:
                    for (int i = 0; i < count; i++) {
                        buffer[i] = char.ToLower(buffer[i]);
                    }

                    break;
                case TextTransform.TitleCase:
                    for (int i = 0; i < count - 1; i++) {
                        if (char.IsLetter(buffer[i]) && char.IsWhiteSpace(buffer[i - 1])) {
                            buffer[i] = char.ToUpper(buffer[i]);
                        }
                    }

                    break;
            }
        }

        public static float GetPadding(in TextDisplayData textStyle, in Vector3 ratios) {
             float4 padding = Vector4.zero;

            float scaleRatio_A = ratios.x;
            float scaleRatio_B = ratios.y;
            float scaleRatio_C = ratios.z;
            
            float faceDilate = textStyle.faceDilate * scaleRatio_A;
            float faceSoftness = textStyle.outlineSoftness * scaleRatio_A;
            float outlineThickness = textStyle.outlineWidth * scaleRatio_A;

            float uniformPadding = outlineThickness + faceSoftness + faceDilate;

            float glowOffset = textStyle.glowOffset * scaleRatio_B;
            float glowOuter = textStyle.glowOuter * scaleRatio_B;

            uniformPadding = math.max(uniformPadding, faceDilate + glowOffset + glowOuter);

            float offsetX = textStyle.underlayX * scaleRatio_C;
            float offsetY = textStyle.underlayY * scaleRatio_C;
            float dilate = textStyle.underlayDilate * scaleRatio_C;
            float softness = textStyle.underlaySoftness * scaleRatio_C;

            // tmp does a max check here with 0, I don't think we need it though
            padding.x = faceDilate + dilate + softness - offsetX;
            padding.y = faceDilate + dilate + softness - offsetY;
            padding.z = faceDilate + dilate + softness + offsetX;
            padding.w = faceDilate + dilate + softness + offsetY;

            padding = math.max(padding, uniformPadding);
//            padding.x = math.max(padding.x, uniformPadding);
//            padding.y = math.max(padding.y, uniformPadding);
//            padding.z = math.max(padding.z, uniformPadding);
//            padding.w = math.max(padding.w, uniformPadding);

            padding = math.min(padding, 1);
            
//            padding.x = math.min(padding.x, 1);
//            padding.y = math.min(padding.y, 1);
//            padding.z = math.min(padding.z, 1);
//            padding.w = math.min(padding.w, 1);
            
            padding *= textStyle.fontAsset.gradientScale;

            // Set UniformPadding to the maximum value of any of its components.
            uniformPadding = math.max(padding.x, padding.y);
            uniformPadding = math.max(padding.z, uniformPadding);
            uniformPadding = math.max(padding.w, uniformPadding);

            return uniformPadding + 0.5f;
        }

        public static Vector3 ComputeRatios(in TextDisplayData textStyle) {
            FontAsset fontAsset = textStyle.fontAsset; 
            float gradientScale = fontAsset.gradientScale;
            float faceDilate = textStyle.faceDilate;
            float outlineThickness = textStyle.outlineWidth;
            float outlineSoftness = textStyle.outlineSoftness;
            float weight = (fontAsset.weightNormal > fontAsset.weightBold ? fontAsset.weightNormal : fontAsset.weightBold) / 4f;
            float ratioA_t = Mathf.Max(1, weight + faceDilate + outlineThickness + outlineSoftness);
            float ratioA = (gradientScale - 1f) / (gradientScale * ratioA_t);

            float glowOffset = textStyle.glowOffset;
            float glowOuter = textStyle.glowOuter;
            float ratioBRange = (weight + faceDilate) * (gradientScale - 1f);

            float ratioB_t = Mathf.Max(1, glowOffset + glowOuter);
            float ratioB = Mathf.Max(0, gradientScale - 1 - ratioBRange) / (gradientScale * ratioB_t);
            float underlayOffsetX = textStyle.underlayX;
            float underlayOffsetY = textStyle.underlayY;
            float underlayDilate = textStyle.underlayDilate;
            float underlaySoftness = textStyle.underlaySoftness;

            float ratioCRange = (weight + faceDilate) * (gradientScale - 1);
            float ratioC_t = Mathf.Max(1, Mathf.Max(Mathf.Abs(underlayOffsetX), Mathf.Abs(underlayOffsetY)) + underlayDilate + underlaySoftness);

            float ratioC = Mathf.Max(0, gradientScale - 1f - ratioCRange) / (gradientScale * ratioC_t);

            return new Vector3(ratioA, ratioB, ratioC);
        }

    }
    
    public struct TextDisplayData {

        public float faceDilate;
        public FontAsset fontAsset;
        public float outlineWidth;
        public float outlineSoftness;
        public float glowOuter;
        public float glowOffset;
        public float underlayX;
        public float underlayY;
        public float underlayDilate;
        public float underlaySoftness;

    }


}