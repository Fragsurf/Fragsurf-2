using System;
using System.Collections.Generic;
using System.Text;

namespace UIForia.Util {

    public static class StringUtil {

        public static CharStringBuilder s_CharStringBuilder = new CharStringBuilder(128);

        public static readonly char[] s_SplitComma = {','};

        public static unsafe string InlineReplace(this string target, char oldValue, char newValue) {
            if (target == null) return null;
            fixed (char* charptr = target) {
                for (int i = 0; i < target.Length; i++) {
                    if (charptr[i] == oldValue) {
                        charptr[i] = newValue;
                    }
                }
            }

            return target;
        }

        public static int FindMatchingIndex(string input, char open, char close) {
            int start = -1;

            for (int i = 0; i < input.Length; i++) {
                if (input[i] == open) {
                    start = i;
                    break;
                }
            }

            if (start == -1) {
                return -1;
            }

            int counter = 0;
            int ptr = start;
            while (ptr < input.Length) {
                char current = input[ptr];
                ptr++;
                if (current == open) {
                    counter++;
                }

                if (current == close) {
                    counter--;
                    if (counter == 0) {
                        return ptr;
                    }
                }
            }

            return -1;
        }

        public static unsafe int CharCompareOrdinal(string strA, char[] chars) {
            return CharCompareOrdinal(strA, chars, 0, chars.Length);
        }

        public static unsafe int CharCompareOrdinal(string strA, char[] chars, int start, int length) {
            fixed (char* ptr = chars) {
                return CharCompareOrdinal(strA, ptr, start, length);
            }
        }

        public static unsafe int CharCompareOrdinal(string strA, char* chars, int start, int length) {
            if (strA == null && length == 0) return 0;

            if (EqualsRangeUnsafe(strA, chars, start, length)) {
                return 0;
            }

            if (strA == null) {
                return -1;
            }

            if (length == 0) {
                return 1;
            }

            int num1 = Math.Min(strA.Length, length);
            int num2 = -1;
            fixed (char* chPtr1 = strA) {
                char* chPtr2 = chars + start;
                char* chPtr3 = chPtr1;
                char* chPtr4 = chPtr2;
                for (; num1 >= 10; num1 -= 10) {
                    if (*(int*) chPtr3 != *(int*) chPtr4) {
                        num2 = 0;
                        break;
                    }

                    if (*(int*) (chPtr3 + 2) != *(int*) (chPtr4 + 2)) {
                        num2 = 2;
                        break;
                    }

                    if (*(int*) (chPtr3 + 4) != *(int*) (chPtr4 + 4)) {
                        num2 = 4;
                        break;
                    }

                    if (*(int*) (chPtr3 + 6) != *(int*) (chPtr4 + 6)) {
                        num2 = 6;
                        break;
                    }

                    if (*(int*) (chPtr3 + 8) != *(int*) (chPtr4 + 8)) {
                        num2 = 8;
                        break;
                    }

                    chPtr3 += 10;
                    chPtr4 += 10;
                }

                if (num2 != -1) {
                    char* chPtr5 = chPtr3 + num2;
                    char* chPtr6 = chPtr4 + num2;
                    int num3;
                    return (num3 = (int) *chPtr5 - (int) *chPtr6) != 0 ? num3 : chPtr5[1] - chPtr6[1];
                }

                for (; num1 > 0 && *(int*) chPtr3 == *(int*) chPtr4; num1 -= 2) {
                    chPtr3 += 2;
                    chPtr4 += 2;
                }

                if (num1 <= 0) {
                    return strA.Length - length;
                }

                int num4;
                return (num4 = (int) *chPtr3 - (int) *chPtr4) != 0 ? num4 : chPtr3[1] - chPtr4[1];

            }
        }

        public static string ListToString(string[] list, string separator = ", ") {
            if (list == null || list.Length == 0) {
                return string.Empty;
            }

            string retn = null;
            s_CharStringBuilder.Clear();

            for (int i = 0; i < list.Length; i++) {
                s_CharStringBuilder.Append(list[i]);
                if (i != list.Length - 1 && separator != null) {
                    s_CharStringBuilder.Append(separator);
                }
            }

            retn = s_CharStringBuilder.ToString();
            s_CharStringBuilder.Clear();
            return retn;
        }

        public static string ListToString(IReadOnlyList<string> list, string separator = ", ") {
            if (list == null || list.Count == 0) {
                return string.Empty;
            }

            string retn = null;
            s_CharStringBuilder.Clear();

            for (int i = 0; i < list.Count; i++) {
                s_CharStringBuilder.Append(list[i]);
                if (i != list.Count - 1 && separator != null) {
                    s_CharStringBuilder.Append(separator);
                }
            }

            retn = s_CharStringBuilder.ToString();
            s_CharStringBuilder.Clear();
            return retn;
        }

        // public static string ListToString(IList<string> list, string separator = ", ") {
        //     if (list == null || list.Count == 0) {
        //         return string.Empty;
        //     }
        //
        //     string retn = null;
        //     TextUtil.StringBuilder.Clear();
        //
        //     for (int i = 0; i < list.Count; i++) {
        //         TextUtil.StringBuilder.Append(list[i]);
        //         if (i != list.Count - 1 && separator != null) {
        //             TextUtil.StringBuilder.Append(separator);
        //         }
        //     }
        //
        //     retn = TextUtil.StringBuilder.ToString();
        //     TextUtil.StringBuilder.Clear();
        //     return retn;
        // }

        public static unsafe bool EqualsRangeUnsafe(string str, CharSpan span) {
            return EqualsRangeUnsafe(str, span.data, span.rangeStart, span.rangeEnd - span.rangeStart);
        }

        public static unsafe bool EqualsRangeUnsafe(char[] a, int aStart, char[] b, int bStart, int length) {
            fixed (char* aPtr = a) {
                fixed (char* bPtr = b) {
                    char* chPtr1 = aPtr + aStart;
                    char* chPtr2 = bPtr + bStart;

                    char* chPtr3 = chPtr1;
                    char* chPtr4 = chPtr2;

                    // todo -- assumes 64 bit
                    for (; length >= 12; length -= 12) {
                        if (*(long*) chPtr3 != *(long*) chPtr4 || *(long*) (chPtr3 + 4) != *(long*) (chPtr4 + 4) || *(long*) (chPtr3 + 8) != *(long*) (chPtr4 + 8)) {
                            return false;
                        }

                        chPtr3 += 12;
                        chPtr4 += 12;
                    }

                    for (; length > 0 && *(int*) chPtr3 == *(int*) chPtr4; length -= 2) {
                        chPtr3 += 2;
                        chPtr4 += 2;
                    }

                    if (length == 1) {
                        return *chPtr3 == *chPtr4;
                    }

                    return length <= 0;
                }
            }
        }

        public static unsafe bool EqualsRangeUnsafe(string str, char[] b, int bStart, int length) {
            fixed (char* charptr = b) {
                return EqualsRangeUnsafe(str, charptr, bStart, length);
            }
        }

        public static unsafe bool EqualsRangeUnsafe(string str, char* b, int bStart, int length) {

            if (str == null) {
                return length != 0;
            }

            if (str.Length != length) {
                return false;
            }

            fixed (char* strPtr = str) {
                char* bPtr = b;
                char* chPtr2 = bPtr + bStart;

                char* chPtr3 = strPtr;
                char* chPtr4 = chPtr2;

                // todo -- assumes 64 bit
                for (; length >= 12; length -= 12) {
                    if (*(long*) chPtr3 != *(long*) chPtr4 || *(long*) (chPtr3 + 4) != *(long*) (chPtr4 + 4) || *(long*) (chPtr3 + 8) != *(long*) (chPtr4 + 8)) {
                        return false;
                    }

                    chPtr3 += 12;
                    chPtr4 += 12;
                }

                for (; length > 0 && *(int*) chPtr3 == *(int*) chPtr4; length -= 2) {
                    chPtr3 += 2;
                    chPtr4 += 2;
                }

                if (length == 1) {
                    return *chPtr3 == *chPtr4;
                }

                return length <= 0;

            }
        }

        // [ThreadStatic] private static StringBuilder s_PerThreadStringBuilder;
        [ThreadStatic] private static StructList<PoolItem> s_PerThreadStringBuilderPool;

        private struct PoolItem {

            public StringBuilder builder;
            public bool active;

        }

        public static StringBuilder GetPerThreadStringBuilder() {
            s_PerThreadStringBuilderPool = s_PerThreadStringBuilderPool ?? new StructList<PoolItem>();
            if (s_PerThreadStringBuilderPool.Count == 0) {
                s_PerThreadStringBuilderPool.Add(new PoolItem() {
                    active = false,
                    builder = new StringBuilder(128)
                });
            }

            for (int i = 0; i < s_PerThreadStringBuilderPool.size; i++) {
                if (s_PerThreadStringBuilderPool.array[i].active == false) {
                    s_PerThreadStringBuilderPool.array[i].active = true;
                    return s_PerThreadStringBuilderPool.array[i].builder;
                }
            }

            PoolItem retn = new PoolItem() {
                active = true,
                builder = new StringBuilder(128)
            };

            s_PerThreadStringBuilderPool.Add(retn);
            return retn.builder;

        }

        public static void ReleasePerThreadStringBuilder(StringBuilder builder) {
            if (s_PerThreadStringBuilderPool == null) return;
            for (int i = 0; i < s_PerThreadStringBuilderPool.size; i++) {
                if (s_PerThreadStringBuilderPool.array[i].builder == builder) {
                    builder.Clear();
                    s_PerThreadStringBuilderPool.array[i].active = false;
                    return;
                }
            }
        }

    }

}