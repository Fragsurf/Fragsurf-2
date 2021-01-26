using System.Runtime.InteropServices;
using UnityEngine;

namespace UIForia.Rendering {

    [StructLayout(LayoutKind.Explicit)]
    public struct StyleColor {

        [FieldOffset(0)] public int rgba;
        [FieldOffset(0)] public byte r;
        [FieldOffset(1)] public byte g;
        [FieldOffset(2)] public byte b;
        [FieldOffset(3)] public byte a;

        public StyleColor(in Color color) {
            this.rgba = 0;
            this.r = (byte) (Mathf.Clamp01(color.r) * byte.MaxValue);
            this.g = (byte) (Mathf.Clamp01(color.g) * byte.MaxValue);
            this.b = (byte) (Mathf.Clamp01(color.b) * byte.MaxValue);
            this.a = (byte) (Mathf.Clamp01(color.a) * byte.MaxValue);
        }
        
        public StyleColor(in Color32 color) {
            this.rgba = 0;
            this.r = color.r;
            this.g = color.g;
            this.b = color.b;
            this.a = color.a;
        }

        public StyleColor(int rgba) {
            this.r = 0;
            this.b = 0;
            this.g = 0;
            this.a = 0;
            this.rgba = rgba;
        }

        public StyleColor(byte r, byte g, byte b, byte a) {
            this.rgba = 0;
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static implicit operator StyleColor(Color c) {
            return new StyleColor((byte) (Mathf.Clamp01(c.r) * byte.MaxValue), (byte) (Mathf.Clamp01(c.g) * byte.MaxValue), (byte) (Mathf.Clamp01(c.b) * byte.MaxValue), (byte) (Mathf.Clamp01(c.a) * byte.MaxValue));
        }

        public static implicit operator Color(StyleColor c) {
            return new Color(c.r / (float) byte.MaxValue, c.g / (float) byte.MaxValue, c.b / (float) byte.MaxValue, c.a / (float) byte.MaxValue);
        }

        public static StyleColor Lerp(StyleColor a, StyleColor b, float t) {
            t = Mathf.Clamp01(t);
            return new StyleColor((byte) (a.r + (b.r - a.r) * t), (byte) (a.g + (b.g - a.g) * t), (byte) (a.b + (b.b - a.b) * t), (byte) (a.a + (b.a - a.a) * t));
        }

        public static StyleColor LerpUnclamped(StyleColor a, StyleColor b, float t) {
            return new StyleColor((byte) (a.r + (b.r - a.r) * t), (byte) (a.g + (b.g - a.g) * t), (byte) (a.b + (b.b - a.b) * t), (byte) (a.a + (b.a - a.a) * t));
        }

    }

}