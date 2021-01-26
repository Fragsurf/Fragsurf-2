using System;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public static class SutherlandHodgman {

        [ThreadStatic] private static StructList<Edge> s_EdgeList;
        [ThreadStatic] private static StructList<Vector2> s_InputList;

        private struct Edge {

            public Vector2 from;
            public Vector2 to;

        }

        /// <summary>
        /// This clips the subject polygon against the clip polygon (gets the intersection of the two polygons)
        /// </summary>
        public static void GetIntersectedPolygon(StructList<Vector2> subjectPoly, StructList<Vector2> clipPoly, StructList<Vector2> outputList) {
            if (subjectPoly.size < 3 || clipPoly.size < 3) {
                throw new ArgumentException(string.Format("The polygons passed in must have at least 3 Vector2s: subject={0}, clip={1}", subjectPoly.size.ToString(), clipPoly.size.ToString()));
            }

            outputList.size = 0;
            outputList.AddRange(subjectPoly);

            //	Make sure it's clockwise
            if (!IsClockwise(subjectPoly, out bool invalid)) {
                outputList.Reverse();
            }

            if (invalid) {
                return;
            }

            s_EdgeList = s_EdgeList ?? new StructList<Edge>();
            s_InputList = s_InputList ?? new StructList<Vector2>(subjectPoly.size);

            IterateEdgesClockwise(clipPoly, s_EdgeList);

            //	Walk around the clip polygon clockwise
            for (int i = 0; i < s_EdgeList.size; i++) {
                ref Edge clipEdge = ref s_EdgeList.array[i];
                s_InputList.size = 0;
                s_InputList.AddRange(outputList);
                outputList.size = 0;

                //	Sometimes when the polygons don't intersect, this list goes to zero.  Jump out to avoid an index out of range exception
                if (s_InputList.size == 0) {
                    break;
                }

                Vector2 S = s_InputList.array[s_InputList.size - 1];

                for (int index = 0; index < s_InputList.size; index++) {
                    ref Vector2 E = ref s_InputList.array[index];
                    if (IsInside(clipEdge, E)) {
                        if (!IsInside(clipEdge, S)) {
                            Vector2? v = GetIntersect(S, E, clipEdge.from, clipEdge.to);
                            if (v == null) {
                                throw new ApplicationException("Line segments don't intersect"); //	may be colinear, or may be a bug
                            }
                            else {
                                outputList.Add(v.Value);
                            }
                        }

                        outputList.Add(E);
                    }
                    else if (IsInside(clipEdge, S)) {
                        Vector2? Vector2 = GetIntersect(S, E, clipEdge.from, clipEdge.to);
                        if (Vector2 == null) {
                            throw new ApplicationException("Line segments don't intersect"); //	may be colinear, or may be a bug
                        }
                        else {
                            outputList.Add(Vector2.Value);
                        }
                    }

                    S = E;
                }
            }
        }

        private static void IterateEdgesClockwise(StructList<Vector2> polygon, StructList<Edge> output) {
            output.EnsureCapacity(polygon.size);
            output.size = 0;

            if (IsClockwise(polygon, out bool _)) {
                for (int i = 0; i < polygon.size - 1; i++) {
                    ref Edge edge = ref output.array[output.size++];
                    edge.from = polygon.array[i];
                    edge.to = polygon.array[i + 1];
                }

                ref Edge last = ref output.array[output.size++];
                last.from = polygon.array[polygon.size - 1];
                last.to = polygon.array[0];
            }
            else {
                for (int i = polygon.size - 1; i > 0; i--) {
                    ref Edge edge = ref output.array[output.size++];
                    edge.from = polygon.array[i];
                    edge.to = polygon.array[i - 1];
                }

                ref Edge last = ref output.array[output.size++];
                last.from = polygon.array[0];
                last.to = polygon.array[polygon.size - 1];
            }
        }

        /// <summary>
        /// Returns the intersection of the two lines (line segments are passed in, but they are treated like infinite lines)
        /// </summary>
        /// <remarks>
        /// Got this here:
        /// http://stackoverflow.com/questions/14480124/how-do-i-detect-triangle-and-rectangle-intersection
        /// </remarks>
        private static Vector2? GetIntersect(in Vector2 line1From, in Vector2 line1To, in Vector2 line2From, in Vector2 line2To) {
            Vector2 direction1 = line1To - line1From;
            Vector2 direction2 = line2To - line2From;
            float dotPerp = (direction1.x * direction2.y) - (direction1.y * direction2.x);

            // 0 means the lines are parallel so have infinite intersection Vector2s
            if (Mathf.Abs(dotPerp) <= 0.000000001f) {
                return null;
            }

            Vector2 c = line2From - line1From;
            float t = (c.x * direction2.y - c.y * direction2.x) / dotPerp;
            return line1From + (t * direction1);
        }

        private static bool IsInside(in Edge edge, in Vector2 test) {
            float tmp1X = edge.to.x - edge.from.x;
            float tmp1Y = edge.to.y - edge.from.y;
            float tmp2X = test.x - edge.to.x;
            float tmp2Y = test.y - edge.to.y;

            //	co-linear values should be considered inside so test with <= instead of <
            return (tmp1X * tmp2Y) - (tmp1Y * tmp2X) <= 0;
        }

        private static bool IsClockwise(StructList<Vector2> polygon, out bool invalid) {
            for (int i = 2; i < polygon.size; i++) {
                bool? isLeft = IsLeftOf(polygon.array[0], polygon.array[1], polygon.array[i]);
                //	some of the points may be co-linear.  That's ok as long as the overall is a polygon
                if (isLeft != null) {
                    invalid = false;
                    return !isLeft.Value;
                }
            }

            invalid = true;
            return false;
        }

        /// <summary>
        /// Tells if the test Vector2 lies on the left side of the edge line
        /// </summary>
        private static bool? IsLeftOf(in Vector2 from, in Vector2 to, in Vector2 test) {
            float tmp1X = to.x - from.x;
            float tmp1Y = to.y - from.y;
            float tmp2X = test.x - to.x;
            float tmp2Y = test.y - to.y;

            float x = (tmp1X * tmp2Y) - (tmp1Y * tmp2X);

            if (x < 0) {
                return false;
            }
            else if (x > 0) {
                return true;
            }
            else {
                return null;
            }
        }

    }

}