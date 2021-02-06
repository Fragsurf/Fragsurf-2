using UnityEngine;
using SourceUtils;
using System.Collections.Generic;

namespace Fragsurf.BSP
{
    public class SourceStudioModel
    {

        public StudioModelFile Mdl;
        public ValveVertexFile Vvd;
        public ValveTriangleFile Vtx;

        public GameObject GenerateMesh(BspToUnity bspToUnity, string modelName, string vvdPath, string vtxPath, IResourceProvider resourceProvider)
        {
            Mdl = StudioModelFile.FromProvider(modelName, resourceProvider);
            Vvd = ValveVertexFile.FromProvider(vvdPath, resourceProvider);
            Vtx = ValveTriangleFile.FromProvider(vtxPath, Mdl, Vvd, resourceProvider);

            var parent = new GameObject();

            for (int b = 0; b < Mdl.BodyPartCount; b++)
            {
                var models = Mdl.GetModels(b);

                var e = models.GetEnumerator();
                var modelIndex = 0;

                while(e.MoveNext())
                {
                    var model = e.Current;
                    var meshes = Mdl.GetMeshes(ref model);
                    var meshIndex = 0;

                    foreach(var m in meshes)
                    {
                        try
                        {
                            var vertexCount = Vtx.GetVertexCount(b, modelIndex, 0, meshIndex);
                            var sv = new StudioVertex[vertexCount];

                            var indexCount = Vtx.GetIndexCount(b, modelIndex, 0, meshIndex);
                            var indices = new int[indexCount];

                            Vtx.GetVertices(b, modelIndex, 0, meshIndex, sv);
                            Vtx.GetIndices(b, modelIndex, 0, meshIndex, indices);

                            var go = new GameObject();
                            go.transform.SetParent(parent.transform);
                            go.transform.localPosition = UnityEngine.Vector3.zero;
                            go.transform.localRotation = Quaternion.identity;
                            go.transform.localScale = UnityEngine.Vector3.one;

                            var mr = go.AddComponent<MeshRenderer>();
                            var mf = go.AddComponent<MeshFilter>();
                            var modelMesh = new Mesh();

                            var vertices = new List<UnityEngine.Vector3>();
                            var normals = new List<UnityEngine.Vector3>();
                            var uvs = new List<UnityEngine.Vector2>();
                            var tris = new List<int>();

                            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                            mr.receiveShadows = false;

                            // verts, normals, uvs
                            for (int j = 0; j < sv.Length; j++)
                            {
                                vertices.Add(sv[j].Position.ToUVector() * bspToUnity.Options.WorldScale);
                                normals.Add(sv[j].Normal.ToUVector());
                                uvs.Add(new UnityEngine.Vector2(sv[j].TexCoordX, sv[j].TexCoordY));
                            }

                            // tris
                            tris.AddRange(indices);

                            // build mesh
                            modelMesh.SetVertices(vertices);
                            modelMesh.SetUVs(0, uvs);
                            modelMesh.SetNormals(normals);
                            modelMesh.SetTriangles(tris, 0);
                            modelMesh.RecalculateNormals();
                            mf.mesh = modelMesh;

                            var matName = Mdl.GetMaterialName(m.Material, resourceProvider);
                            var mat = bspToUnity.ApplyMaterial(mr, matName);
                            go.name = matName;
                        }
                        catch
                        {
                            Debug.Log("NOPE: " + Vtx.NumLods);
                            continue;
                        }

                        meshIndex++;
                    }

                    modelIndex++;
                }
            }

            return parent;
        }

    }
}

