using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fragsurf.Utility
{
    public class LineHelper
    {

        public static void DrawLineForOneFrame(Vector3 a, Vector3 b, Color color)
        {
            var obj = GeneratePath(new Vector3[] { a, b }, color);
            obj.GetComponent<LineHelperComponent>().DestroyIn = Mathf.Epsilon;
        }

        public static GameObject GeneratePath(Vector3[] points, Color color, float widthMultiplier = 1f)
        {
            var result = new GameObject("[Generated Outline]");

            var lr = result.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = points.Length;
            lr.SetPositions(points);
            lr.alignment = LineAlignment.View;
            lr.receiveShadows = false;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.material = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/Simple Line"));
            lr.startColor = color;
            lr.endColor = color;
            lr.widthMultiplier = widthMultiplier;
            lr.startWidth = .02f;
            lr.endWidth = .02f;

            result.AddComponent<LineHelperComponent>();

            return result;
        }

        public static GameObject GenerateOutline(MeshFilter mf, Color color, float widthMultiplier = 1f)
        {
            var outlines = mf.gameObject.GetComponentsInChildren<LineHelperComponent>(true);
            foreach(var outline in outlines)
            {
                GameObject.Destroy(outline.gameObject);
            }

            var result = new GameObject("[Generated Outline]");
            var boundary = FindBoundary(/*mf.sharedMesh.normals, mf.sharedMesh.vertices, */GetEdges(mf.sharedMesh));
            boundary = SortEdges(mf.sharedMesh.vertices, boundary);

            var positions = new List<Vector3>();
            foreach(var edge in boundary)
            {
                var v1 = mf.transform.TransformPoint(mf.sharedMesh.vertices[edge.v1]);
                positions.Add(v1);
                positions.Add(v1);
                positions.Add(v1);
            }

            var lr = result.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = positions.Count;
            lr.SetPositions(positions.ToArray());
            lr.alignment = LineAlignment.View;
            lr.receiveShadows = false;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.material = GameObject.Instantiate<Material>(Resources.Load<Material>("Materials/Simple Line"));
            lr.startColor = color;
            lr.endColor = color;
            lr.widthMultiplier = widthMultiplier;
            lr.startWidth = .02f;
            lr.endWidth = .02f;

            result.AddComponent<LineHelperComponent>();
            result.transform.SetParent(mf.transform, true);

            return result;
        }

        public struct Edge
        {
            public int v1;
            public int v2;
            public int triangleIndex;
            public Edge(int aV1, int aV2, int aIndex)
            {
                v1 = aV1;
                v2 = aV2;
                triangleIndex = aIndex;
            }
        }

        public static List<Edge> GetEdges(Mesh mesh)
        {
            List<Edge> result = new List<Edge>();
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                int v1 = mesh.triangles[i];
                int v2 = mesh.triangles[i + 1];
                int v3 = mesh.triangles[i + 2];
                result.Add(new Edge(v1, v2, i));
                result.Add(new Edge(v2, v3, i));
                result.Add(new Edge(v3, v1, i));
            }
            return result;
        }

        public static List<Edge> FindBoundary(/*Vector3[] normals, Vector3[] verts, */List<Edge> aEdges)
        {
            List<Edge> result = new List<Edge>(aEdges);
            //foreach (var edge in aEdges)
            //{
            //    var dir = (verts[edge.v2] - verts[edge.v1]).normalized;
            //    var normal = Vector3.Cross(dir, normals[edge.v2]);
            //    var center = (verts[edge.v1] + verts[edge.v2]) / 2f;
            //    var plane = new Plane(normal, center);
            //    var neg = verts.Count(x => plane.GetSide(x) == false);
            //    if (neg == verts.Length)
            //    {
            //        result.Add(edge);
            //    }
            //}
            for (int i = result.Count - 1; i > 0; i--)
            {
                for (int n = i - 1; n >= 0; n--)
                {
                    if (result[i].v1 == result[n].v2 && result[i].v2 == result[n].v1)
                    {
                        // shared edge so remove both
                        result.RemoveAt(i);
                        result.RemoveAt(n);
                        i--;
                        break;
                    }
                }
            }
            return result;
        }

        public static List<Edge> SortEdges(Vector3[] verts, List<Edge> aEdges)
        {
            List<Edge> result = new List<Edge>(aEdges);
            for (int i = 0; i < result.Count - 2; i++)
            {
                Edge E = result[i];
                for (int n = i + 1; n < result.Count; n++)
                {
                    Edge a = result[n];
                    if(verts[E.v2] == verts[a.v1])
                    //if (E.v2 == a.v1)
                    {
                        // in this case they are already in order so just continoue with the next one
                        if (n == i + 1)
                            break;
                        // if we found a match, swap them with the next one after "i"
                        result[n] = result[i + 1];
                        result[i + 1] = a;
                        break;
                    }
                }
            }
            return result;
        }

    }
}

