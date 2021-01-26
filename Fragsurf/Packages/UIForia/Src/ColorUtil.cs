using System;
using UnityEngine;

namespace UIForia.Util {

    public static class ColorUtil {

        public static readonly Color UnsetValue = new Color(-1, -1, -1, -1);

        private struct ColorLookup {

            public readonly string name;
            public readonly Color32 color;

            public ColorLookup(string name, in Color32 color) {
                this.name = name;
                this.color = color;
            }

        }

        public static Color32 ColorFromInt(int value) {
            // todo -- endianness probably matters
            return new Color32(
                (byte) ((value >> 24) & 0xff),
                (byte) ((value >> 16) & 0xff),
                (byte) ((value >> 8) & 0xff),
                (byte) ((value >> 0) & 0xff)
            );
        }
        
         public static Color32 ColorFromRGBHash(int value) {
            // todo -- endianness probably matters
            return new Color32(
                (byte) ((value >> 16) & 0xff),
                (byte) ((value >> 8) & 0xff),
                (byte) ((value >> 0) & 0xff),
                255
            );
        }

        public static int ColorToInt(Color32 color) {
            // todo -- endianness probably matters
            return (color.r << 24) + (color.g << 16) + (color.b << 8) + (color.a << 0);
        }

        private static readonly ColorLookup[] s_ColorList = new[] {
            new ColorLookup("clear", new Color32(0, 0, 0, 0)),
            new ColorLookup("transparent", new Color32(0, 0, 0, 0)),
            new ColorLookup("black", new Color32(0, 0, 0, 255)),
            new ColorLookup("indianred", new Color32(205, 92, 92, 255)),
            new ColorLookup("lightcoral", new Color32(240, 128, 128, 255)),
            new ColorLookup("salmon", new Color32(250, 128, 114, 255)),
            new ColorLookup("darksalmon", new Color32(233, 150, 122, 255)),
            new ColorLookup("lightsalmon", new Color32(255, 160, 122, 255)),
            new ColorLookup("crimson", new Color32(220, 20, 60, 255)),
            new ColorLookup("red", new Color32(255, 0, 0, 255)),
            new ColorLookup("firebrick", new Color32(178, 34, 34, 255)),
            new ColorLookup("darkred", new Color32(139, 0, 0, 255)),
            new ColorLookup("pink", new Color32(255, 192, 203, 255)),
            new ColorLookup("lightpink", new Color32(255, 182, 193, 255)),
            new ColorLookup("hotpink", new Color32(255, 105, 180, 255)),
            new ColorLookup("deeppink", new Color32(255, 20, 147, 255)),
            new ColorLookup("mediumvioletred", new Color32(199, 21, 133, 255)),
            new ColorLookup("palevioletred", new Color32(219, 112, 147, 255)),
            new ColorLookup("coral", new Color32(255, 127, 80, 255)),
            new ColorLookup("tomato", new Color32(255, 99, 71, 255)),
            new ColorLookup("orangered", new Color32(255, 69, 0, 255)),
            new ColorLookup("darkorange", new Color32(255, 140, 0, 255)),
            new ColorLookup("orange", new Color32(255, 165, 0, 255)),
            new ColorLookup("gold", new Color32(255, 215, 0, 255)),
            new ColorLookup("yellow", new Color32(255, 255, 0, 255)),
            new ColorLookup("lightyellow", new Color32(255, 255, 224, 255)),
            new ColorLookup("lemonchiffon", new Color32(255, 250, 205, 255)),
            new ColorLookup("lightgoldenrodyellow", new Color32(250, 250, 210, 255)),
            new ColorLookup("papayawhip", new Color32(255, 239, 213, 255)),
            new ColorLookup("moccasin", new Color32(255, 228, 181, 255)),
            new ColorLookup("peachpuff", new Color32(255, 218, 185, 255)),
            new ColorLookup("palegoldenrod", new Color32(238, 232, 170, 255)),
            new ColorLookup("khaki", new Color32(240, 230, 140, 255)),
            new ColorLookup("darkkhaki", new Color32(189, 183, 107, 255)),
            new ColorLookup("lavender", new Color32(230, 230, 250, 255)),
            new ColorLookup("thistle", new Color32(216, 191, 216, 255)),
            new ColorLookup("plum", new Color32(221, 160, 221, 255)),
            new ColorLookup("violet", new Color32(238, 130, 238, 255)),
            new ColorLookup("orchid", new Color32(218, 112, 214, 255)),
            new ColorLookup("fuchsia", new Color32(255, 0, 255, 255)),
            new ColorLookup("magenta", new Color32(255, 0, 255, 255)),
            new ColorLookup("mediumorchid", new Color32(186, 85, 211, 255)),
            new ColorLookup("mediumpurple", new Color32(147, 112, 219, 255)),
            new ColorLookup("blueviolet", new Color32(138, 43, 226, 255)),
            new ColorLookup("darkviolet", new Color32(148, 0, 211, 255)),
            new ColorLookup("darkorchid", new Color32(153, 50, 204, 255)),
            new ColorLookup("darkmagenta", new Color32(139, 0, 139, 255)),
            new ColorLookup("purple", new Color32(128, 0, 128, 255)),
            new ColorLookup("rebeccapurple", new Color32(102, 51, 153, 255)),
            new ColorLookup("indigo", new Color32(75, 0, 130, 255)),
            new ColorLookup("mediumslateblue", new Color32(123, 104, 238, 255)),
            new ColorLookup("slateblue", new Color32(106, 90, 205, 255)),
            new ColorLookup("darkslateblue", new Color32(72, 61, 139, 255)),
            new ColorLookup("greenyellow", new Color32(173, 255, 47, 255)),
            new ColorLookup("chartreuse", new Color32(127, 255, 0, 255)),
            new ColorLookup("lawngreen", new Color32(124, 252, 0, 255)),
            new ColorLookup("lime", new Color32(0, 255, 0, 255)),
            new ColorLookup("limegreen", new Color32(50, 205, 50, 255)),
            new ColorLookup("palegreen", new Color32(152, 251, 152, 255)),
            new ColorLookup("lightgreen", new Color32(144, 238, 144, 255)),
            new ColorLookup("mediumspringgreen", new Color32(0, 250, 154, 255)),
            new ColorLookup("springgreen", new Color32(0, 255, 127, 255)),
            new ColorLookup("mediumseagreen", new Color32(60, 179, 113, 255)),
            new ColorLookup("seagreen", new Color32(46, 139, 87, 255)),
            new ColorLookup("forestgreen", new Color32(34, 139, 34, 255)),
            new ColorLookup("green", new Color32(0, 128, 0, 255)),
            new ColorLookup("darkgreen", new Color32(0, 100, 0, 255)),
            new ColorLookup("yellowgreen", new Color32(154, 205, 50, 255)),
            new ColorLookup("olivedrab", new Color32(107, 142, 35, 255)),
            new ColorLookup("olive", new Color32(128, 128, 0, 255)),
            new ColorLookup("darkolivegreen", new Color32(85, 107, 47, 255)),
            new ColorLookup("mediumaquamarine", new Color32(102, 205, 170, 255)),
            new ColorLookup("darkseagreen", new Color32(143, 188, 143, 255)),
            new ColorLookup("lightseagreen", new Color32(32, 178, 170, 255)),
            new ColorLookup("darkcyan", new Color32(0, 139, 139, 255)),
            new ColorLookup("teal", new Color32(0, 128, 128, 255)),
            new ColorLookup("aqua", new Color32(0, 255, 255, 255)),
            new ColorLookup("cyan", new Color32(0, 255, 255, 255)),
            new ColorLookup("lightcyan", new Color32(224, 255, 255, 255)),
            new ColorLookup("paleturquoise", new Color32(175, 238, 238, 255)),
            new ColorLookup("aquamarine", new Color32(127, 255, 212, 255)),
            new ColorLookup("turquoise", new Color32(64, 224, 208, 255)),
            new ColorLookup("mediumturquoise", new Color32(72, 209, 204, 255)),
            new ColorLookup("darkturquoise", new Color32(0, 206, 209, 255)),
            new ColorLookup("cadetblue", new Color32(95, 158, 160, 255)),
            new ColorLookup("steelblue", new Color32(70, 130, 180, 255)),
            new ColorLookup("lightsteelblue", new Color32(176, 196, 222, 255)),
            new ColorLookup("powderblue", new Color32(176, 224, 230, 255)),
            new ColorLookup("lightblue", new Color32(173, 216, 230, 255)),
            new ColorLookup("skyblue", new Color32(135, 206, 235, 255)),
            new ColorLookup("lightskyblue", new Color32(135, 206, 250, 255)),
            new ColorLookup("deepskyblue", new Color32(0, 191, 255, 255)),
            new ColorLookup("dodgerblue", new Color32(30, 144, 255, 255)),
            new ColorLookup("cornflowerblue", new Color32(100, 149, 237, 255)),
            new ColorLookup("royalblue", new Color32(65, 105, 225, 255)),
            new ColorLookup("blue", new Color32(0, 0, 255, 255)),
            new ColorLookup("mediumblue", new Color32(0, 0, 205, 255)),
            new ColorLookup("darkblue", new Color32(0, 0, 139, 255)),
            new ColorLookup("navy", new Color32(0, 0, 128, 255)),
            new ColorLookup("midnightblue", new Color32(25, 25, 112, 255)),
            new ColorLookup("cornsilk", new Color32(255, 248, 220, 255)),
            new ColorLookup("blanchedalmond", new Color32(255, 235, 205, 255)),
            new ColorLookup("bisque", new Color32(255, 228, 196, 255)),
            new ColorLookup("navajowhite", new Color32(255, 222, 173, 255)),
            new ColorLookup("wheat", new Color32(245, 222, 179, 255)),
            new ColorLookup("burlywood", new Color32(222, 184, 135, 255)),
            new ColorLookup("tan", new Color32(210, 180, 140, 255)),
            new ColorLookup("rosybrown", new Color32(188, 143, 143, 255)),
            new ColorLookup("sandybrown", new Color32(244, 164, 96, 255)),
            new ColorLookup("goldenrod", new Color32(218, 165, 32, 255)),
            new ColorLookup("darkgoldenrod", new Color32(184, 134, 11, 255)),
            new ColorLookup("peru", new Color32(205, 133, 63, 255)),
            new ColorLookup("chocolate", new Color32(210, 105, 30, 255)),
            new ColorLookup("saddlebrown", new Color32(139, 69, 19, 255)),
            new ColorLookup("sienna", new Color32(160, 82, 45, 255)),
            new ColorLookup("brown", new Color32(165, 42, 42, 255)),
            new ColorLookup("maroon", new Color32(128, 0, 0, 255)),
            new ColorLookup("white", new Color32(255, 255, 255, 255)),
            new ColorLookup("snow", new Color32(255, 250, 250, 255)),
            new ColorLookup("honeydew", new Color32(240, 255, 240, 255)),
            new ColorLookup("mintcream", new Color32(245, 255, 250, 255)),
            new ColorLookup("azure", new Color32(240, 255, 255, 255)),
            new ColorLookup("aliceblue", new Color32(240, 248, 255, 255)),
            new ColorLookup("ghostwhite", new Color32(248, 248, 255, 255)),
            new ColorLookup("whitesmoke", new Color32(245, 245, 245, 255)),
            new ColorLookup("seashell", new Color32(255, 245, 238, 255)),
            new ColorLookup("beige", new Color32(245, 245, 220, 255)),
            new ColorLookup("oldlace", new Color32(253, 245, 230, 255)),
            new ColorLookup("floralwhite", new Color32(255, 250, 240, 255)),
            new ColorLookup("ivory", new Color32(255, 255, 240, 255)),
            new ColorLookup("antiquewhite", new Color32(250, 235, 215, 255)),
            new ColorLookup("linen", new Color32(250, 240, 230, 255)),
            new ColorLookup("lavenderblush", new Color32(255, 240, 245, 255)),
            new ColorLookup("mistyrose", new Color32(255, 228, 225, 255)),
            new ColorLookup("gainsboro", new Color32(220, 220, 220, 255)),
            new ColorLookup("lightgray", new Color32(211, 211, 211, 255)),
            new ColorLookup("lightgrey", new Color32(211, 211, 211, 255)),
            new ColorLookup("silver", new Color32(192, 192, 192, 255)),
            new ColorLookup("darkgray", new Color32(169, 169, 169, 255)),
            new ColorLookup("darkgrey", new Color32(169, 169, 169, 255)),
            new ColorLookup("gray", new Color32(128, 128, 128, 255)),
            new ColorLookup("grey", new Color32(128, 128, 128, 255)),
            new ColorLookup("dimgray", new Color32(105, 105, 105, 255)),
            new ColorLookup("dimgrey", new Color32(105, 105, 105, 255)),
            new ColorLookup("lightslategray", new Color32(119, 136, 153, 255)),
            new ColorLookup("lightslategrey", new Color32(119, 136, 153, 255)),
            new ColorLookup("slategray", new Color32(112, 128, 144, 255)),
            new ColorLookup("slategrey", new Color32(112, 128, 144, 255)),
            new ColorLookup("darkslategray", new Color32(47, 79, 79, 255)),
            new ColorLookup("darkslategrey", new Color32(47, 79, 79, 255)),
        };

        private static bool isSorted;

        public static bool TryParseColorName(char[] data, int start, int end, out Color32 color) {
            if (!isSorted) {
                isSorted = true;
                Array.Sort(s_ColorList, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            }

            int num1 = 0;
            int num2 = s_ColorList.Length - 1;

            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);

                ref ColorLookup lookup = ref s_ColorList[index1];
                int num3 = StringUtil.CharCompareOrdinal(lookup.name, data, start, end);

                if (num3 == 0) {
                    color = lookup.color;
                    return true;
                }

                if (num3 < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            color = default;
            return false;
        }

        public static bool TryParseColorName(CharSpan charSpan, out Color32 color, out int nameLength) {
            if (!isSorted) {
                isSorted = true;
                Array.Sort(s_ColorList, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            }

            int num1 = 0;
            int num2 = s_ColorList.Length - 1;

            unsafe {
                while (num1 <= num2) {
                    int index1 = num1 + (num2 - num1 >> 1);

                    ref ColorLookup lookup = ref s_ColorList[index1];
                    // int num3 = string.CompareOrdinal(lookup.name, target); //StringUtil.CharCompareOrdinal(lookup.name, target)));
                    int num3 = StringUtil.CharCompareOrdinal(lookup.name, charSpan.data, charSpan.rangeStart, charSpan.rangeEnd - charSpan.rangeStart);

                    if (num3 == 0) {
                        color = lookup.color;
                        nameLength = lookup.name.Length;
                        return true;
                    }

                    if (num3 < 0) {
                        num1 = index1 + 1;
                    }
                    else {
                        num2 = index1 - 1;
                    }
                }
            }

            nameLength = 0;
            color = default;
            return false;
        }

        public static bool TryParseColorName(string name, out Color32 color) {
            if (!isSorted) {
                isSorted = true;
                Array.Sort(s_ColorList, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            }

            int num1 = 0;
            int num2 = s_ColorList.Length - 1;

            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);

                ref ColorLookup lookup = ref s_ColorList[index1];
                int num3 = string.CompareOrdinal(lookup.name, name);

                if (num3 == 0) {
                    color = lookup.color;
                    return true;
                }

                if (num3 < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            color = default;
            return false;
        }

    }

}