using System;
using UnityEngine;

namespace UIForia.Util {

    public static class MathUtil {

        public static float Round(float amountToRound, float nearstOf, float fairness = 0.5f) {
            return (float) Math.Floor(amountToRound / nearstOf + fairness) * nearstOf;
        }

        public static float RemapRange(float s, float a1, float a2, float b1, float b2) {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        public static float PercentOfRange(float v, float bottom, float top) {
            float div = top - bottom;
            return div == 0 ? 0 : (v - bottom) / div;
        }

        public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4) {

            float d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

            if (d == 0.0f) {
                return false;
            }

            float u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            float v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

            return !(u < 0.0f) && !(u > 1.0f) && !(v < 0.0f) && !(v > 1.0f);
        }

        public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4, out Vector2 intersection) {
            intersection = Vector2.zero;

            float d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

            if (d == 0.0f) {
                return false;
            }

            float u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            float v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f) {
                return false;
            }

            intersection.x = p1.x + u * (p2.x - p1.x);
            intersection.y = p1.y + u * (p2.y - p1.y);

            return true;
        }

        public static float WrapAngleDeg(float angleDeg) {
            return angleDeg % 360f;
        }

        public static bool Between(float val, float min, float max) {
            return val >= min && val <= max;
        }

    }

}