using System;
using System.Collections.Generic;
using System.Text;
using ZFormat;

namespace UIForia.Util {

    public class CharStringBuilder {

        public int size;
        public char[] characters;
        public Stack<CharStringBuilder> builderStack;

        private static char[] s_Scratch = new char[256];

        public CharStringBuilder(int capacity = 32) {
            this.size = 0;
            this.characters = new char[Math.Max(8, capacity)];
        }

        public CharStringBuilder PushBuilder() {
            builderStack = builderStack ?? new Stack<CharStringBuilder>();
            // todo -- pool
            CharStringBuilder builder = new CharStringBuilder();
            builderStack.Push(builder);
            return builder;
        }

        public CharStringBuilder PopBuilder() {
            return builderStack.Pop();
        }

        public void Clear() {
            size = 0;
        }

        public CharStringBuilder Append(string str) {
            // was previously  AppendCharacterRange(strMem, 0, str.Length); return this;
            // but profiling showed it was decently faster to inline this since its called very frequently

            if (str == null || str.Length == 0) return this;

            unsafe {
                fixed (char* smem = str) {
                    int strLength = str.Length;

                    if (size + strLength >= characters.Length) {
                        Array.Resize(ref characters, (size + strLength) * 2);
                    }

                    fixed (char* dmem = characters) {
                        char* d = (dmem + size);
                        char* s = (smem);
                        int length = strLength;

                        // while we can treat our data as a long, do that (4 = size of 4 characters (16 bits))
                        for (; length >= 4; length -= 4) {
                            *(long*) d = *(long*) s;
                            s += 4;
                            d += 4;
                        }

                        // while we can treat our data as ints, do that (2 = size of 2 characters (16 bits))
                        for (; length > 0; length -= 2) {
                            *(int*) d = *(int*) s;
                            s += 2;
                            d += 2;
                        }

                        // if we have an odd input length, just assign the last one
                        if (length == 1) {
                            *d = *s;
                        }
                    }

                    size += strLength;
                }
            }

            return this;
        }

        private unsafe void AppendCharacterRange(char* smem, int start, int end) {
            if (end - start <= 0) return;
            int strLength = end - start;

            if (size + strLength >= characters.Length) {
                Array.Resize(ref characters, (size + strLength) * 2);
            }

            fixed (char* dmem = characters) {
                char* d = (dmem + size);
                char* s = (smem + start);
                int length = strLength;

                // while we can treat our data as a long, do that (4 = size of 4 characters (16 bits))
                for (; length >= 4; length -= 4) {
                    *(long*) d = *(long*) s;
                    s += 4;
                    d += 4;
                }

                // while we can treat our data as ints, do that (2 = size of 2 characters (16 bits))
                for (; length > 0; length -= 2) {
                    *(int*) d = *(int*) s;
                    s += 2;
                    d += 2;
                }

                // if we have an odd input length, just assign the last one
                if (length == 1) {
                    *d = *s;
                }
            }

            size += strLength;
        }

        public CharStringBuilder Append(short val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(int value) {
            int digitCount = DigitsInInt(value);

            if (size + digitCount >= characters.Length) {
                Array.Resize(ref characters, size + digitCount);
            }

            if (value < 0) {
                value = -value;
                characters[size++] = '-';
            }

            if (value >= 100000000) {
                // ZFormat.ZNumberFormatter.Instance.NumberToChars(value);
                //  Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
                //this.NumberToChars((string) null, value, znfi);

                AppendIntegerDigits((uint) value);

                return this;
            }

            if (value >= 10000) {
                int val = value / 10000;
                FastAppendDigits(val, false);
                FastAppendDigits(value - val * 10000, true);
            }
            else {
                FastAppendDigits(value, false);
            }

            return this;
        }

        public CharStringBuilder Append(long val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(ushort val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(uint val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(ulong val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(float val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(double val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(decimal val) {
            Append(val.ToString());
            return this;
        }


        public CharStringBuilder Append(byte val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(sbyte val) {
            ZNumberFormatter.Instance.NumberToChars(val);
            Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
            return this;
        }

        public CharStringBuilder Append(bool val) {
            return Append(val ? "true" : "false");
        }

        public CharStringBuilder Append(char str) {
            if (str == '\0') return this;
            if (size + 1 >= characters.Length) {
                Array.Resize(ref characters, (size + 16));
            }

            characters[size] = str;
            size++;
            return this;
        }

        public CharStringBuilder Append(char[] str) {
            if (str == null) {
                return this;
            }

            unsafe {
                fixed (char* smem = str) {
                    AppendCharacterRange(smem, 0, str.Length);
                }
            }

            return this;
        }

        public CharStringBuilder Append(char[] str, int count) {
            return Append(str, 0, count);
        }

        public CharStringBuilder Append(string str, int start, int end) {
            if (str == null) return this;

            if (end >= str.Length) end = str.Length;

            if (start >= end || start < 0) {
                return this;
            }

            unsafe {
                fixed (char* smem = str) {
                    AppendCharacterRange(smem, start, end);
                }
            }

            return this;
        }

        public CharStringBuilder Append(char[] str, int start, int end) {
            if (str == null) return this;

            unsafe {
                fixed (char* smem = str) {
                    AppendCharacterRange(smem, start, end);
                }
            }

            return this;
        }

        public override string ToString() {
            return new string(characters, 0, size);
        }

        public unsafe bool EqualsString(string str) {
            if (str == null || size != str.Length) {
                return false;
            }

            int length = size;

            fixed (char* chPtr1 = characters) {
                fixed (char* chPtr2 = str) {
                    char* chPtr3 = chPtr1;
                    char* chPtr4 = chPtr2;
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

                    // string will have a null terminator, char buffer might not
                    if (length == 1) {
                        return *chPtr3 == *chPtr4;
                    }

                    return length <= 0;
                }
            }
        }

        public unsafe bool EqualsString(char[] str) {
            if (size != str.Length) {
                return false;
            }

            int length = size;

            fixed (char* chPtr1 = characters) {
                fixed (char* chPtr2 = str) {
                    char* chPtr3 = chPtr1;
                    char* chPtr4 = chPtr2;
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

                    return length <= 0;
                }
            }
        }

        public static unsafe bool CompareStringBuilder_String(StringBuilder builder, string str) {
            if (builder.Length != str.Length) {
                return false;
            }

            if (builder.Length > s_Scratch.Length) {
                s_Scratch = new char[builder.Length];
            }

            builder.CopyTo(0, s_Scratch, 0, builder.Length);
            int length = builder.Length;

            fixed (char* chPtr1 = s_Scratch) {
                fixed (char* chPtr2 = str) {
                    char* chPtr3 = chPtr1;
                    char* chPtr4 = chPtr2;
                    for (; length >= 12; length -= 12) {
                        if (*(long*) chPtr3 != *(long*) chPtr4 || *(long*) (chPtr3 + 4) != *(long*) (chPtr4 + 4) || *(long*) (chPtr3 + 8) != *(long*) (chPtr4 + 8))
                            return false;
                        chPtr3 += 12;
                        chPtr4 += 12;
                    }

                    for (; length > 0 && *(int*) chPtr3 == *(int*) chPtr4; length -= 2) {
                        chPtr3 += 2;
                        chPtr4 += 2;
                    }

                    return length <= 0;
                }
            }
        }

        private static int DigitsInInt(int n) {
            if (n >= 0) {
                if (n < 10) return 1;
                if (n < 100) return 2;
                if (n < 1000) return 3;
                if (n < 10000) return 4;
                if (n < 100000) return 5;
                if (n < 1000000) return 6;
                if (n < 10000000) return 7;
                if (n < 100000000) return 8;
                if (n < 1000000000) return 9;
                return 10;
            }
            else {
                if (n > -10) return 2;
                if (n > -100) return 3;
                if (n > -1000) return 4;
                if (n > -10000) return 5;
                if (n > -100000) return 6;
                if (n > -1000000) return 7;
                if (n > -10000000) return 8;
                if (n > -100000000) return 9;
                if (n > -1000000000) return 10;
                return 11;
            }
        }

        private void FastIntegerToString(int value) {
            int digitCount = DigitsInInt(value);

            if (size + digitCount >= characters.Length) {
                Array.Resize(ref characters, size + digitCount);
            }

            if (value < 0) {
                value = -value;
                characters[size++] = '-';
            }

            if (value >= 100000000) {
                // ZFormat.ZNumberFormatter.Instance.NumberToChars(value);
                //  Append(ZNumberFormatter.Instance.Chars, ZNumberFormatter.Instance.Count);
                //this.NumberToChars((string) null, value, znfi);

                AppendIntegerDigits((uint) value);

                return;
            }

            if (value >= 10000) {
                int val = value / 10000;
                FastAppendDigits(val, false);
                FastAppendDigits(value - val * 10000, true);
            }
            else {
                FastAppendDigits(value, false);
            }
        }

        private static uint FastToDecHex(int val) {
            if (val < 100) {
                return (uint) s_DecHexDigits[val];
            }

            int index = val * 5243 >> 19;
            return (uint) (s_DecHexDigits[index] << 8 | s_DecHexDigits[val - index * 100]);
        }

        private static uint ToDecHex(int val) {
            uint num = 0;
            int index = 0;
            if (val >= 10000) {
                int val1 = val / 10000;
                val -= val1 * 10000;
                if (val1 < 100) {
                    num = (uint) s_DecHexDigits[val1];
                }
                else {
                    index = val1 * 5243 >> 19;
                    num = (uint) (s_DecHexDigits[index] << 8 | s_DecHexDigits[val1 - index * 100]);
                }

                num <<= 16;
            }

            if (val < 100) {
                return num | (uint) s_DecHexDigits[val];
            }

            index = val * 5243 >> 19;
            return num | (uint) (s_DecHexDigits[index] << 8 | s_DecHexDigits[val - index * 100]);
        }

        private void AppendIntegerDigits(uint value) {
            int len = size + 10;
            int num1 = 8 - (0 & 7);

            int start = 0;
            int end = 10;

            uint val1 = 0;
            uint val2 = 0;

            if (value >= 100000000U) {
                int val = (int) (value / 100000000U);
                value -= (uint) (100000000 * val);
                // val2 = FastToDecHex(val);
                if (val < 100) {
                    val2 = (uint) s_DecHexDigits[val];
                }
                else {
                    int index = val * 5243 >> 19;
                    val2 = (uint) (s_DecHexDigits[index] << 8 | s_DecHexDigits[val - index * 100]);
                }
            }

            val1 = ToDecHex((int) value);

            while (true) {
                uint num2;
                switch (num1) {
                    case 8:
                        num2 = val1;
                        break;
                    case 16:
                        num2 = val2;
                        break;
                    case 24:
                        num2 = 0;
                        break;
                    case 32:
                        num2 = 0;
                        break;
                    default:
                        num2 = 0U;
                        break;
                }

                uint num3 = num2 >> ((start & 7) << 2);

                if (num1 > end) num1 = end;

                characters[--len] = (char) (48 | (int) num3 & 15);

                switch (num1 - start) {
                    case 1:
                        if (num1 != end) {
                            break;
                        }

                        return;
                    case 2:
                        characters[--len] = (char) (48 | (int) (num3 >> 4) & 15);
                        goto case 1;
                    case 3:
                        characters[--len] = (char) (48 | (int) (num3 >>= 4) & 15);
                        goto case 2;
                    case 4:
                        characters[--len] = (char) (48 | (int) (num3 >>= 4) & 15);
                        goto case 3;
                    case 5:
                        characters[--len] = (char) (48 | (int) (num3 >>= 4) & 15);
                        goto case 4;
                    case 6:
                        characters[--len] = (char) (48 | (int) (num3 >>= 4) & 15);
                        goto case 5;
                    case 7:
                        characters[--len] = (char) (48 | (int) (num3 >>= 4) & 15);
                        goto case 6;
                    case 8:
                        characters[--len] = (char) (48 | (int) (num3 >>= 4) & 15);
                        goto case 7;
                }

                start = num1;
                num1 += 8;
            }
        }

        private void FastAppendDigits(int val, bool force) {
            int decHexDigit1;
            if (force || val >= 100) {
                int index = val * 5243 >> 19;
                int decHexDigit2 = s_DecHexDigits[index];

                if (force || val >= 1000) {
                    characters[size++] = (char) (48 | decHexDigit2 >> 4);
                }

                characters[size++] = (char) (48 | decHexDigit2 & 15);

                decHexDigit1 = s_DecHexDigits[val - index * 100];
            }
            else {
                decHexDigit1 = s_DecHexDigits[val];
            }

            if (force || val >= 10) {
                characters[size++] = (char) (48 | decHexDigit1 >> 4);
            }

            characters[size++] = (char) (ushort) (48 | decHexDigit1 & 15);
        }

        private static readonly int[] s_DecHexDigits = new int[100] {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            16,
            17,
            18,
            19,
            20,
            21,
            22,
            23,
            24,
            25,
            32,
            33,
            34,
            35,
            36,
            37,
            38,
            39,
            40,
            41,
            48,
            49,
            50,
            51,
            52,
            53,
            54,
            55,
            56,
            57,
            64,
            65,
            66,
            67,
            68,
            69,
            70,
            71,
            72,
            73,
            80,
            81,
            82,
            83,
            84,
            85,
            86,
            87,
            88,
            89,
            96,
            97,
            98,
            99,
            100,
            101,
            102,
            103,
            104,
            105,
            112,
            113,
            114,
            115,
            116,
            117,
            118,
            119,
            120,
            121,
            128,
            129,
            130,
            131,
            132,
            133,
            134,
            135,
            136,
            137,
            144,
            145,
            146,
            147,
            148,
            149,
            150,
            151,
            152,
            153
        };

    }

}