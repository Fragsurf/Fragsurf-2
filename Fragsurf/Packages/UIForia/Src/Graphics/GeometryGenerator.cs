using System;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace Vertigo {

    public static class GeometryGenerator {

        private static readonly LightList<int> s_IntScratch0 = new LightList<int>(32);
        private static readonly LightList<int> s_IntScratch1 = new LightList<int>(32);
        private static readonly LightList<float> s_FloatScratch = new LightList<float>(32);
        private static readonly StructList<Vector2> s_ScratchVector2 = new StructList<Vector2>(32);

        public struct RenderState {

            public float strokeWidth;
            public LineCap lineCap;
            public LineJoin lineJoin;
            public int miterLimit;

        }

        public static GeometryRange FillRegularPolygon(in GeometryData data, Vector2 position, float width, float height, int segmentCount) {
            int nPlus1 = segmentCount + 1;
            int nMinus2 = segmentCount - 2;

            int vertexStart = data.positionList.size;
            int triangleStart = data.triangleList.size;
            int vertexCount = nPlus1;
            int triangleCount = nMinus2 * 3;

            data.positionList.EnsureAdditionalCapacity(vertexCount);
            data.texCoordList0.EnsureAdditionalCapacity(vertexCount);
            data.texCoordList1.EnsureAdditionalCapacity(vertexCount);
            data.triangleList.EnsureAdditionalCapacity(triangleCount);

            Vector3[] positions = data.positionList.array;
            Vector4[] texCoord0 = data.texCoordList0.array;
            int[] triangles = data.triangleList.array;

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            float centerX = position.x + halfWidth;
            float centerY = position.y + halfHeight;

            // todo this probably isn't generating in a fan from the center, I think it should so we can use edge distance sdf
            for (int i = 0; i < nPlus1; i++) {
                float a = (2 * Mathf.PI * i) / segmentCount;
                float x = halfWidth * math.sin(a);
                float y = halfHeight * math.cos(a);
                int vertIdx = vertexStart + i;
                float finalY = -(centerY + y);
                positions[vertIdx].x = centerX + x;
                positions[vertIdx].y = finalY;
                texCoord0[vertIdx].x = 1 - PercentOfRange(centerX + x, position.x, position.x + width);
                texCoord0[vertIdx].y = PercentOfRange(centerY + y, position.y, position.y + height);
            }

            int s = triangleStart;
            for (int i = 0; i < nMinus2; i++) {
                triangles[s++] = triangleStart;
                triangles[s++] = triangleStart + i + 1;
                triangles[s++] = triangleStart + i + 2;
            }

            data.triangleList.size += triangleCount;
            data.positionList.size += vertexCount;
            data.texCoordList0.size += vertexCount;
            data.texCoordList1.size += vertexCount;

            return new GeometryRange() {
                vertexStart = vertexStart,
                vertexEnd = vertexStart + vertexCount,
                triangleStart = triangleStart,
                triangleEnd = triangleStart + triangleCount
            };
        }
        
        public static GeometryRange FillDecoratedRect(in GeometryData data, Vector2 position, float width, float height, in CornerDefinition cornerDefinition) {
            data.positionList.EnsureAdditionalCapacity(9);
            data.texCoordList0.EnsureAdditionalCapacity(9);
            data.texCoordList1.EnsureAdditionalCapacity(9);
            data.triangleList.EnsureAdditionalCapacity(24);

            Vector3[] positions = data.positionList.array;
            Vector4[] texCoord0 = data.texCoordList0.array;
            int[] triangles = data.triangleList.array;

            int startVert = data.positionList.size;
            int startTriangle = data.triangleList.size;

            positions[startVert + 0] = new Vector3(position.x + 0, -(position.y + cornerDefinition.topLeftY), 0);
            positions[startVert + 1] = new Vector3(position.x + cornerDefinition.topLeftX, -position.y, 0);
            positions[startVert + 2] = new Vector3(position.x + width - cornerDefinition.topRightX, -position.y, 0);
            positions[startVert + 3] = new Vector3(position.x + width, -(position.y + cornerDefinition.topRightY), 0);
            positions[startVert + 4] = new Vector3(position.x + width, -(position.y + height - cornerDefinition.bottomRightY), 0);
            positions[startVert + 5] = new Vector3(position.x + width - cornerDefinition.bottomRightX, -(position.y + height), 0);
            positions[startVert + 6] = new Vector3(position.x + cornerDefinition.bottomLeftX, -(position.y + height), 0);
            positions[startVert + 7] = new Vector3(position.x + 0, -(position.y + height - cornerDefinition.bottomLeftY), 0);
            positions[startVert + 8] = new Vector3(position.x + width * 0.5f, -(position.y + (height * 0.5f)), 0);

            triangles[startTriangle + 0] = startVert + 1;
            triangles[startTriangle + 1] = startVert + 8;
            triangles[startTriangle + 2] = startVert + 0;

            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 8;
            triangles[startTriangle + 5] = startVert + 1;

            triangles[startTriangle + 6] = startVert + 3;
            triangles[startTriangle + 7] = startVert + 8;
            triangles[startTriangle + 8] = startVert + 2;

            triangles[startTriangle + 9] = startVert + 4;
            triangles[startTriangle + 10] = startVert + 8;
            triangles[startTriangle + 11] = startVert + 3;

            triangles[startTriangle + 12] = startVert + 5;
            triangles[startTriangle + 13] = startVert + 8;
            triangles[startTriangle + 14] = startVert + 4;

            triangles[startTriangle + 15] = startVert + 6;
            triangles[startTriangle + 16] = startVert + 8;
            triangles[startTriangle + 17] = startVert + 5;

            triangles[startTriangle + 18] = startVert + 7;
            triangles[startTriangle + 19] = startVert + 8;
            triangles[startTriangle + 20] = startVert + 6;

            triangles[startTriangle + 21] = startVert + 0;
            triangles[startTriangle + 22] = startVert + 8;
            triangles[startTriangle + 23] = startVert + 7;

            for (int i = 0; i < 9; i++) {
                float x = (positions[startVert + i].x - position.x) / width;
                float y = 1 - ((positions[startVert + i].y + position.y) / -height);
                texCoord0[startVert + i] = new Vector4(x, y, x, y);
            }

            data.triangleList.size += 24;
            data.positionList.size += 9;
            data.texCoordList0.size += 9;
            data.texCoordList1.size += 9;

            return new GeometryRange() {
                vertexStart = startVert,
                vertexEnd = startVert + 9,
                triangleStart = startTriangle,
                triangleEnd = startTriangle + 24
            };
        }

        public static GeometryRange FillRect(in GeometryData data, float x, float y, float width, float height) {
            data.positionList.EnsureAdditionalCapacity(4);
            data.texCoordList0.EnsureAdditionalCapacity(4);
            data.texCoordList1.EnsureAdditionalCapacity(4);
            data.triangleList.EnsureAdditionalCapacity(6);

            Vector3 p0 = new Vector3(x, -y);
            Vector3 p1 = new Vector3(x + width, -y);
            Vector3 p2 = new Vector3(x + width, -(y + height));
            Vector3 p3 = new Vector3(x, -(y + height));

            Vector4 uv0 = new Vector4(0, 1);
            Vector4 uv1 = new Vector4(1, 1);
            Vector4 uv2 = new Vector4(1, 0);
            Vector4 uv3 = new Vector4(0, 0);

            int startVert = data.positionList.size;
            int startTriangle = data.triangleList.size;

            Vector3[] positions = data.positionList.array;
            Vector4[] texCoord0 = data.texCoordList0.array;
            int[] triangles = data.triangleList.array;

            positions[startVert + 0] = p0;
            positions[startVert + 1] = p1;
            positions[startVert + 2] = p2;
            positions[startVert + 3] = p3;

            texCoord0[startVert + 0] = uv0;
            texCoord0[startVert + 1] = uv1;
            texCoord0[startVert + 2] = uv2;
            texCoord0[startVert + 3] = uv3;

            triangles[startTriangle + 0] = startVert + 0;
            triangles[startTriangle + 1] = startVert + 1;
            triangles[startTriangle + 2] = startVert + 2;
            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 3;
            triangles[startTriangle + 5] = startVert + 0;

            data.positionList.size += 4;
            data.texCoordList0.size += 4;
            data.texCoordList1.size += 4;
            data.triangleList.size += 6;

            return new GeometryRange() {
                vertexStart = startVert,
                vertexEnd = startVert + 4,
                triangleStart = startTriangle,
                triangleEnd = startTriangle + 6
            };
        }

        public static GeometryRange StrokeRect(GeometryData data, float x, float y, float width, float height, float strokeWidth) {
            data.positionList.EnsureAdditionalCapacity(8);
            data.texCoordList0.EnsureAdditionalCapacity(8);
            data.texCoordList1.EnsureAdditionalCapacity(8);
            data.triangleList.EnsureAdditionalCapacity(24);

            Vector3 p0 = new Vector3(x, -y);
            Vector3 p1 = new Vector3(x + width, -y);
            Vector3 p2 = new Vector3(x + width, -(y + height));
            Vector3 p3 = new Vector3(x, -(y + height));
            Vector3 p0Inset = p0 + new Vector3(strokeWidth, -strokeWidth);
            Vector3 p1Inset = p1 + new Vector3(-strokeWidth, -strokeWidth);
            Vector3 p2Inset = p2 + new Vector3(-strokeWidth, strokeWidth);
            Vector3 p3Inset = p3 + new Vector3(strokeWidth, strokeWidth);

            Vector3[] positions = data.positionList.array;
            Vector4[] texCoord0 = data.texCoordList0.array;
            int[] triangles = data.triangleList.array;

            int startVert = data.positionList.size;
            int startTriangle = data.triangleList.size;

            float xMax = x + width;
            float yMax = -(y + height);

            const int p0_index = 0;
            const int p1_index = 1;
            const int p2_index = 2;
            const int p3_index = 3;
            const int p0_inset_index = 4;
            const int p1_inset_index = 5;
            const int p2_inset_index = 6;
            const int p3_inset_index = 7;

            positions[startVert + 0] = p0;
            positions[startVert + 1] = p1;
            positions[startVert + 2] = p2;
            positions[startVert + 3] = p3;
            positions[startVert + 4] = p0Inset;
            positions[startVert + 5] = p1Inset;
            positions[startVert + 6] = p2Inset;
            positions[startVert + 7] = p3Inset;

            texCoord0[startVert + 0] = new Vector4(1 - PercentOfRange(p0.x, x, xMax), PercentOfRange(p0.y, -y, yMax));
            texCoord0[startVert + 1] = new Vector4(1 - PercentOfRange(p1.x, x, xMax), PercentOfRange(p1.y, -y, yMax));
            texCoord0[startVert + 2] = new Vector4(1 - PercentOfRange(p2.x, x, xMax), PercentOfRange(p2.y, -y, yMax));
            texCoord0[startVert + 3] = new Vector4(1 - PercentOfRange(p3.x, x, xMax), PercentOfRange(p3.y, -y, yMax));
            texCoord0[startVert + 4] = new Vector4(1 - PercentOfRange(p0Inset.x, x, xMax), PercentOfRange(p0Inset.y, -y, yMax));
            texCoord0[startVert + 5] = new Vector4(1 - PercentOfRange(p1Inset.x, x, xMax), PercentOfRange(p1Inset.y, -y, yMax));
            texCoord0[startVert + 6] = new Vector4(1 - PercentOfRange(p2Inset.x, x, xMax), PercentOfRange(p2Inset.y, -y, yMax));
            texCoord0[startVert + 7] = new Vector4(1 - PercentOfRange(p3Inset.x, x, xMax), PercentOfRange(p3Inset.y, -y, yMax));

            triangles[startTriangle + 0] = startVert + p0_index;
            triangles[startTriangle + 1] = startVert + p1_index;
            triangles[startTriangle + 2] = startVert + p0_inset_index;

            triangles[startTriangle + 3] = startVert + p1_index;
            triangles[startTriangle + 4] = startVert + p1_inset_index;
            triangles[startTriangle + 5] = startVert + p0_inset_index;

            triangles[startTriangle + 6] = startVert + p1_index;
            triangles[startTriangle + 7] = startVert + p2_index;
            triangles[startTriangle + 8] = startVert + p2_inset_index;

            triangles[startTriangle + 9] = startVert + p2_inset_index;
            triangles[startTriangle + 10] = startVert + p1_inset_index;
            triangles[startTriangle + 11] = startVert + p1_index;

            triangles[startTriangle + 12] = startVert + p2_index;
            triangles[startTriangle + 13] = startVert + p3_index;
            triangles[startTriangle + 14] = startVert + p3_inset_index;

            triangles[startTriangle + 15] = startVert + p2_index;
            triangles[startTriangle + 16] = startVert + p3_inset_index;
            triangles[startTriangle + 17] = startVert + p2_inset_index;

            triangles[startTriangle + 18] = startVert + p3_index;
            triangles[startTriangle + 19] = startVert + p0_index;
            triangles[startTriangle + 20] = startVert + p0_inset_index;

            triangles[startTriangle + 21] = startVert + p0_inset_index;
            triangles[startTriangle + 22] = startVert + p3_inset_index;
            triangles[startTriangle + 23] = startVert + p3_index;

            data.positionList.size += 8;
            data.texCoordList0.size += 8;
            data.texCoordList1.size += 8;
            data.triangleList.size += 24;

            return new GeometryRange() {
                vertexStart = startVert,
                vertexEnd = startVert + 8,
                triangleStart = startTriangle,
                triangleEnd = startTriangle + 24
            };
        }
        
        public struct PathData {

            public Rect bounds;
            public RangeInt pointRange;
            public RangeInt holeRange;
            public StructList<ShapeGenerator.PathPoint> points;
            public StructList<ShapeGenerator.PathPoint> holes;

        }

        public static GeometryRange FillClosedPath(in PathData pathData, in GeometryData data) {
            int pointRangeStart = pathData.pointRange.start;
            int pointRangeEnd = pathData.pointRange.end;

            int holeRangeStart = pathData.holeRange.start;
            int holeRangeEnd = pathData.holeRange.end;

            int vertexStart = data.positionList.size;
            int triangleStart = data.triangleList.size;

            ShapeGenerator.PathPoint[] points = pathData.points.array;
            ShapeGenerator.PathPoint[] holes = pathData.holes.array;

            s_FloatScratch.EnsureCapacity(2 * (pointRangeEnd - pointRangeStart));
            data.positionList.EnsureAdditionalCapacity(pointRangeEnd - pointRangeStart);
            data.texCoordList0.EnsureAdditionalCapacity(pointRangeEnd - pointRangeStart);

            int floatIdx = 0;
            float[] floats = s_FloatScratch.Array;
            Vector3[] positions = data.positionList.array;
            Vector4[] texCoord0 = data.texCoordList0.array;
            int vertexIdx = vertexStart;

            float minX = pathData.bounds.xMin;
            float maxX = pathData.bounds.xMax;
            float minY = pathData.bounds.yMin;
            float maxY = pathData.bounds.yMax;

            for (int j = pointRangeStart; j < pointRangeEnd; j++) {
                Vector2 position = points[j].position;
                floats[floatIdx++] = position.x;
                floats[floatIdx++] = -position.y;
                positions[vertexIdx] = new Vector3(position.x, -position.y);
                texCoord0[vertexIdx] = new Vector4(
                    PercentOfRange(position.x, minX, maxX),
                    1 - PercentOfRange(position.y, minY, maxY)
                );
                vertexIdx++;
            }

            for (int j = holeRangeStart; j < holeRangeEnd; j++) {
                if ((holes[j].flags & ShapeGenerator.PointFlag.HoleStart) != 0) {
                    s_IntScratch0.Add(vertexIdx);
                }

                Vector2 position = holes[j].position;
                floats[floatIdx++] = position.x;
                floats[floatIdx++] = -position.y;
                positions[vertexIdx] = new Vector3(position.x, -position.y);
                texCoord0[vertexIdx] = new Vector4(
                    PercentOfRange(position.x, minX, maxX),
                    1 - PercentOfRange(position.y, minY, maxY)
                );
                vertexIdx++;
            }

            s_FloatScratch.size = floatIdx;

            Earcut.Tessellate(s_FloatScratch, s_IntScratch0, s_IntScratch1);

            int count = s_IntScratch1.size;
            int[] tessellatedIndices = s_IntScratch1.array;

            data.triangleList.EnsureAdditionalCapacity(count);
            int triangleIdx = data.triangleList.size;
            int[] triangles = data.triangleList.array;

            for (int j = 0; j < count; j++) {
                triangles[triangleIdx++] = tessellatedIndices[j];
            }

            s_IntScratch0.Count = 0;
            s_IntScratch1.Count = 0;
            s_FloatScratch.Count = 0;

            data.positionList.size = vertexIdx;
            data.texCoordList0.size = vertexIdx;
            data.triangleList.size = triangleIdx;

            return new GeometryRange() {
                vertexStart = vertexStart,
                vertexEnd = vertexIdx,
                triangleStart = triangleStart,
                triangleEnd = triangleIdx
            };
        }

        public static GeometryRange FillRhombus(in GeometryData data, in Rect bounds) {
            data.positionList.EnsureAdditionalCapacity(4);
            data.texCoordList0.EnsureAdditionalCapacity(4);
            data.texCoordList1.EnsureAdditionalCapacity(4);
            data.triangleList.EnsureAdditionalCapacity(6);

            float x = bounds.x;
            float y = -bounds.y;
            float width = bounds.width;
            float height = bounds.height;

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            Vector4 uv0 = new Vector4(0.5f, 1);
            Vector4 uv1 = new Vector4(1, 0.5f);
            Vector4 uv2 = new Vector4(0.5f, 0f);
            Vector4 uv3 = new Vector4(0f, 0.5f);

            int startVert = data.positionList.size;
            int startTriangle = data.triangleList.size;
            Vector3[] positions = data.positionList.array;
            Vector4[] texCoord0 = data.texCoordList0.array;
            int[] triangles = data.triangleList.array;

            positions[startVert + 0] = new Vector3(x + halfWidth, -y);
            positions[startVert + 1] = new Vector3(x + width, -(y + halfHeight));
            positions[startVert + 2] = new Vector3(x + halfWidth, -(y + height));
            positions[startVert + 3] = new Vector3(x, -(y + halfHeight));

            texCoord0[startVert + 0] = uv0;
            texCoord0[startVert + 1] = uv1;
            texCoord0[startVert + 2] = uv2;
            texCoord0[startVert + 3] = uv3;

            int vertexCount = startVert + 4;
            triangles[startTriangle + 0] = vertexCount + 0;
            triangles[startTriangle + 1] = vertexCount + 1;
            triangles[startTriangle + 2] = vertexCount + 2;
            triangles[startTriangle + 3] = vertexCount + 2;
            triangles[startTriangle + 4] = vertexCount + 3;
            triangles[startTriangle + 5] = vertexCount + 0;

            data.positionList.size += 4;
            data.texCoordList0.size += 4;
            data.texCoordList1.size += 4;
            data.triangleList.size += 6;

            return new GeometryRange() {
                vertexStart = startVert,
                vertexEnd = startVert + 4,
                triangleStart = startTriangle,
                triangleEnd = startTriangle + 6
            };
        }

        public static GeometryRange FillTriangle(in GeometryData data, Vector2 p0, Vector2 p1, Vector2 p2) {
            data.positionList.EnsureAdditionalCapacity(3);
            data.texCoordList0.EnsureAdditionalCapacity(3);
            data.texCoordList1.EnsureAdditionalCapacity(3);
            data.triangleList.EnsureAdditionalCapacity(3);

            float minX = p0.x;
            float minY = p0.y;
            float maxX = p0.x;
            float maxY = p0.y;
            minX = p1.x < minX ? p1.x : minX;
            minX = p2.x < minX ? p2.x : minX;
            minY = p1.y < minY ? p1.y : minY;
            minY = p2.y < minY ? p2.y : minY;
            maxX = p1.x > maxX ? p1.x : maxX;
            maxX = p2.x > maxX ? p2.x : maxX;
            maxY = p1.y > maxY ? p1.y : maxY;
            maxY = p2.y > maxY ? p2.y : maxY;

            Vector4 uv0 = new Vector4(
                PercentOfRange(p0.x, minX, maxX),
                1 - PercentOfRange(p0.y, minY, maxY)
            );

            Vector4 uv1 = new Vector4(
                PercentOfRange(p1.x, minX, maxX),
                1 - PercentOfRange(p1.y, minY, maxY)
            );

            Vector4 uv2 = new Vector4(
                PercentOfRange(p2.x, minX, maxX),
                1 - PercentOfRange(p2.y, minY, maxY)
            );

            int startVert = data.positionList.size;
            int startTriangle = data.triangleList.size;
            Vector3[] positions = data.positionList.array;
            Vector4[] texCoord0 = data.texCoordList0.array;
            int[] triangles = data.triangleList.array;

            positions[startVert + 0] = new Vector3(p0.x, -p0.y);
            positions[startVert + 1] = new Vector3(p1.x, -p1.y);
            positions[startVert + 2] = new Vector3(p2.x, -p2.y);

            texCoord0[startVert + 0] = uv0;
            texCoord0[startVert + 1] = uv1;
            texCoord0[startVert + 2] = uv2;

            triangles[startTriangle + 0] = startTriangle + 0;
            triangles[startTriangle + 1] = startTriangle + 1;
            triangles[startTriangle + 2] = startTriangle + 2;

            data.positionList.size += 3;
            data.texCoordList0.size += 3;
            data.texCoordList1.size += 3;
            data.triangleList.size += 3;

            return new GeometryRange() {
                vertexStart = startVert,
                vertexEnd = startVert + 3,
                triangleStart = startTriangle,
                triangleEnd = startTriangle + 3
            };
        }

        private static float PercentOfRange(float v, float bottom, float top) {
            float div = top - bottom;
            return div == 0 ? 0 : (v - bottom) / div;
        }

        private static void GenerateStartCap(in GeometryData data, ShapeGenerator.PathPoint[] pathPointArray, int startIdx, in RenderState renderState) {
            float halfStrokeWidth = renderState.strokeWidth * 0.5f;
            Vector2 start = pathPointArray[startIdx + 0].position;
            Vector2 next = pathPointArray[startIdx + 1].position;
            start.y = -start.y;
            next.y = -next.y;
            Vector2 fromNext = (start - next).normalized;
            Vector2 perp = new Vector2(-fromNext.y, fromNext.x);
            int vertexStart = data.positionList.size;
            int triangleStart = data.triangleList.size;

            if (renderState.lineCap == LineCap.Round) {
                throw new NotImplementedException();
            }
            else if (renderState.lineCap == LineCap.TriangleOut) {
                data.positionList.EnsureAdditionalCapacity(3, 3);
                data.texCoordList0.EnsureAdditionalCapacity(3, 3);
                data.texCoordList1.EnsureAdditionalCapacity(3, 3);
                data.triangleList.EnsureAdditionalCapacity(3, 3);

                data.positionList.array[vertexStart + 0] = start + (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 1] = start - (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 2] = start + (fromNext * halfStrokeWidth);

                data.triangleList.array[triangleStart + 0] = vertexStart + 0;
                data.triangleList.array[triangleStart + 1] = vertexStart + 1;
                data.triangleList.array[triangleStart + 2] = vertexStart + 2;

                data.positionList.size += 3;
                data.texCoordList0.size += 3;
                data.texCoordList1.size += 3;
                data.triangleList.size += 3;
            }
            else if (renderState.lineCap == LineCap.TriangleIn) {
                data.positionList.EnsureAdditionalCapacity(6);
                data.texCoordList0.EnsureAdditionalCapacity(6);
                data.texCoordList1.EnsureAdditionalCapacity(6);
                data.triangleList.EnsureAdditionalCapacity(6);

                data.positionList.array[vertexStart + 0] = start;
                data.positionList.array[vertexStart + 1] = start + (fromNext * halfStrokeWidth) + (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 2] = start + (perp * halfStrokeWidth);

                data.positionList.array[vertexStart + 3] = start;
                data.positionList.array[vertexStart + 4] = start + (fromNext * halfStrokeWidth) - (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 5] = start + (perp * -halfStrokeWidth);


                data.triangleList.array[triangleStart + 0] = vertexStart + 0;
                data.triangleList.array[triangleStart + 1] = vertexStart + 1;
                data.triangleList.array[triangleStart + 2] = vertexStart + 2;

                data.triangleList.array[triangleStart + 3] = vertexStart + 3;
                data.triangleList.array[triangleStart + 4] = vertexStart + 4;
                data.triangleList.array[triangleStart + 5] = vertexStart + 5;

                data.positionList.size += 6;
                data.texCoordList0.size += 6;
                data.texCoordList1.size += 6;
                data.triangleList.size += 6;
            }
            else if (renderState.lineCap == LineCap.Square) {
                data.positionList.EnsureAdditionalCapacity(4);
                data.texCoordList0.EnsureAdditionalCapacity(4);
                data.texCoordList1.EnsureAdditionalCapacity(4);
                data.triangleList.EnsureAdditionalCapacity(6);

                data.positionList.array[vertexStart + 0] = start + (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 1] = start - (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 2] = start + (fromNext * halfStrokeWidth) + (perp * -halfStrokeWidth);
                data.positionList.array[vertexStart + 3] = start + (fromNext * halfStrokeWidth) - (perp * -halfStrokeWidth);

                data.triangleList.array[triangleStart + 0] = vertexStart + 0;
                data.triangleList.array[triangleStart + 1] = vertexStart + 1;
                data.triangleList.array[triangleStart + 2] = vertexStart + 2;
                data.triangleList.array[triangleStart + 3] = vertexStart + 2;
                data.triangleList.array[triangleStart + 4] = vertexStart + 3;
                data.triangleList.array[triangleStart + 5] = vertexStart + 0;

                data.positionList.size += 4;
                data.texCoordList0.size += 4;
                data.texCoordList1.size += 4;
                data.triangleList.size += 6;
            }
        }

        private static void GenerateEndCap(GeometryData data, ShapeGenerator.PathPoint[] pathPointArray, int endIndex, in RenderState renderState) {
            float halfStrokeWidth = renderState.strokeWidth * 0.5f;
            Vector2 end = pathPointArray[endIndex - 1].position;
            Vector2 prev = pathPointArray[endIndex - 2].position;
            end.y = -end.y;
            prev.y = -prev.y;

            Vector2 fromPrev = (prev - end).normalized;
            Vector2 perp = new Vector2(-fromPrev.y, fromPrev.x);
            int vertexStart = data.positionList.size;
            int triangleStart = data.triangleList.size;
            if (renderState.lineCap == LineCap.Round) {
//                 Vector2 center = end;
//                 int segmentCount = (int) (math.abs(Math.PI * halfStrokeWidth) / 5) + 1;
//
//                 Vector3[] positions = retn.positions.array;
//                 int vertIdx = retn.vertexCount;
//                 int triIdx = retn.triangleCount;
//                 int[] triangles = retn.triangles.array;
//                 
//                 float angleInc = 180f / segmentCount;      
//                 
//                 for (int i = 0; i < segmentCount; i++) {
//                     
//                     positions[vertIdx++] = new Vector3(center.x, center.y);
//
//                     positions[vertIdx++] = new Vector3(
//                         center.x + halfStrokeWidth * math.cos(angleInc * i),
//                         center.y + halfStrokeWidth * math.sin(angleInc * i)
//                     );
//
//                     positions[vertIdx++] = new Vector3(
//                         center.x + halfStrokeWidth * math.cos(angleInc * (1 + i)),
//                         center.y + halfStrokeWidth * math.sin(angleInc * (1 + i))
//                     );
//
//                     triangles[triIdx++] = vertexCount + 0;
//                     triangles[triIdx++] = vertexCount + 1;
//                     triangles[triIdx++] = vertexCount + 2;
//                     vertexCount += 3;
//                 }
//                 
            }
            else if (renderState.lineCap == LineCap.TriangleOut) {
                data.positionList.EnsureAdditionalCapacity(3, 3);
                data.texCoordList0.EnsureAdditionalCapacity(3, 3);
                data.texCoordList1.EnsureAdditionalCapacity(3, 3);
                data.triangleList.EnsureAdditionalCapacity(3, 3);

                data.positionList.array[vertexStart + 0] = end - (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 1] = end + (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 2] = end - (fromPrev * halfStrokeWidth);

                data.triangleList.array[triangleStart + 0] = vertexStart + 0;
                data.triangleList.array[triangleStart + 1] = vertexStart + 1;
                data.triangleList.array[triangleStart + 2] = vertexStart + 2;

                data.positionList.size += 3;
                data.texCoordList0.size += 3;
                data.texCoordList1.size += 3;
                data.triangleList.size += 3;
            }
            else if (renderState.lineCap == LineCap.TriangleIn) {
                data.positionList.EnsureAdditionalCapacity(6);
                data.texCoordList0.EnsureAdditionalCapacity(6);
                data.texCoordList1.EnsureAdditionalCapacity(6);
                data.triangleList.EnsureAdditionalCapacity(6);

                data.positionList.array[vertexStart + 0] = end;
                data.positionList.array[vertexStart + 1] = end - (fromPrev * halfStrokeWidth) - (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 2] = end - (perp * halfStrokeWidth);

                data.positionList.array[vertexStart + 3] = end;
                data.positionList.array[vertexStart + 4] = end - (fromPrev * halfStrokeWidth) + (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 5] = end - (perp * -halfStrokeWidth);


                data.triangleList.array[triangleStart + 0] = vertexStart + 0;
                data.triangleList.array[triangleStart + 1] = vertexStart + 1;
                data.triangleList.array[triangleStart + 2] = vertexStart + 2;

                data.triangleList.array[triangleStart + 3] = vertexStart + 3;
                data.triangleList.array[triangleStart + 4] = vertexStart + 4;
                data.triangleList.array[triangleStart + 5] = vertexStart + 5;

                data.positionList.size += 6;
                data.texCoordList0.size += 6;
                data.texCoordList1.size += 6;
                data.triangleList.size += 6;
            }
            else if (renderState.lineCap == LineCap.Square) {
                data.positionList.EnsureAdditionalCapacity(4);
                data.texCoordList0.EnsureAdditionalCapacity(4);
                data.texCoordList1.EnsureAdditionalCapacity(4);
                data.triangleList.EnsureAdditionalCapacity(6);

                data.positionList.array[vertexStart + 0] = end - (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 1] = end + (perp * halfStrokeWidth);
                data.positionList.array[vertexStart + 2] = end - (fromPrev * halfStrokeWidth) - (perp * -halfStrokeWidth);
                data.positionList.array[vertexStart + 3] = end - (fromPrev * halfStrokeWidth) + (perp * -halfStrokeWidth);

                data.triangleList.array[triangleStart + 0] = vertexStart + 0;
                data.triangleList.array[triangleStart + 1] = vertexStart + 1;
                data.triangleList.array[triangleStart + 2] = vertexStart + 2;
                data.triangleList.array[triangleStart + 3] = vertexStart + 2;
                data.triangleList.array[triangleStart + 4] = vertexStart + 3;
                data.triangleList.array[triangleStart + 5] = vertexStart + 0;

                data.positionList.size += 4;
                data.texCoordList0.size += 4;
                data.texCoordList1.size += 4;
                data.triangleList.size += 6;
            }
        }

        public static GeometryRange StrokeOpenPath(in GeometryData data, StructList<ShapeGenerator.PathPoint> pathPoints, in RangeInt pointRange, in RenderState renderState) {
            if (pointRange.length < 2) {
                return default;
            }

            int vertexStart = data.positionList.size;
            int triangleStart = data.triangleList.size;

            int pointStart = pointRange.start;
            ShapeGenerator.PathPoint[] points = pathPoints.array;

            for (int i = pointStart + 1; i < pathPoints.size; i++) {
                if (points[i].flags == ShapeGenerator.PointFlag.Move) {
                    StrokePathSegment(data, pathPoints, new RangeInt(pointStart, i - pointStart), renderState);
                    pointStart = i;
                }
            }

            StrokePathSegment(data, pathPoints, new RangeInt(pointStart, pathPoints.size - pointStart), renderState);

            return new GeometryRange() {
                vertexStart = vertexStart,
                vertexEnd = data.positionList.size,
                triangleStart = triangleStart,
                triangleEnd = data.triangleList.size
            };
        }

        private static void StrokePathSegment(in GeometryData data, StructList<ShapeGenerator.PathPoint> pathPoints, in RangeInt pointRange, in RenderState renderState) {
            if (pointRange.length < 2) {
                return;
            }

            float halfStrokeWidth = renderState.strokeWidth * 0.5f;
            LineJoin join = renderState.lineJoin;
            int miterLimit = renderState.miterLimit;

            ComputeOpenPathSegments(pointRange, pathPoints, s_ScratchVector2);

            int count = s_ScratchVector2.size;
            Vector2[] midpoints = s_ScratchVector2.array;

            GenerateStartCap(data, pathPoints.array, pointRange.start, renderState);

            EnsureCapacityForStrokeTriangles(data, join, count / 2);

            for (int i = 1; i < count - 1; i++) {
                // todo -- replace w/ Vase, this sucks for lots of reasons
                CreateStrokeTriangles(data, midpoints[i - 1], midpoints[i], midpoints[i + 1], halfStrokeWidth, join, miterLimit);
            }

            GenerateEndCap(data, pathPoints.array, pointRange.end, renderState);
        }

        public static GeometryRange StrokeClosedPath(in GeometryData data, StructList<ShapeGenerator.PathPoint> pathPoints, in RangeInt pointRange, in RenderState renderState) {
            if (pointRange.length < 2) {
                return default;
            }

            float halfStrokeWidth = renderState.strokeWidth * 0.5f;
            LineJoin join = renderState.lineJoin;
            int miterLimit = renderState.miterLimit;

            int vertexStart = data.positionList.size;
            int triangleStart = data.triangleList.size;

            ComputeClosedPathSegments(pointRange, pathPoints, s_ScratchVector2);

            int count = s_ScratchVector2.size;
            Vector2[] points = s_ScratchVector2.array;

            EnsureCapacityForStrokeTriangles(data, join, count / 2);

            for (int i = 0; i < count - 2; i += 2) {
                CreateStrokeTriangles(data, points[i], points[i + 1], points[i + 2], halfStrokeWidth, join, miterLimit);
            }

            return new GeometryRange() {
                vertexStart = vertexStart,
                vertexEnd = data.positionList.size,
                triangleStart = triangleStart,
                triangleEnd = data.triangleList.size
            };
        }

        private static void ComputeClosedPathSegments(RangeInt range, StructList<ShapeGenerator.PathPoint> points, StructList<Vector2> segments) {
            ShapeGenerator.PathPoint[] pointData = points.array;
            int start = range.start + 1;
            int end = range.end - 1;
            segments.EnsureCapacity(range.length * 2);
            segments.size = 0;
            int ptIdx = 0;

            // find first mid point
            Vector2 p0 = pointData[start - 1].position;
            Vector2 m0 = (p0 + pointData[start].position) * 0.5f;

            segments[ptIdx++] = m0;

            for (int i = start; i < end; i++) {
                Vector2 current = pointData[i + 0].position;
                Vector2 next = pointData[i + 1].position;
                segments[ptIdx++] = current;
                segments[ptIdx++] = (current + next) * 0.5f;
            }

            segments[ptIdx++] = points[end].position;
            segments[ptIdx++] = (points[end].position + p0) * 0.5f;
            segments[ptIdx++] = p0;
            segments[ptIdx++] = m0;

            segments.size = ptIdx;
        }

        private static void ComputeOpenPathSegments(RangeInt range, StructList<ShapeGenerator.PathPoint> points, StructList<Vector2> midpoints) {
            int count = range.length;
            ShapeGenerator.PathPoint[] pointData = points.array;

            midpoints.EnsureCapacity(count * 2);
            midpoints.size = 0;

            Vector2[] data = midpoints.array;

            int start = range.start;
            int end = range.start + count;
            int ptIdx = 0;
            data[ptIdx].x = points[start].position.x;
            data[ptIdx].y = points[start].position.y;
            ptIdx++;

            for (int i = start + 1; i < end; i++) {
                Vector2 p0 = pointData[i - 1].position;
                Vector2 p1 = pointData[i - 0].position;
                data[ptIdx].x = (p0.x + p1.x) * 0.5f;
                data[ptIdx].y = (p0.y + p1.y) * 0.5f;
                ptIdx++;
            }

            data[ptIdx].x = points[end - 1].position.x;
            data[ptIdx].y = points[end - 1].position.y;
            midpoints.size = ptIdx + 1;
        }

        private static void EnsureCapacityForStrokeTriangles(in GeometryData data, LineJoin lineJoin, int segmentCount) {
            // 4 before join 
            // 4 after join
            // 3 - 6 for miter / bevel
            // ? for round, guessing about 16 but who won't know until we hit that case
            int beforeJoinSize = 4 * segmentCount;
            int afterJoinSize = 4 * segmentCount;
            int size = 0;
            if (lineJoin == LineJoin.Bevel || lineJoin == LineJoin.Miter || lineJoin == LineJoin.MiterClip) {
                size = beforeJoinSize + afterJoinSize + (6 * segmentCount);
            }
            else {
                size = beforeJoinSize + afterJoinSize + (16 * segmentCount);
            }

            data.positionList.EnsureAdditionalCapacity(size);
            data.texCoordList0.EnsureAdditionalCapacity(size);
            data.texCoordList1.EnsureAdditionalCapacity(size);
            data.triangleList.EnsureAdditionalCapacity((int) (size * 1.5));
        }

        private static void CreateStrokeTriangles(in GeometryData data, Vector2 p0, Vector2 p1, Vector2 p2, float strokeWidth, LineJoin joinType, int miterLimit) {
            p0.y = -p0.y;
            p1.y = -p1.y;
            p2.y = -p2.y;

            Vector2 t0 = p1 - p0;
            Vector2 t2 = p2 - p1;

            float tempX = t0.x;
            t0.x = -t0.y;
            t0.y = tempX;

            tempX = t2.x;
            t2.x = -t2.y;
            t2.y = tempX;

            float signedArea = (p1.x - p0.x) * (p2.y - p0.y) - (p2.x - p0.x) * (p1.y - p0.y);

            // invert if signed area > 0
            if (signedArea > 0) {
                t0.x = -t0.x;
                t0.y = -t0.y;
                t2.x = -t2.x;
                t2.y = -t2.y;
            }

            t0 = t0.normalized;
            t2 = t2.normalized;

            t0 *= strokeWidth;
            t2 *= strokeWidth;

            Vector2 intersection = default;
            Vector2 anchor = default;
            float anchorLength = float.MaxValue;

            bool intersecting = LineIntersect(t0 + p0, t0 + p1, t2 + p2, t2 + p1, out intersection);

            if (intersecting) {
                anchor = intersection - p1;
                anchorLength = anchor.magnitude;
            }

            int dd = (int) (anchorLength / strokeWidth);

            Vector2 p0p1 = p0 - p1;
            float p0p1Length = p0p1.magnitude;

            Vector2 p1p2 = p1 - p2;
            float p1p2Length = p1p2.magnitude;

            int vertIdx = data.positionList.size;
            int triangleIdx = data.triangleList.size;
            Vector3[] positions = data.positionList.array;
            int[] triangles = data.triangleList.array;

//            Color[] colors = retn.colors.array;

            // todo -- texcoords & sdf packing
            // todo -- fix reversed triangles, this only works w/ culling off right now
            // todo -- fix overdraw cases where possible
            // todo -- fix bad bending of joins at sharp angles
            // todo -- use burst & jobs

            // can't use the miter a s reference point
            if (anchorLength > p0p1Length || anchorLength > p1p2Length) {
                Vector2 v0 = p0 - t0;
                Vector2 v1 = p0 + t0;
                Vector2 v2 = p1 + t0;
                Vector2 v3 = p1 - t0;

                Vector2 v4 = p2 + t2;
                Vector2 v5 = p1 + t2;
                Vector2 v6 = p1 - t2;
                Vector2 v7 = p2 - t2;

//                colors[vertIdx + 0] = Color.red;
//                colors[vertIdx + 1] = Color.red;
//                colors[vertIdx + 2] = Color.red;
//                colors[vertIdx + 3] = Color.red;

                ref Vector3 pos = ref positions[vertIdx + 0];
                pos.x = v0.x;
                pos.y = v0.y;
                pos = ref positions[vertIdx + 1];
                pos.x = v1.x;
                pos.y = v1.y;
                pos = ref positions[vertIdx + 2];
                pos.x = v2.x;
                pos.y = v2.y;
                pos = ref positions[vertIdx + 3];
                pos.x = v3.x;
                pos.y = v3.y;
//                positions[vertIdx + 0] = v0;
//                positions[vertIdx + 1] = v1;
//                positions[vertIdx + 2] = v2;
//                positions[vertIdx + 3] = v3;

                triangles[triangleIdx + 0] = vertIdx + 0;
                triangles[triangleIdx + 1] = vertIdx + 1;
                triangles[triangleIdx + 2] = vertIdx + 2;
                triangles[triangleIdx + 3] = vertIdx + 2;
                triangles[triangleIdx + 4] = vertIdx + 3;
                triangles[triangleIdx + 5] = vertIdx + 0;

                vertIdx += 4;
                triangleIdx += 6;

                if (joinType == LineJoin.Round) {
                    // generating unknown geometry in the cap function, have to write out our book keeping
                    data.positionList.size = vertIdx;
                    data.triangleList.size = triangleIdx;

                    CreateRoundJoin(data, p1, v2, v5, p2);
                    vertIdx = data.positionList.size;
                    triangleIdx = data.triangleList.size;

                    positions = data.positionList.array;
                    triangles = data.triangleList.array;
//                    colors = retn.colors.array;
                }
                else if (joinType == LineJoin.Bevel || joinType == LineJoin.Miter && dd >= miterLimit) {
                    positions[vertIdx + 0] = v2;
                    positions[vertIdx + 1] = p1;
                    positions[vertIdx + 2] = v5;

//                    colors[vertIdx + 0] = Color.yellow;
//                    colors[vertIdx + 1] = Color.yellow;
//                    colors[vertIdx + 2] = Color.yellow;

                    triangles[triangleIdx + 0] = vertIdx + 0;
                    triangles[triangleIdx + 1] = vertIdx + 1;
                    triangles[triangleIdx + 2] = vertIdx + 2;

                    vertIdx += 3;
                    triangleIdx += 3;
                }
                else if (joinType == LineJoin.Miter && dd < miterLimit && intersecting) {
                    positions[vertIdx + 0] = v2;
                    positions[vertIdx + 1] = intersection;
                    positions[vertIdx + 2] = v5;
                    positions[vertIdx + 3] = p1;

//                    colors[vertIdx + 0] = Color.yellow;
//                    colors[vertIdx + 1] = Color.yellow;
//                    colors[vertIdx + 2] = Color.yellow;
//                    colors[vertIdx + 3] = Color.yellow;

                    triangles[triangleIdx + 0] = vertIdx + 0;
                    triangles[triangleIdx + 1] = vertIdx + 1;
                    triangles[triangleIdx + 2] = vertIdx + 2;
                    triangles[triangleIdx + 3] = vertIdx + 2;
                    triangles[triangleIdx + 4] = vertIdx + 3;
                    triangles[triangleIdx + 5] = vertIdx + 0;

                    vertIdx += 4;
                    triangleIdx += 6;
                }

//
//                positions[vertIdx + 0] = v4;
//                positions[vertIdx + 1] = v5;
//                positions[vertIdx + 2] = v6;
//                positions[vertIdx + 3] = v7;
                ref Vector3 pos3 = ref positions[vertIdx + 0];
                pos3.x = v4.x;
                pos3.y = v4.y;
                pos3 = ref positions[vertIdx + 1];
                pos3.x = v5.x;
                pos3.y = v5.y;
                pos3 = ref positions[vertIdx + 2];
                pos3.x = v6.x;
                pos3.y = v6.y;
                pos3 = ref positions[vertIdx + 3];
                pos3.x = v7.x;
                pos3.y = v7.y;

//                colors[vertIdx + 0] = Color.red;
//                colors[vertIdx + 1] = Color.red;
//                colors[vertIdx + 2] = Color.red;
//                colors[vertIdx + 3] = Color.red;

                triangles[triangleIdx + 0] = vertIdx + 0;
                triangles[triangleIdx + 1] = vertIdx + 1;
                triangles[triangleIdx + 2] = vertIdx + 2;
                triangles[triangleIdx + 3] = vertIdx + 2;
                triangles[triangleIdx + 4] = vertIdx + 3;
                triangles[triangleIdx + 5] = vertIdx + 0;

                vertIdx += 4;
                triangleIdx += 6;
            }
            else {
                Vector2 v0 = p1 - anchor;
                Vector2 v1 = p0 - t0;
                Vector2 v2 = p0 + t0;
                Vector2 v3 = p1 + t0;

                Vector2 v4 = v0;
                Vector2 v5 = p1 + t2;
                Vector2 v6 = p2 + t2;
                Vector2 v7 = p2 - t2;
//
//                positions[vertIdx + 0] = v0;
//                positions[vertIdx + 1] = v1;
//                positions[vertIdx + 2] = v2;
//                positions[vertIdx + 3] = v3;
                ref Vector3 pos = ref positions[vertIdx + 0];
                pos.x = v0.x;
                pos.y = v0.y;
                pos = ref positions[vertIdx + 1];
                pos.x = v1.x;
                pos.y = v1.y;
                pos = ref positions[vertIdx + 2];
                pos.x = v2.x;
                pos.y = v2.y;
                pos = ref positions[vertIdx + 3];
                pos.x = v3.x;
                pos.y = v3.y;

//                colors[vertIdx + 0] = Color.white;
//                colors[vertIdx + 1] = Color.white;
//                colors[vertIdx + 2] = Color.white;
//                colors[vertIdx + 3] = Color.white;

                triangles[triangleIdx + 0] = vertIdx + 0;
                triangles[triangleIdx + 1] = vertIdx + 1;
                triangles[triangleIdx + 2] = vertIdx + 2;
                triangles[triangleIdx + 3] = vertIdx + 2;
                triangles[triangleIdx + 4] = vertIdx + 3;
                triangles[triangleIdx + 5] = vertIdx + 0;

                vertIdx += 4;
                triangleIdx += 6;

                if (joinType == LineJoin.Round) {
                    // generating unknown geometry in the cap function, have to write out our book keeping

                    positions[vertIdx + 0] = v3;
                    positions[vertIdx + 1] = p1; // + t2;
                    positions[vertIdx + 2] = v0;

//                    colors[vertIdx + 0] = Color.yellow;
//                    colors[vertIdx + 1] = Color.yellow;
//                    colors[vertIdx + 2] = Color.yellow;

                    triangles[triangleIdx + 0] = vertIdx + 0;
                    triangles[triangleIdx + 1] = vertIdx + 1;
                    triangles[triangleIdx + 2] = vertIdx + 2;

                    vertIdx += 3;
                    triangleIdx += 3;

                    data.positionList.size = vertIdx;
                    data.triangleList.size = triangleIdx;

                    CreateRoundJoin(data, p1, v3, p1 + t2, v0);

                    vertIdx = data.positionList.size;
                    triangleIdx = data.triangleList.size;

                    positions = data.positionList.array;
                    triangles = data.triangleList.array;

//                    colors = retn.colors.array;

                    positions[vertIdx + 0] = p1 - anchor;
                    positions[vertIdx + 1] = p1;
                    positions[vertIdx + 2] = p1 + t2;
//
//                    colors[vertIdx + 0] = Color.yellow;
//                    colors[vertIdx + 1] = Color.yellow;
//                    colors[vertIdx + 2] = Color.yellow;

                    triangles[triangleIdx + 0] = vertIdx + 0;
                    triangles[triangleIdx + 1] = vertIdx + 1;
                    triangles[triangleIdx + 2] = vertIdx + 2;

                    vertIdx += 3;
                    triangleIdx += 3;
                }
                else {
                    if (joinType == LineJoin.Bevel || joinType == LineJoin.Miter && dd >= miterLimit) {
                        positions[vertIdx + 0] = p1 + t2;
                        positions[vertIdx + 1] = p1 - anchor;
                        positions[vertIdx + 2] = p1 + t0;
//
//                        colors[vertIdx + 0] = Color.yellow;
//                        colors[vertIdx + 1] = Color.yellow;
//                        colors[vertIdx + 2] = Color.yellow;

                        triangles[triangleIdx + 0] = vertIdx + 0;
                        triangles[triangleIdx + 1] = vertIdx + 1;
                        triangles[triangleIdx + 2] = vertIdx + 2;

                        vertIdx += 3;
                        triangleIdx += 3;
                    }
                    else if (joinType == LineJoin.Miter && dd < miterLimit) {
                        // todo -- convert to quad

                        positions[vertIdx + 0] = p1 + t2;
                        positions[vertIdx + 1] = p1 - anchor;
                        positions[vertIdx + 2] = p1 + t0;
//
//                        colors[vertIdx + 0] = Color.yellow;
//                        colors[vertIdx + 1] = Color.yellow;
//                        colors[vertIdx + 2] = Color.yellow;

                        triangles[triangleIdx + 0] = vertIdx + 0;
                        triangles[triangleIdx + 1] = vertIdx + 1;
                        triangles[triangleIdx + 2] = vertIdx + 2;

                        vertIdx += 3;
                        triangleIdx += 3;

                        positions[vertIdx + 0] = p1 + t0;
                        positions[vertIdx + 1] = p1 + t2;
                        positions[vertIdx + 2] = intersection;

//                        colors[vertIdx + 0] = Color.yellow;
//                        colors[vertIdx + 1] = Color.yellow;
//                        colors[vertIdx + 2] = Color.yellow;

                        triangles[triangleIdx + 0] = vertIdx + 0;
                        triangles[triangleIdx + 1] = vertIdx + 1;
                        triangles[triangleIdx + 2] = vertIdx + 2;

                        vertIdx += 3;
                        triangleIdx += 3;
                    }
                }

//                positions[vertIdx + 0] = v4;
//                positions[vertIdx + 1] = v5;
//                positions[vertIdx + 2] = v6;
//                positions[vertIdx + 3] = v7;

                ref Vector3 pos2 = ref positions[vertIdx + 0];
                pos2.x = v4.x;
                pos2.y = v4.y;
                pos2 = ref positions[vertIdx + 1];
                pos2.x = v5.x;
                pos2.y = v5.y;
                pos2 = ref positions[vertIdx + 2];
                pos2.x = v6.x;
                pos2.y = v6.y;
                pos2 = ref positions[vertIdx + 3];
                pos2.x = v7.x;
                pos2.y = v7.y;

//                colors[vertIdx + 0] = Color.white;
//                colors[vertIdx + 1] = Color.white;
//                colors[vertIdx + 2] = Color.white;
//                colors[vertIdx + 3] = Color.white;

                triangles[triangleIdx + 0] = vertIdx + 0;
                triangles[triangleIdx + 1] = vertIdx + 1;
                triangles[triangleIdx + 2] = vertIdx + 2;
                triangles[triangleIdx + 3] = vertIdx + 2;
                triangles[triangleIdx + 4] = vertIdx + 3;
                triangles[triangleIdx + 5] = vertIdx + 0;

                vertIdx += 4;
                triangleIdx += 6;
            }

            data.positionList.size = vertIdx;
            data.texCoordList0.size = vertIdx;
            data.texCoordList1.size = vertIdx;
            data.triangleList.size = triangleIdx;
        }

        private static void CreateRoundJoin(in GeometryData data, in Vector2 center, in Vector2 p0, in Vector2 p1, in Vector2 nextInLine) {
            const float Epsilon = 0.0001f;
            float radius = (center - p0).magnitude;
            float angle0 = math.atan2(p1.y - center.y, p1.x - center.x);
            float angle1 = math.atan2(p0.y - center.y, p0.x - center.x);
            float originalAngle = angle0;
            if (angle1 > angle0) {
                if (angle1 - angle0 >= Math.PI - 0.0001) {
                    angle1 -= 2 * Mathf.PI;
                }
            }
            else {
                if (angle0 - angle1 >= Math.PI - 0.0001) {
                    angle0 -= 2 * Mathf.PI;
                }
            }

            float angleDiff = angle1 - angle0;
            if (math.abs(angleDiff) >= Mathf.PI - Epsilon && math.abs(angleDiff) <= Mathf.PI + Epsilon) {
                Vector2 r1 = center - nextInLine;
                if (r1.x == 0) {
                    if (r1.y > 0) {
                        angleDiff = -angleDiff;
                    }
                }
                else if (r1.x >= -Epsilon) {
                    angleDiff = -angleDiff;
                }
            }

            int segmentCount = (int) (math.abs(angleDiff * radius) / 7) + 1;

            float angleInc = angleDiff / segmentCount;

            int vertexCount = data.positionList.size;
            int triangleCount = data.triangleList.size;
            int triIdx = triangleCount;
            int vertIdx = vertexCount;

            data.positionList.EnsureAdditionalCapacity(segmentCount * 3);
            data.texCoordList0.EnsureAdditionalCapacity(segmentCount * 3);
            data.texCoordList1.EnsureAdditionalCapacity(segmentCount * 3);
            data.triangleList.EnsureAdditionalCapacity(segmentCount * 3);

            int[] triangles = data.triangleList.array;
            Vector3[] positions = data.positionList.array;

            // todo -- can do fewer sin / cos if we precompute and store since its always next & prev
            for (int i = 0; i < segmentCount; i++) {
                positions[vertIdx++] = new Vector3(center.x, center.y);

                positions[vertIdx++] = new Vector3(
                    center.x + radius * math.cos(originalAngle + angleInc * i),
                    center.y + radius * math.sin(originalAngle + angleInc * i)
                );

                positions[vertIdx++] = new Vector3(
                    center.x + radius * math.cos(originalAngle + angleInc * (1 + i)),
                    center.y + radius * math.sin(originalAngle + angleInc * (1 + i))
                );

                triangles[triIdx++] = vertexCount + 0;
                triangles[triIdx++] = vertexCount + 1;
                triangles[triIdx++] = vertexCount + 2;

                vertexCount += 3;
            }

            data.positionList.size = vertIdx;
            data.texCoordList0.size = vertIdx;
            data.texCoordList1.size = vertIdx;
            data.triangleList.size = triIdx;
        }

        private static bool LineIntersect(in Vector2 p0, in Vector2 p1, in Vector2 p2, in Vector2 p3, out Vector2 intersection) {
            const float Epsilon = 0.0001f;

            float a0 = p1.y - p0.y;
            float b0 = p0.x - p1.x;

            float a1 = p3.y - p2.y;
            float b1 = p2.x - p3.x;

            float det = a0 * b1 - a1 * b0;
            if (det > -Epsilon && det < Epsilon) {
                intersection = default;
                return false;
            }
            else {
                float c0 = a0 * p0.x + b0 * p0.y;
                float c1 = a1 * p2.x + b1 * p2.y;

                intersection.x = (b1 * c0 - b0 * c1) / det;
                intersection.y = (a0 * c1 - a1 * c0) / det;
                return true;
            }
        }


        // public static void FillSprite(Sprite sprite, Rect rect, GeometryCache retn) {
        //     VertigoUtil.SpriteData spriteData = VertigoUtil.GetSpriteData(sprite);
        //     Vector2[] vertices = spriteData.vertices;
        //     Vector2[] texCoords = spriteData.uvs;
        //     ushort[] spriteTriangles = spriteData.triangles;
        //
        //     retn.EnsureAdditionalCapacity(vertices.Length, spriteData.triangles.Length);
        //     Vector3[] positions = retn.positions.array;
        //     Vector4[] texCoord0 = retn.texCoord0.array;
        //     int[] triangles = retn.triangles.array;
        //     int vertexStart = retn.vertexCount;
        //     int triangleStart = retn.triangleCount;
        //     int vertIdx = retn.vertexCount;
        //     int triIdx = retn.triangleCount;
        //     Vector2 pivot = sprite.pivot;
        //     float ppi = sprite.pixelsPerUnit;
        //
        //     for (int i = 0; i < vertices.Length; i++) {
        //         positions[vertIdx].x = pivot.x - (vertices[i].x * ppi);
        //         positions[vertIdx].y = (vertices[i].y * ppi) - pivot.y;
        //         texCoord0[vertIdx].x = texCoords[i].x;
        //         texCoord0[vertIdx].y = texCoords[i].y;
        //         vertIdx++;
        //     }
        //
        //     if (rect != default) {
        //         vertIdx = vertexStart;
        //         float minX = float.MaxValue;
        //         float minY = float.MaxValue;
        //         float maxX = float.MinValue;
        //         float maxY = float.MinValue;
        //         for (int i = 0; i < vertices.Length; i++) {
        //             float x = positions[vertIdx].x;
        //             float y = -positions[vertIdx].y;
        //             if (x < minX) minX = x;
        //             if (x > maxX) maxX = x;
        //             if (y < minY) minY = y;
        //             if (y > maxY) maxY = y;
        //             vertIdx++;
        //         }
        //
        //         float boundsWidth = maxX - minX;
        //         float boundsHeight = maxY - minY;
        //         float baseX = rect.x;
        //         float baseY = rect.y;
        //         float targetWidth = rect.width;
        //         float targetHeight = rect.height;
        //         vertIdx = vertexStart;
        //
        //         for (int i = 0; i < vertices.Length; i++) {
        //             float x = positions[vertIdx].x;
        //             float y = positions[vertIdx].y;
        //             float percentX = PercentOfRange(x, minX, boundsWidth);
        //             float percentY = PercentOfRange(y, minY, boundsHeight);
        //             positions[vertIdx].x = baseX + (percentX * targetWidth);
        //             positions[vertIdx].y = (percentY * targetHeight) - baseY;
        //             vertIdx++;
        //         }
        //     }
        //
        //     for (int i = 0; i < spriteTriangles.Length; i++) {
        //         triangles[triIdx++] = vertexStart + spriteTriangles[i];
        //     }
        //
        //     retn.shapes.Add(new GeometryShape() {
        //         shapeType = ShapeType.Sprite,
        //         geometryType = GeometryType.Physical,
        //         vertexStart = vertexStart,
        //         vertexCount = vertIdx - vertexStart,
        //         triangleStart = triangleStart,
        //         triangleCount = triIdx - triangleStart
        //     });
        // }

    }

}