using UIForia.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace UIForia.Util {

    public static class MeshUtil {

        public static readonly VertexHelper s_VertexHelper = new VertexHelper();
        private static readonly ObjectPool<Mesh> s_MeshPool = new ObjectPool<Mesh>(null, (m) => m.Clear());
        private static readonly Vector2 s_UVEmpty = Vector2.zero;
        private static readonly Vector4 s_TangentEmpty = new Vector4();

        public static Mesh CreateStandardUIMesh(Size size, Color32 color32 = default(Color32)) {
            Mesh mesh = new Mesh();
            mesh.name = "UIForiaStandard";

            Vector2 uv1 = new Vector2();
            Vector4 tangent = new Vector4();

            Vector3 normal0 = new Vector3(0, 0, 0);
            Vector3 normal1 = new Vector3(0, 1, 0);
            Vector3 normal2 = new Vector3(1, 1, 0);
            Vector3 normal3 = new Vector3(1, 0, 0);            

            Vector3 v0 = new Vector3(0, 0);
            Vector3 v1 = new Vector3(0, -size.height);
            Vector3 v2 = new Vector3(size.width, -size.height);
            Vector3 v3 = new Vector3(size.width, 0);
            
            s_VertexHelper.AddVert(v0, color32, new Vector2(0f, 1f), uv1, normal0, tangent);
            s_VertexHelper.AddVert(v1, color32, new Vector2(0f, 0f), uv1, normal1, tangent);
            s_VertexHelper.AddVert(v2, color32, new Vector2(1f, 0f), uv1, normal2, tangent);
            s_VertexHelper.AddVert(v3, color32, new Vector2(1f, 1f), uv1, normal3, tangent);

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.AddTriangle(2, 3, 0);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            return mesh;
        }

        public static Mesh CreateStandardUIMesh(Vector2 offset, Size size, Color32 color32) {
            Mesh mesh = new Mesh();
            mesh.name = "UIForiaStandard";

            s_VertexHelper.AddVert(new Vector3(offset.x + 0, offset.y + 0), color32, new Vector2(0f, 1f));
            s_VertexHelper.AddVert(new Vector3(offset.x + 0, offset.y + -size.height), color32, new Vector2(0f, 0f));
            s_VertexHelper.AddVert(new Vector3(offset.x + size.width, offset.y + -size.height), color32, new Vector2(1f, 0f));
            s_VertexHelper.AddVert(new Vector3(offset.x + size.width, offset.y + 0), color32, new Vector2(1f, 1f));

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.AddTriangle(2, 3, 0);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            return mesh;
        }

        public static Mesh CreateDoubleCutoutMesh(Size size, Rect cutout1, Rect cutout2, Color color0, Color color1, Color color2) {
            Mesh mesh = new Mesh();
            mesh.name = "UIForiaDoubleCutout";

            Vector3 v0 = new Vector3(0, 0, 0);
            Vector3 v1 = new Vector3(0, -size.height, 0);
            Vector3 v2 = new Vector3(size.width, -size.height, 0);
            Vector3 v3 = new Vector3(size.width, 0, 0);

            Vector3 v4 = new Vector3(cutout1.x, -cutout1.y, 0);
            Vector3 v5 = new Vector3(cutout1.x + cutout1.width, -cutout1.y, 0);
            Vector3 v6 = new Vector3(cutout1.x + cutout1.width, -(cutout1.y + cutout1.height), 0);
            Vector3 v7 = new Vector3(cutout1.x, -(cutout1.y + cutout1.height), 0);

            Vector3 v8 = new Vector3(cutout2.x, -cutout2.y, 0);
            Vector3 v9 = new Vector3(cutout2.x + cutout2.width, -cutout2.y, 0);
            Vector3 v10 = new Vector3(cutout2.x + cutout2.width, -(cutout2.y + cutout2.height), 0);
            Vector3 v11 = new Vector3(cutout2.x, -(cutout2.y + cutout2.height), 0);

            s_VertexHelper.AddVert(v0, color0, new Vector2()); // 0
            s_VertexHelper.AddVert(v1, color0, new Vector2()); // 1
            s_VertexHelper.AddVert(v2, color0, new Vector2()); // 2
            s_VertexHelper.AddVert(v3, color0, new Vector2()); // 3

            s_VertexHelper.AddVert(v4, color0, new Vector2()); // 4
            s_VertexHelper.AddVert(v5, color0, new Vector2()); // 5
            s_VertexHelper.AddVert(v6, color0, new Vector2()); // 6
            s_VertexHelper.AddVert(v7, color0, new Vector2()); // 7

            s_VertexHelper.AddVert(v4, color1, new Vector2()); // 8
            s_VertexHelper.AddVert(v5, color1, new Vector2()); // 9
            s_VertexHelper.AddVert(v6, color1, new Vector2()); // 10
            s_VertexHelper.AddVert(v7, color1, new Vector2()); // 11

            s_VertexHelper.AddVert(v8, color1, new Vector2()); // 12
            s_VertexHelper.AddVert(v9, color1, new Vector2()); // 13
            s_VertexHelper.AddVert(v10, color1, new Vector2()); // 14
            s_VertexHelper.AddVert(v11, color1, new Vector2()); // 15

            s_VertexHelper.AddVert(v8, color2, new Vector2()); // 16
            s_VertexHelper.AddVert(v9, color2, new Vector2()); // 17
            s_VertexHelper.AddVert(v10, color2, new Vector2()); // 18
            s_VertexHelper.AddVert(v11, color2, new Vector2()); // 19

            s_VertexHelper.AddTriangle(0, 4, 1);
            s_VertexHelper.AddTriangle(1, 4, 7);
            s_VertexHelper.AddTriangle(1, 7, 2);
            s_VertexHelper.AddTriangle(2, 6, 7);
            s_VertexHelper.AddTriangle(2, 6, 5);
            s_VertexHelper.AddTriangle(5, 3, 2);
            s_VertexHelper.AddTriangle(3, 5, 4);
            s_VertexHelper.AddTriangle(4, 0, 3);

            s_VertexHelper.AddTriangle(8, 12, 9);
            s_VertexHelper.AddTriangle(9, 12, 13);
            s_VertexHelper.AddTriangle(13, 14, 10);
            s_VertexHelper.AddTriangle(10, 9, 13);

            s_VertexHelper.AddTriangle(8, 15, 11);
            s_VertexHelper.AddTriangle(8, 15, 12);

            s_VertexHelper.AddTriangle(15, 10, 14);
            s_VertexHelper.AddTriangle(15, 11, 10);

            s_VertexHelper.AddTriangle(16, 17, 18);
            s_VertexHelper.AddTriangle(18, 19, 16);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            return mesh;
        }

        public static Mesh CreateCutoutMesh(Size size, Rect cutout, Color color) {
            Mesh mesh = new Mesh();
            mesh.name = "UIForiaCutout";

            Vector2 uv1 = new Vector2();
            Vector4 tangent = new Vector4();

            Vector3 normal0 = new Vector3(0, 0, 0);
            Vector3 normal1 = new Vector3(0, 1, 0);
            Vector3 normal2 = new Vector3(1, 1, 0);
            Vector3 normal3 = new Vector3(1, 0, 0);
            Color32 color32 = color;

            Vector3 v0 = new Vector3(0, 0, 0);
            Vector3 v1 = new Vector3(0, -size.height, 0);
            Vector3 v2 = new Vector3(size.width, -size.height, 0);
            Vector3 v3 = new Vector3(size.width, 0, 0);

            Vector3 v4 = new Vector3(cutout.x, -cutout.y, 0);
            Vector3 v5 = new Vector3(cutout.x + cutout.width, -cutout.y, 0);
            Vector3 v6 = new Vector3(cutout.x + cutout.width, -(cutout.y + cutout.height), 0);
            Vector3 v7 = new Vector3(cutout.x, -(cutout.y + cutout.height), 0);

            s_VertexHelper.AddVert(v0, color32, new Vector2(0f, 1f), uv1, normal0, tangent);
            s_VertexHelper.AddVert(v1, color32, new Vector2(0f, 0f), uv1, normal1, tangent);
            s_VertexHelper.AddVert(v2, color32, new Vector2(1f, 0f), uv1, normal2, tangent);
            s_VertexHelper.AddVert(v3, color32, new Vector2(1f, 1f), uv1, normal3, tangent);

            float cutoutXOverWidth = cutout.x / size.width;
            float cutoutYOverHeight = cutout.y / size.height;
            float cutoutWOverWidth = (cutout.x + cutout.width) / size.width;
            float cutoutHOverHeight = (cutout.y + cutout.height) / size.height;

            s_VertexHelper.AddVert(v4, color32, new Vector2(cutoutXOverWidth, cutoutYOverHeight), uv1, normal0, tangent);
            s_VertexHelper.AddVert(v5, color32, new Vector2(cutoutWOverWidth, cutoutYOverHeight), uv1, normal1, tangent);
            s_VertexHelper.AddVert(v6, color32, new Vector2(cutoutWOverWidth, cutoutHOverHeight), uv1, normal2, tangent);
            s_VertexHelper.AddVert(v7, color32, new Vector2(cutoutXOverWidth, cutoutHOverHeight), uv1, normal3, tangent);

            s_VertexHelper.AddTriangle(0, 4, 1);
            s_VertexHelper.AddTriangle(1, 4, 7);
            s_VertexHelper.AddTriangle(1, 7, 2);
            s_VertexHelper.AddTriangle(2, 6, 7);
            s_VertexHelper.AddTriangle(2, 6, 5);
            s_VertexHelper.AddTriangle(5, 3, 2);
            s_VertexHelper.AddTriangle(3, 5, 4);
            s_VertexHelper.AddTriangle(4, 0, 3);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            return mesh;
        }

        public static Mesh CreateTriangle(Size size, float offset = 0) {
            Mesh mesh = new Mesh();
            mesh.name = "UIForiaTriangle";
            
            Vector3 normal0 = new Vector3(-1, 0, 0);
            Vector3 normal1 = new Vector3(0, 1, 0);
            Vector3 normal2 = new Vector3(1, 0, 0);
            Color32 color32 = Color.white;
            offset = Mathf.Clamp01(offset);
            s_VertexHelper.AddVert(new Vector3(size.width * offset, size.height, 0), color32, new Vector2(offset, 1f), s_UVEmpty, normal0, s_TangentEmpty);
            s_VertexHelper.AddVert(new Vector3(0, 0, 0), color32, new Vector2(0f, 0f), s_UVEmpty, normal1, s_TangentEmpty);
            s_VertexHelper.AddVert(new Vector3(size.width, 0, 0), color32, new Vector2(1f, 0f), s_UVEmpty, normal2, s_TangentEmpty);

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();

            return mesh;
        }

        public static Mesh CreateDiamond(Size size) {
            Mesh mesh = new Mesh();
            mesh.name = "UIForiaDiamond";
            
            // top, right, bottom, left

            Vector3 normal0 = new Vector3(0, 1, 0);
            Vector3 normal1 = new Vector3(1, 0, 0);
            Vector3 normal2 = new Vector3(0, -1, 0);
            Vector3 normal3 = new Vector3(-1, 0, 0);
            Color32 color32 = Color.white;

            s_VertexHelper.AddVert(new Vector3(size.width * 0.5f, 0, 0f), color32, new Vector2(0.5f, 0), s_UVEmpty, normal0, s_TangentEmpty);
            s_VertexHelper.AddVert(new Vector3(size.width, size.height * 0.5f, 0), color32, new Vector2(1f, 0.5f), s_UVEmpty, normal1, s_TangentEmpty);
            s_VertexHelper.AddVert(new Vector3(size.width * 0.5f, size.height, 0), color32, new Vector2(0.5f, 1f), s_UVEmpty, normal2, s_TangentEmpty);
            s_VertexHelper.AddVert(new Vector3(0, size.height * 0.5f, 0), color32, new Vector2(0, 0.5f), s_UVEmpty, normal3, s_TangentEmpty);

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.AddTriangle(2, 3, 1);
            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();

            return mesh;
        }

        public static void Release(Mesh mesh) {
            // todo -- free the arrays independently back into the respective array pools
            s_MeshPool.Release(mesh);
        }

        public static Mesh ResizeStandardUIMesh(Mesh mesh, Size size) {
            if (mesh == null) {
                mesh = new Mesh();
                mesh.name = "UIForiaStandard";
            }

            Bounds bounds = mesh.bounds;
            if (bounds.size.x == size.width && bounds.size.y == size.height) {
                return mesh;
            }
            
            Vector2 uv1 = new Vector2();
            Vector4 tangent = new Vector4();

            Vector3 normal0 = new Vector3(0, 0, 0);
            Vector3 normal1 = new Vector3(0, 1, 0);
            Vector3 normal2 = new Vector3(1, 1, 0);
            Vector3 normal3 = new Vector3(1, 0, 0);

            Vector3 v0 = new Vector3(0, 0);
            Vector3 v1 = new Vector3(0, -size.height);
            Vector3 v2 = new Vector3(size.width, -size.height);
            Vector3 v3 = new Vector3(size.width, 0);

            Color32 color32 = Color.white;
            s_VertexHelper.AddVert(v0, color32, new Vector2(0f, 1f), uv1, normal0, tangent);
            s_VertexHelper.AddVert(v1, color32, new Vector2(0f, 0f), uv1, normal1, tangent);
            s_VertexHelper.AddVert(v2, color32, new Vector2(1f, 0f), uv1, normal2, tangent);
            s_VertexHelper.AddVert(v3, color32, new Vector2(1f, 1f), uv1, normal3, tangent);

            s_VertexHelper.AddTriangle(0, 1, 2);
            s_VertexHelper.AddTriangle(2, 3, 0);

            s_VertexHelper.FillMesh(mesh);
            s_VertexHelper.Clear();
            return mesh;
        }

    }

}
