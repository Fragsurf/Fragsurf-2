using UnityEngine;

namespace UIForia.Extensions {

    public static class VectorExtensions {

        public static Vector4 IntersectAsRect(this Vector4 rect, in Vector4 other) {
            float xMin = rect.x > other.x ? rect.x : other.x;
            float xMax = rect.z < other.z ? rect.z : other.z;
            float yMin = rect.y > other.y ? rect.y : other.y;
            float yMax = rect.w < other.w ? rect.w : other.w;
            return new Vector4(xMin, yMin, xMax, yMax);
        }

        public static bool OverlapAsRect(this Vector4 self, Vector4 other) {
            return other.z >= self.x && other.x <= self.z && other.w >= self.y && other.y <= self.w;
        }

        public static Vector2 Project(this Vector2 v, Vector2 a, Vector2 b) {
            Vector2 atob = b - a;
            Vector2 atop = v - a;
            float len = atob.x * atob.x + atob.y * atob.y;
            float dot = atop.x * atob.x + atop.y * atob.y;
            float t = Mathf.Clamp01(dot / len);
            return new Vector2(a.x + atob.x * t, a.y + atob.y * t);
        }

        public static Vector2 Rotate(this Vector2 v, Vector2 pivot, float degrees) {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            v.x -= pivot.x;
            v.y -= pivot.y;

            float newX = (cos * v.x) - (sin * v.y);
            float newY = (sin * v.x) + (cos * v.y);

            v.x = pivot.x + newX;
            v.y = pivot.y + newY;

            return v;
        }

        public static Vector2 Perpendicular(this Vector2 v) {
            return new Vector2(-v.y, v.x);
        }

        public static Vector2 Invert(this Vector2 v) {
            return new Vector2(-v.x, -v.y);
        }

        public static float Angle(this Vector2 v) {
            return v.y / v.x;
        }

    }

}