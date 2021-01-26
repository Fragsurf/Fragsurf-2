using System;
using UIForia.Layout;
using UIForia.Rendering.Vertigo;
using UIForia.Util;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

namespace UIForia.Rendering {

    public class UIForiaGeometry {

        public StructList<Vector3> positionList;
        public StructList<Vector4> texCoordList0;
        public StructList<Vector4> texCoordList1;
        public StructList<int> triangleList;

        public Vector4 packedColors;
        public Vector4 objectData;
        public Texture mainTexture;
        public Vector4 miscData;
        public Vector4 cornerData;

        public UIForiaGeometry() {
            this.positionList = new StructList<Vector3>();
            this.texCoordList0 = new StructList<Vector4>();
            this.texCoordList1 = new StructList<Vector4>();
            this.triangleList = new StructList<int>();
        }

        public void EnsureAdditionalCapacity(int vertexCount, int triangleCount) {
            positionList.EnsureAdditionalCapacity(vertexCount);
            texCoordList0.EnsureAdditionalCapacity(vertexCount);
            texCoordList1.EnsureAdditionalCapacity(vertexCount);
            triangleList.EnsureAdditionalCapacity(triangleCount);
        }

        public void Clear() {
            mainTexture = null;
            objectData = default;
            packedColors = default;
            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            triangleList.size = 0;
        }

        public void UpdateSizes(int vertexCount, int triangleCount) {
            positionList.size += vertexCount;
            texCoordList0.size += vertexCount;
            texCoordList1.size += vertexCount;
            triangleList.size += triangleCount;
        }

        public void Quad(float width, float height) {
            EnsureAdditionalCapacity(4, 6);

            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            ref Vector3 p0 = ref positions[startVert + 0];
            ref Vector3 p1 = ref positions[startVert + 1];
            ref Vector3 p2 = ref positions[startVert + 2];
            ref Vector3 p3 = ref positions[startVert + 3];

            ref Vector4 uv0 = ref texCoord0[startVert + 0];
            ref Vector4 uv1 = ref texCoord0[startVert + 1];
            ref Vector4 uv2 = ref texCoord0[startVert + 2];
            ref Vector4 uv3 = ref texCoord0[startVert + 3];

            p0.x = 0;
            p0.y = 0;
            p0.z = 0;

            p1.x = width;
            p1.y = 0;
            p1.z = 0;

            p2.x = width;
            p2.y = -height;
            p2.z = 0;

            p3.x = 0;
            p3.y = -height;
            p3.z = 0;

            uv0.x = 0;
            uv0.y = 1;

            uv1.x = 1;
            uv1.y = 1;

            uv2.x = 1;
            uv2.y = 0;

            uv3.x = 0;
            uv3.y = 0;

            triangles[startTriangle + 0] = startVert + 0;
            triangles[startTriangle + 1] = startVert + 1;
            triangles[startTriangle + 2] = startVert + 2;
            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 3;
            triangles[startTriangle + 5] = startVert + 0;

            UpdateSizes(4, 6);
        }

        public void ClipCornerRect(Size size, in CornerDefinition cornerDefinition, in Vector2 position = default) {
            EnsureAdditionalCapacity(9, 24);
            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            float width = size.width;
            float height = size.height;

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

            triangleList.size += 24;
            positionList.size += 9;
            texCoordList0.size += 9;
            texCoordList1.size += 9;
        }

        public void FillRect(float width, float height, in Vector2 position = default) {
            Vector3[] positions = positionList.array;
            Vector4[] texCoord0 = texCoordList0.array;
            int[] triangles = triangleList.array;

            int startVert = positionList.size;
            int startTriangle = triangleList.size;

            ref Vector3 p0 = ref positions[startVert + 0];
            ref Vector3 p1 = ref positions[startVert + 1];
            ref Vector3 p2 = ref positions[startVert + 2];
            ref Vector3 p3 = ref positions[startVert + 3];

            ref Vector4 uv0 = ref texCoord0[startVert + 0];
            ref Vector4 uv1 = ref texCoord0[startVert + 1];
            ref Vector4 uv2 = ref texCoord0[startVert + 2];
            ref Vector4 uv3 = ref texCoord0[startVert + 3];

            p0.x = (position.x + 0);
            p0.y = -position.y;
            p0.z = 0;

            p1.x = position.x + width;
            p1.y = -position.y;
            p1.z = 0;

            p2.x = position.x + width;
            p2.y = -(position.y + height);
            p2.z = 0;

            p3.x = position.x;
            p3.y = -(position.y + height);
            p3.z = 0;

//            p0 -= new Vector3(100, -100, 0);
//            p1 -= new Vector3(100, -100, 0);
//            p2 -= new Vector3(100, -100, 0);
//            p3 -= new Vector3(100, -100, 0);

            uv0.x = 0;
            uv0.y = 1;
            uv0.z = 0;
            uv0.w = 1;

            uv1.x = 1;
            uv1.y = 1;
            uv1.z = 1;
            uv1.w = 1;

            uv2.x = 1;
            uv2.y = 0;
            uv2.z = 1;
            uv2.w = 0;

            uv3.x = 0;
            uv3.y = 0;
            uv3.z = 0;
            uv3.w = 0;

            triangles[startTriangle + 0] = startVert + 0;
            triangles[startTriangle + 1] = startVert + 1;
            triangles[startTriangle + 2] = startVert + 2;
            triangles[startTriangle + 3] = startVert + 2;
            triangles[startTriangle + 4] = startVert + 3;
            triangles[startTriangle + 5] = startVert + 0;

            positionList.size += 4;
            texCoordList0.size += 4;
            texCoordList1.size += 4;
            triangleList.size += 6;
        }

        private static readonly Vector3[] s_Xy = new Vector3[4];

        public void FillMeshType(float x, float y, float width, float height, MeshType fillMethod, MeshFillOrigin fillOrigin, float fillAmount, MeshFillDirection fillClockwise) {

            GenerateFilledSprite(new Vector4(x, -height, width, y), fillMethod, (int) fillOrigin, fillAmount, fillClockwise == MeshFillDirection.Clockwise);

            for (int i = 0; i < texCoordList0.size; i++) {
                float uvX = (positionList.array[i].x / width);
                float uvY = 1 - (positionList.array[i].y / -height);
                texCoordList0.array[i] = new Vector4(uvX, uvY, uvX, uvY);
            }

        }

        private void GenerateFilledSprite(Vector4 v, MeshType fillMethod, int fillOrigin, float fillAmount, bool fillClockwise) {

            if (fillAmount < 0.001f) {
                return;
            }

            if (fillAmount >= 1) {
                fillMethod = MeshType.Simple;
            }

            if (fillOrigin > 3) fillOrigin = 0;

            switch (fillMethod) {

                case MeshType.Simple:
                    s_Xy[0] = new Vector2(v.x, v.y);
                    s_Xy[1] = new Vector2(v.x, v.w);
                    s_Xy[2] = new Vector2(v.z, v.w);
                    s_Xy[3] = new Vector2(v.z, v.y);
                    AddQuad(s_Xy);
                    return;

                case MeshType.FillRadial90: {
                    s_Xy[0] = new Vector2(v.x, v.y);
                    s_Xy[1] = new Vector2(v.x, v.w);
                    s_Xy[2] = new Vector2(v.z, v.w);
                    s_Xy[3] = new Vector2(v.z, v.y);
                    RadialCut(s_Xy, fillAmount, fillClockwise, fillOrigin);
                    AddQuad(s_Xy);
                    return;
                }

                case MeshType.FillRadial180: {
                    for (int side = 0; side < 2; side++) {
                        float fx0, fx1, fy0, fy1;
                        int even = fillOrigin > 1 ? 1 : 0;

                        if (fillOrigin == 0 || fillOrigin == 2) {
                            fy0 = 0f;
                            fy1 = 1f;
                            if (side == even) {
                                fx0 = 0f;
                                fx1 = 0.5f;
                            }
                            else {
                                fx0 = 0.5f;
                                fx1 = 1f;
                            }
                        }
                        else {
                            fx0 = 0f;
                            fx1 = 1f;
                            if (side == even) {
                                fy0 = 0.5f;
                                fy1 = 1f;
                            }
                            else {
                                fy0 = 0f;
                                fy1 = 0.5f;
                            }
                        }

                        s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                        s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);

                        s_Xy[1].x = s_Xy[0].x;
                        s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);

                        s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                        s_Xy[2].y = s_Xy[1].y;

                        s_Xy[3].x = s_Xy[2].x;
                        s_Xy[3].y = s_Xy[0].y;

                        float val = fillClockwise ? fillAmount * 2f - side : fillAmount * 2f - (1 - side);

                        RadialCut(s_Xy, Mathf.Clamp01(val), fillClockwise, ((side + fillOrigin + 3) % 4));
                        AddQuad(s_Xy);
                    }

                    return;
                }

                case MeshType.FillRadial360: {
                    for (int corner = 0; corner < 4; ++corner) {
                        float fx0, fx1, fy0, fy1;

                        if (corner < 2) {
                            fx0 = 0f;
                            fx1 = 0.5f;
                        }
                        else {
                            fx0 = 0.5f;
                            fx1 = 1f;
                        }

                        if (corner == 0 || corner == 3) {
                            fy0 = 0f;
                            fy1 = 0.5f;
                        }
                        else {
                            fy0 = 0.5f;
                            fy1 = 1f;
                        }

                        s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                        s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);

                        s_Xy[1].x = s_Xy[0].x;
                        s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);

                        s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                        s_Xy[2].y = s_Xy[1].y;

                        s_Xy[3].x = s_Xy[2].x;
                        s_Xy[3].y = s_Xy[0].y;

                        float val = fillClockwise
                            ? fillAmount * 4f - ((corner + fillOrigin) % 4)
                            : fillAmount * 4f - (3 - ((corner + fillOrigin) % 4));

                        RadialCut(s_Xy, Mathf.Clamp01(val), fillClockwise, ((corner + 2) % 4));
                        AddQuad(s_Xy);
                    }

                    return;
                }

                case MeshType.FillHorizontal: {
                    if (fillOrigin == 1) {
                        v.x = v.z - (v.z - v.x) * fillAmount;
                    }
                    else {
                        v.z = v.x + (v.z - v.x) * fillAmount;
                    }

                    s_Xy[0] = new Vector3(v.x, v.y, 0);
                    s_Xy[1] = new Vector3(v.x, v.w, 0);
                    s_Xy[2] = new Vector3(v.z, v.w, 0);
                    s_Xy[3] = new Vector3(v.z, v.y, 0);
                    AddQuad(s_Xy);
                    return;
                }

                case MeshType.FillVertical: {

                    if (fillOrigin == 1) {
                        v.y = v.w - (v.w - v.y) * fillAmount;
                    }
                    else {
                        v.w = v.y + (v.w - v.y) * fillAmount;
                    }

                    s_Xy[0] = new Vector3(v.x, v.y, 0);
                    s_Xy[1] = new Vector3(v.x, v.w, 0);
                    s_Xy[2] = new Vector3(v.z, v.w, 0);
                    s_Xy[3] = new Vector3(v.z, v.y, 0);
                    AddQuad(s_Xy);
                    return;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(fillMethod), fillMethod, null);
            }

        }

        private unsafe void AddQuad(Vector3[] quadPositions) {
            int startIndex = positionList.size;

            if (positionList.size + 4 >= positionList.array.Length) {
                positionList.EnsureAdditionalCapacity(4);
                texCoordList0.EnsureAdditionalCapacity(4);
                texCoordList1.EnsureAdditionalCapacity(4);
            }

            if (triangleList.size + 6 >= triangleList.array.Length) {
                triangleList.EnsureAdditionalCapacity(6);
            }

            fixed (Vector3* positions = quadPositions)
            fixed (Vector3* ptr = positionList.array) {
                UnsafeUtility.MemCpy(ptr + positionList.size, positions, sizeof(Vector4) * 4);
            }

            positionList.size += 4;
            texCoordList0.size += 4;
            texCoordList1.size += 4;

            int tri = triangleList.size;
            triangleList.array[tri + 0] = startIndex + 0;
            triangleList.array[tri + 1] = startIndex + 1;
            triangleList.array[tri + 2] = startIndex + 2;

            triangleList.array[tri + 3] = startIndex + 2;
            triangleList.array[tri + 4] = startIndex + 3;
            triangleList.array[tri + 5] = startIndex + 0;
            triangleList.size += 6;

        }

        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>
        private static void RadialCut(Vector3[] xy, float fill, bool invert, int corner) {

            // Even corners invert the fill direction
            if ((corner & 1) == 1) invert = !invert;

            // Nothing to adjust
            if (!invert && fill > 0.999f) return;

            // Convert 0-1 value into 0 to 90 degrees angle in radians
            float angle = Mathf.Clamp01(fill);
            if (invert) angle = 1f - angle;
            angle *= 90f * Mathf.Deg2Rad;

            // Calculate the effective X and Y factors
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            RadialCut(xy, cos, sin, invert, corner);

        }

        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>
        static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner) {
            int i0 = corner;
            int i1 = ((corner + 1) % 4);
            int i2 = ((corner + 2) % 4);
            int i3 = ((corner + 3) % 4);

            if ((corner & 1) == 1) {
                if (sin > cos) {
                    cos /= sin;
                    sin = 1f;

                    if (invert) {
                        xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i2].x = xy[i1].x;
                    }
                }
                else if (cos > sin) {
                    sin /= cos;
                    cos = 1f;

                    if (!invert) {
                        xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i3].y = xy[i2].y;
                    }
                }
                else {
                    cos = 1f;
                    sin = 1f;
                }

                if (!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
            }
            else {
                if (cos > sin) {
                    sin /= cos;
                    cos = 1f;

                    if (!invert) {
                        xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i2].y = xy[i1].y;
                    }
                }
                else if (sin > cos) {
                    cos /= sin;
                    sin = 1f;

                    if (invert) {
                        xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i3].x = xy[i2].x;
                    }
                }
                else {
                    cos = 1f;
                    sin = 1f;
                }

                if (invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
            }
        }

        public void EnsureCapacity(int vertexCount, int triangleCount) {
            if (positionList.array.Length < vertexCount) {
                Array.Resize(ref positionList.array, vertexCount);
                Array.Resize(ref texCoordList0.array, vertexCount);
                Array.Resize(ref texCoordList1.array, vertexCount);
            }

            if (triangleList.array.Length < triangleCount) {
                Array.Resize(ref triangleList.array, triangleCount);
            }
        }

        public void ToMesh(PooledMesh mesh) {
            mesh.mesh.Clear();
            mesh.SetVertices(positionList.array, positionList.size);
            mesh.SetTextureCoord0(texCoordList0.array, texCoordList0.size);
            mesh.SetTextureCoord1(texCoordList1.array, texCoordList1.size);
            mesh.SetTriangles(triangleList.array, triangleList.size);
        }

    }

}