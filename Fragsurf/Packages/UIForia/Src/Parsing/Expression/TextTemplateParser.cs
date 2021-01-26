using System;
using System.Text;
using UIForia.Util;

namespace UIForia.Parsing.Expressions {

    public static class TextTemplateProcessor {

        private static bool Escape(string input, ref int ptr, out char result) {
            // xml parser might already do this for us
            if (StringCompare(input, ref ptr, "amp;", '&', out result)) return true;
            if (StringCompare(input, ref ptr, "lt;", '<', out result)) return true;
            if (StringCompare(input, ref ptr, "amp;", '>', out result)) return true;
            if (StringCompare(input, ref ptr, "amp;", '"', out result)) return true;
            if (StringCompare(input, ref ptr, "amp;", '\'', out result)) return true;
            if (StringCompare(input, ref ptr, "obrc;", '{', out result)) return true;
            if (StringCompare(input, ref ptr, "cbrc;", '}', out result)) return true;
            return false;
        }

        public static bool TextExpressionIsConstant(StructList<TextExpression> test) {
            if (test == null || test.size == 0) return true;

            for (int i = 0; i < test.Count; i++) {
                if (test.array[i].isExpression) {
                    return false;
                }
            }

            return true;
        }
        
        private static bool StringCompare(string input, ref int ptr, string target, char match, out char result) {
            result = '\0';

            if (ptr + target.Length - 1 >= input.Length) {
                return false;
            }

            for (int i = 0; i < target.Length; i++) {
                if (target[i] != input[ptr + i]) {
                    return false;
                }
            }

            ptr += target.Length;
            result = match;
            return true;
        }


        private static readonly StringBuilder s_Builder = new StringBuilder(512);

        
        public static void ProcessTextExpressions(string input, StructList<TextExpression> outputList) {
            //input = input.Trim(); // todo -- let style handle this 
            int ptr = 0;
            int level = 0;

            StringBuilder builder = s_Builder;
            builder.Clear();

            TextExpression currentExpression = default;

            char prev = '\0';
            
            while (ptr < input.Length) {
                char current = input[ptr++];
                
                if (current == '&') {
                    // todo -- escape probably needs to go the other way round
                    if (Escape(input, ref ptr, out char result)) {
                        builder.Append(result);
                        prev = current;
                        continue;
                    }
                }

                // handle escaping this with \
                if (current == '{' && prev != '\\') {
                    if (level == 0) {
                        if (builder.Length > 0) {
                            currentExpression.text = builder.ToString();
                            outputList.Add(currentExpression);
                            currentExpression = default;
                            builder.Clear();
                        }

                        currentExpression.isExpression = true;
                        level++;
                        prev = current;
                        continue;
                    }

                    level++;
                }
                
                // handle escaping this with \
                if (current == '}' && prev != '\\') {
                    level--;
                    if (level == 0) {
                        if (builder.Length > 0) {
                            currentExpression.text = builder.ToString();
                            outputList.Add(currentExpression);
                            currentExpression.isExpression = default;
                            builder.Clear();
                        }

                        prev = current;
                        continue;
                    }
                }

                // remove last character if escaping a brace
                if ((current == '}' || current == '{') && prev == '\\') {
                    builder.Length--;
                }

                builder.Append(current);
                prev = current;
            }

            if (level != 0) {
                throw new Exception($"Error processing {input} into expressions. Too many unmatched braces");
            }

            if (builder.Length > 0) {
                currentExpression.text = builder.ToString();
                outputList.Add(currentExpression);
            }

            builder.Clear();
        }

    }

    public struct TextExpression {

        public bool isExpression;
        public string text;

    }

}