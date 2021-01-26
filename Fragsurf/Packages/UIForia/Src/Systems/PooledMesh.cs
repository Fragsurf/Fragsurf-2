using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering.Vertigo {

    public class PooledMesh {

        internal bool isActive;

        public readonly Mesh mesh;
        public readonly MeshPool origin;

        private static readonly List<Vector3> s_ScratchVector3 = new List<Vector3>(0);
        private static readonly List<Vector4> s_ScratchVector4 = new List<Vector4>(0);
        private static readonly List<Color> s_ScratchColor = new List<Color>(0);
        private static readonly List<int> s_ScratchInt = new List<int>(0);

        public PooledMesh(MeshPool origin) {
            this.origin = origin;
            this.mesh = new Mesh();
            this.isActive = false;
            this.mesh.MarkDynamic();
        }

        public void Release() {
            origin?.Release(this);
        }

        public void SetVertices(Vector3[] positions, int count) {
            ListAccessor<Vector3>.SetArray(s_ScratchVector3, positions, count);
            mesh.SetVertices(s_ScratchVector3);
        }

        public void SetColors(Color[] colors, int count) {
            ListAccessor<Color>.SetArray(s_ScratchColor, colors, count);
            mesh.SetColors(s_ScratchColor);
        }

        public void SetNormals(Vector3[] normals, int count) {
            ListAccessor<Vector3>.SetArray(s_ScratchVector3, normals, count);
            mesh.SetNormals(s_ScratchVector3);
        }

        public void SetTextureCoord0(Vector4[] texCoords, int count) {
            ListAccessor<Vector4>.SetArray(s_ScratchVector4, texCoords, count);
            mesh.SetUVs(0, s_ScratchVector4);
        }

        public void SetTextureCoord1(Vector4[] texCoords, int count) {
            ListAccessor<Vector4>.SetArray(s_ScratchVector4, texCoords, count);
            mesh.SetUVs(1, s_ScratchVector4);
        }

        public void SetTextureCoord2(Vector4[] texCoords, int count) {
            ListAccessor<Vector4>.SetArray(s_ScratchVector4, texCoords, count);
            mesh.SetUVs(2, s_ScratchVector4);
        }

        public void SetTextureCoord3(Vector4[] texCoords, int count) {
            ListAccessor<Vector4>.SetArray(s_ScratchVector4, texCoords, count);
            mesh.SetUVs(3, s_ScratchVector4);
        }

        public void SetTriangles(int[] triangles, int count) {
            ListAccessor<int>.SetArray(s_ScratchInt, triangles, count);
            mesh.SetTriangles(s_ScratchInt, 0);
        }

    }

    public class MeshPool {

        private readonly LightList<PooledMesh> dynamicPool;

        public MeshPool() {
            this.dynamicPool = new LightList<PooledMesh>();
        }

        public PooledMesh Get() {
            PooledMesh retn = null;
            if (dynamicPool.Count > 0) {
                retn = dynamicPool.RemoveLast();
            }
            else {
                retn = new PooledMesh(this);
            }

            retn.isActive = true;
            return retn;
        }

        public void Release(PooledMesh mesh) {
            if (mesh == null) return;
            if (mesh.isActive) {
                if (mesh.mesh == null) {
                    return;
                }
                mesh.mesh.Clear(true);
                dynamicPool.Add(mesh);
                mesh.isActive = false;
            }
        }

        public void Destroy() {
            for (int i = 0; i < dynamicPool.Count; i++) {
                Object.Destroy(dynamicPool[i].mesh);
            }

            dynamicPool.Clear();
        }

    }

}