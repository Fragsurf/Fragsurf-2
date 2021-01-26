using UnityEngine;

namespace UIForia.Extensions {

    public static class RectExtensions {

        public static Rect Intersect(this Rect rect, in Rect other) {
            float xMin = rect.x > other.x ? rect.x : other.x;
            float xMax = rect.x + rect.width < other.x + other.width ? rect.x + rect.width : other.x + other.width;
            float yMin = rect.y > other.y ? rect.y : other.y;
            float yMax = rect.y + rect.height < other.y + other.height ? rect.y + rect.height : other.y + other.height;
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public static bool ContainOrOverlap(this Rect rect, Vector2 point) {
            return point.x >= rect.xMin && point.x <= rect.xMax && point.y >= rect.yMin && point.y <= rect.yMax;
        }

        public static Rect Grow(this Rect rect, float value) {
            return new Rect(
                rect.x - value,
                rect.y - value,
                rect.width + (2 * value),
                rect.height + (2 * value)
            );
        }

    }

}