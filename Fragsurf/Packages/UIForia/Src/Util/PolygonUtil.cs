using System;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Util {

    public static class PolygonUtil {

        // this returns true if the rect is rotated also! careful!
        private static bool IsRect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {
            int distP0P1 = (int) (p0 - p1).sqrMagnitude;
            int distP1P2 = (int) (p1 - p2).sqrMagnitude;
            int distP2P3 = (int) (p2 - p3).sqrMagnitude;
            int distP3P0 = (int) (p3 - p0).sqrMagnitude;
            int distP2P0 = (int) (p2 - p0).sqrMagnitude;
            int distP1P3 = (int) (p1 - p3).sqrMagnitude;

            return (distP0P1 ^ distP1P2 ^ distP2P3 ^ distP3P0 ^ distP2P0 ^ distP1P3) == 0;
        }
        
        public static bool PointInPolygon(in Vector2 point, Vector2[] polygon) {
            int indexer = polygon.Length - 1;
            bool inside = false;
            float x = point.x;
            float y = point.y;
            for (int i = 0; i < polygon.Length; indexer = i++) {
                float polyX = polygon[i].x;
                float polyY = polygon[i].y;
                inside ^= polyY > y ^ polygon[indexer].y > y &&
                          x < (polygon[indexer].x - polyX)
                          * (y - polyY)
                          / (polygon[indexer].y - polyY)
                          + polyX;
            }

            return inside;
        }

        public static bool PointInOrientedBounds(in Vector2 point, in OrientedBounds bounds) {
            return PointInTriangle(point, bounds.p0, bounds.p1, bounds.p2) || PointInTriangle(point, bounds.p2, bounds.p3, bounds.p0);
        }

        public static bool PointInTriangle(in Vector2 test, in Vector2 p0, in Vector2 p1, in Vector2 p2) {
            float s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * test.x + (p0.x - p2.x) * test.y;
            float t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * test.x + (p1.x - p0.x) * test.y;

            if ((s < 0) != (t < 0)) {
                return false;
            }

            float area = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

            return area < 0 ? (s <= 0 && s + t >= area) : (s >= 0 && s + t <= area);
        }

        public static bool PointInPolygon(in Vector2 point, Vector2[] polygon, int size) {
            int indexer = size - 1;
            bool inside = false;
            float x = point.x;
            float y = point.y;
            for (int i = 0; i < size; indexer = i++) {
                float polyX = polygon[i].x;
                float polyY = polygon[i].y;
                ref Vector2 checkPoint = ref polygon[indexer];
                inside ^= polyY > y ^ checkPoint.y > y &&
                          x < (checkPoint.x - polyX)
                          * (y - polyY)
                          / (checkPoint.y - polyY)
                          + polyX;
            }

            return inside;
        }

        public static Vector4 GetBounds(StructList<Vector2> p) {
            float minX = Single.MaxValue;
            float minY = Single.MaxValue;
            float maxX = Single.MinValue;
            float maxY = Single.MinValue;
            for (int b = 0; b < p.size; b++) {
                Vector2 point = p.array[b];
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.y > maxY) maxY = point.y;
            }

            return new Vector4(minX, minY, maxX, maxY);
        }

    }

}