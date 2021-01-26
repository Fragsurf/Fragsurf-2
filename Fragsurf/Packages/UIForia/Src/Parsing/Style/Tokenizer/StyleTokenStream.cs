using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Parsing.Style.Tokenizer {

    public struct StyleTokenStream {

        private int ptr;

        private Stack<int> stack;
        private List<StyleToken> tokens;

        public StyleTokenStream(List<StyleToken> tokens) {
            ptr = 0;
            this.tokens = tokens;
            stack = StackPool<int>.Get();
        }

        public int CurrentIndex => ptr;

        public StyleToken Current {
            [DebuggerStepThrough] get { return (ptr >= tokens.Count || tokens.Count == 0) ? StyleToken.Invalid : tokens[ptr]; }
        }

        public StyleToken Next {
            [DebuggerStepThrough] get { return (ptr + 1 >= tokens.Count) ? StyleToken.Invalid : tokens[ptr + 1]; }
        }

        public StyleToken Previous {
            get { return (ptr - 1 < 0 || tokens.Count == 0) ? StyleToken.Invalid : tokens[ptr - 1]; }
        }

        public StyleToken Last {
            [DebuggerStepThrough] get { return (tokens.Count == 0) ? StyleToken.Invalid : tokens[tokens.Count - 1]; }
        }

        public bool HasMoreTokens {
            [DebuggerStepThrough] get { return ptr < tokens.Count; }
        }

        public bool HasPrevious {
            [DebuggerStepThrough] get { return ptr - 1 >= 0; }
        }

        [DebuggerStepThrough]
        public void Advance(int count = 1) {
            ptr = Mathf.Min(ptr + count, tokens.Count);
        }

        [DebuggerStepThrough]
        public void Save() {
            stack.Push(ptr);
        }

        [DebuggerStepThrough]
        public void Restore() {
            ptr = stack.Pop();
        }

        [DebuggerStepThrough]
        public void Chop() {
            tokens.RemoveAt(tokens.Count - 1);
        }

        public override string ToString() {
            string retn = string.Empty;
            for (int i = 0; i < tokens.Count; i++) {
                retn += i + ": " + tokens[i].value + ", ";
            }

            return retn + $" (idx: {ptr}, {Current.value} -> {Current.styleTokenType})";
        }

        [DebuggerStepThrough]
        public int FindNextIndex(StyleTokenType targetStyleTokenType) {
            int i = 0;
            int counter = 0;
            while (HasTokenAt(i)) {
                StyleTokenType styleToken = Peek(i);
                if (styleToken == StyleTokenType.ParenOpen) {
                    counter++;
                }
                else if (styleToken == StyleTokenType.ParenClose) {
                    counter--;
                }
                else if (styleToken == targetStyleTokenType && counter == 0) {
                    return i;
                }

                i++;
            }

            return -1;
        }
        
        [DebuggerStepThrough]
        public int FindNextIndexAtSameLevel(StyleTokenType targetStyleTokenType) {
            int i = 0;
            int level = 0;
            while (HasTokenAt(i)) {
                StyleTokenType styleToken = Peek(i);
                if (styleToken == StyleTokenType.ParenOpen || styleToken == StyleTokenType.BracketOpen || styleToken == StyleTokenType.LessThan) {
                    level++;
                }
                else if (styleToken == StyleTokenType.ParenClose || styleToken == StyleTokenType.BracketClose || styleToken == StyleTokenType.GreaterThan) {
                    level--;
                }
                else if (styleToken == targetStyleTokenType && level == 0) {
                    return i;
                }

                i++;
            }

            return -1;
        }

        [DebuggerStepThrough]
        public int FindMatchingIndex(StyleTokenType braceOpen, StyleTokenType braceClose) {
            if (Current != braceOpen) {
                return -1;
            }

            Save();

            int i = -1;
            int counter = 0;
            while (ptr != tokens.Count) {
                i++;

                if (Current == braceOpen) {
                    counter++;
                }

                if (Current == braceClose) {
                    counter--;
                    if (counter == 0) {
                        Restore();
                        return i;
                    }
                }

                Advance();
            }

            Restore();
            return -1;
        }

        [DebuggerStepThrough]
        public StyleTokenStream AdvanceAndReturnSubStream(int advance) {
            List<StyleToken> subStreamTokens = tokens.GetRange(ptr, advance);
            Advance(advance);
            return new StyleTokenStream(subStreamTokens);
        }

        [DebuggerStepThrough]
        public StyleTokenType Peek(int i) {
            return tokens[ptr + i];
        }

        [DebuggerStepThrough]
        public bool HasTokenAt(int p0) {
            return ptr + p0 < tokens.Count;
        }

        public void Release() {
            StackPool<int>.Release(stack);
            ListPool<StyleToken>.Release(ref tokens);
            stack = null;
            tokens = null;
        }

        public void Rewind(int count = 1) {
            ptr -= count;
        }

    }

}