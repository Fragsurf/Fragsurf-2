using SourceUtils;
using SourceUtils.ValveBsp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fragsurf.BSP
{
	public static class BspUtils
	{
		public static UnityEngine.Vector2 ToUVector(this SourceUtils.Vector2 vec)
		{
			return new UnityEngine.Vector2(vec.X, vec.Y);
		}

		public static UnityEngine.Vector3 ToUVector(this SourceUtils.Vector3 vec)
		{
			return new UnityEngine.Vector3(float.IsNaN(vec.X) ? 0 : vec.X, float.IsNaN(vec.Z) ? 0 : vec.Z, float.IsNaN(vec.Y) ? 0 : vec.Y);
		}

		public static SourceUtils.Vector3 FromUVector(this UnityEngine.Vector3 vec)
		{
			return new SourceUtils.Vector3(vec.x, vec.z, vec.y);
		}

		public static UnityEngine.Vector3 TOUDirection(this SourceUtils.Vector3 directionVector)
        {
			var result = new UnityEngine.Vector3(directionVector.X, directionVector.Y, directionVector.Z);
			var angle = new UnityEngine.Vector3(-result.z, -result.y, -result.x);
			return (Quaternion.Euler(angle) * UnityEngine.Vector3.right);
		}

		public static T ReadAtPosition<T>(this byte[] buffer, int position)
			where T : struct
		{
			int size = Marshal.SizeOf(typeof(T));
			var bytes = new byte[size];

			Array.Copy(buffer, position, bytes, 0, size);
			T stuff;
			GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			try
			{
				stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			}
			finally
			{
				handle.Free();
			}
			return stuff;
		}

		public static void ReverseNormals(this Mesh mesh)
		{
			UnityEngine.Vector3[] normals = mesh.normals;
			for (int i = 0; i < normals.Length; i++)
				normals[i] = -normals[i];
			mesh.normals = normals;

			for (int m = 0; m < mesh.subMeshCount; m++)
			{
				int[] triangles = mesh.GetTriangles(m);
				for (int i = 0; i < triangles.Length; i += 3)
				{
					int temp = triangles[i + 0];
					triangles[i + 0] = triangles[i + 1];
					triangles[i + 1] = temp;
				}
				mesh.SetTriangles(triangles, m);
			}
		}

		public static IEnumerable<int> GetNodeLeafs(ValveBspFile bsp, BspNode node)
		{
			if (node.ChildA.IsLeaf)
			{
				yield return node.ChildA.Index;
			}
			else
			{
				foreach (var leaf in GetNodeLeafs(bsp, bsp.Nodes[node.ChildA.Index]))
				{
					yield return leaf;
				}
			}

			if (node.ChildB.IsLeaf)
			{
				yield return node.ChildB.Index;
			}
			else
			{
				foreach (var leaf in GetNodeLeafs(bsp, bsp.Nodes[node.ChildB.Index]))
				{
					yield return leaf;
				}
			}
		}

		//public static UnityEngine.Vector3[] GetVerticesFromPlanesJob(SourceUtils.ValveBsp.Plane[] planes)
		//{
		//    var input = new NativeArray<BurstPlane>(planes.Length, Allocator.Persistent);
		//    var output = new NativeList<UnityEngine.Vector3>(Allocator.Persistent);

		//    for (int i = 0; i < planes.Length; i++)
		//    {
		//        input[i] = new BurstPlane()
		//        {
		//            Dist = planes[i].Dist,
		//            X = planes[i].Normal.X,
		//            Y = planes[i].Normal.Y,
		//            Z = planes[i].Normal.Z
		//        };
		//    }

		//    var job = new VertsFromPlanesJob()
		//    {
		//        Input = input,
		//        Output = output
		//    };

		//    job.Schedule().Complete();

		//    var result = new UnityEngine.Vector3[job.Output.Length];
		//    for(int i = 0; i < job.Output.Length; i++)
		//    {
		//        result[i] = job.Output[i];
		//    }

		//    job.Input.Dispose();
		//    job.Output.Dispose();

		//    return result;
		//}

		//public struct BurstPlane
		//{
		//    public float Dist;
		//    public float X;
		//    public float Y;
		//    public float Z;
		//}

		//[BurstCompile(CompileSynchronously = true)]
		//public struct VertsFromPlanesJob : IJob
		//{
		//    [ReadOnly]
		//    public NativeArray<BurstPlane> Input;
		//    public NativeList<UnityEngine.Vector3> Output;

		//    public void Execute()
		//    {
		//        var numPlanes = Input.Length;

		//        for (var i = 0; i < numPlanes; i++)
		//        {
		//            var N1 = Input[i];

		//            for (var j = i + 1; j < numPlanes; j++)
		//            {
		//                var N2 = Input[j];

		//                for (var k = j + 1; k < numPlanes; k++)
		//                {
		//                    var N3 = Input[k];

		//                    //var n2n3 = N2.Normal.Cross(N3.Normal);
		//                    //var n3n1 = N3.Normal.Cross(N1.Normal);
		//                    //var n1n2 = N1.Normal.Cross(N2.Normal);
		//                    var n2n3 = new SourceUtils.Vector3(N2.Y * N3.Z - N2.Z * N3.Y, N2.Z * N3.X - N2.X * N3.Z, N2.X * N3.Y - N2.Y * N3.X);
		//                    var n3n1 = new SourceUtils.Vector3(N3.Y * N1.Z - N3.Z * N1.Y, N3.Z * N1.X - N3.X * N1.Z, N3.X * N1.Y - N3.Y * N1.X);
		//                    var n1n2 = new SourceUtils.Vector3(N1.Y * N2.Z - N1.Z * N2.Y, N1.Z * N2.X - N1.X * N2.Z, N1.X * N2.Y - N1.Y * N2.X);

		//                    if ((n2n3.LengthSquared > 0.0001) && (n3n1.LengthSquared > 0.0001) && (n1n2.LengthSquared > 0.0001))
		//                    {
		//                        var quotient = N1.X * n2n3.X + N1.Y * n2n3.Y + N1.Z * n2n3.Z;
		//                        //var quotient = N1.Normal.Dot(n2n3);

		//                        if (Math.Abs(quotient) > double.Epsilon)
		//                        {
		//                            quotient = -1.0f / quotient;
		//                            //n2n3 *= -N1.Dist;
		//                            //n3n1 *= -N2.Dist;
		//                            //n1n2 *= -N3.Dist;

		//                            n2n3.X *= -N1.Dist;
		//                            n2n3.Y *= -N1.Dist;
		//                            n2n3.Z *= -N1.Dist;
		//                            n3n1.X *= -N2.Dist;
		//                            n3n1.Y *= -N2.Dist;
		//                            n3n1.Z *= -N2.Dist;
		//                            n1n2.X *= -N3.Dist;
		//                            n1n2.Y *= -N3.Dist;
		//                            n1n2.Z *= -N3.Dist;

		//                            var potentialVertex = new SourceUtils.Vector3();
		//                            potentialVertex.X = (n2n3.X + n3n1.X + n1n2.X) * quotient;
		//                            potentialVertex.Y = (n2n3.Y + n3n1.Y + n1n2.Y) * quotient;
		//                            potentialVertex.Z = (n2n3.Z + n3n1.Z + n1n2.Z) * quotient;
		//                            //var potentialVertex = n2n3;
		//                            //potentialVertex += n3n1;
		//                            //potentialVertex += n1n2;
		//                            //potentialVertex *= quotient;

		//                            if (IsPointInsidePlanes(Input, potentialVertex, 0.01))
		//                            {
		//                                Output.Add(new UnityEngine.Vector3(potentialVertex.X, potentialVertex.Z, potentialVertex.Y));
		//                            }
		//                        }
		//                    }
		//                }
		//            }
		//        }
		//    }
		//}

		public static IEnumerable<UnityEngine.Vector3> GetVerticesFromPlanes(SourceUtils.ValveBsp.Plane[] planes)
		{
			var result = new List<UnityEngine.Vector3>();
			var planesWrapper = new PlaneWrapper[planes.Length];
			for (int i = 0; i < planes.Length; i++)
			{
				planesWrapper[i] = new PlaneWrapper()
				{
					Normal = planes[i].Normal,
					Dist = planes[i].Dist
				};
			}

			var numPlanes = planesWrapper.Length;

			for (var i = 0; i < numPlanes; i++)
			{
				var N1 = planesWrapper[i];

				for (var j = i + 1; j < numPlanes; j++)
				{
					var N2 = planesWrapper[j];

					for (var k = j + 1; k < numPlanes; k++)
					{
						var N3 = planesWrapper[k];

						//var n2n3 = N2.Normal.Cross(N3.Normal);
						//var n3n1 = N3.Normal.Cross(N1.Normal);
						//var n1n2 = N1.Normal.Cross(N2.Normal);
						var n2n3 = new SourceUtils.Vector3(N2.Normal.Y * N3.Normal.Z - N2.Normal.Z * N3.Normal.Y, N2.Normal.Z * N3.Normal.X - N2.Normal.X * N3.Normal.Z, N2.Normal.X * N3.Normal.Y - N2.Normal.Y * N3.Normal.X);
						var n3n1 = new SourceUtils.Vector3(N3.Normal.Y * N1.Normal.Z - N3.Normal.Z * N1.Normal.Y, N3.Normal.Z * N1.Normal.X - N3.Normal.X * N1.Normal.Z, N3.Normal.X * N1.Normal.Y - N3.Normal.Y * N1.Normal.X);
						var n1n2 = new SourceUtils.Vector3(N1.Normal.Y * N2.Normal.Z - N1.Normal.Z * N2.Normal.Y, N1.Normal.Z * N2.Normal.X - N1.Normal.X * N2.Normal.Z, N1.Normal.X * N2.Normal.Y - N1.Normal.Y * N2.Normal.X);

						if ((n2n3.LengthSquared > 0.0001) && (n3n1.LengthSquared > 0.0001) && (n1n2.LengthSquared > 0.0001))
						{
							var quotient = N1.Normal.X * n2n3.X + N1.Normal.Y * n2n3.Y + N1.Normal.Z * n2n3.Z;
							//var quotient = N1.Normal.Dot(n2n3);

							if (Math.Abs(quotient) > double.Epsilon)
							{
								quotient = -1.0f / quotient;
								//n2n3 *= -N1.Dist;
								//n3n1 *= -N2.Dist;
								//n1n2 *= -N3.Dist;

								n2n3.X *= -N1.Dist;
								n2n3.Y *= -N1.Dist;
								n2n3.Z *= -N1.Dist;
								n3n1.X *= -N2.Dist;
								n3n1.Y *= -N2.Dist;
								n3n1.Z *= -N2.Dist;
								n1n2.X *= -N3.Dist;
								n1n2.Y *= -N3.Dist;
								n1n2.Z *= -N3.Dist;

								var potentialVertex = new SourceUtils.Vector3();
								potentialVertex.X = (n2n3.X + n3n1.X + n1n2.X) * quotient;
								potentialVertex.Y = (n2n3.Y + n3n1.Y + n1n2.Y) * quotient;
								potentialVertex.Z = (n2n3.Z + n3n1.Z + n1n2.Z) * quotient;
								//var potentialVertex = n2n3;
								//potentialVertex += n3n1;
								//potentialVertex += n1n2;
								//potentialVertex *= quotient;

								if (IsPointInsidePlanes(planesWrapper, potentialVertex, 0.01))
								{
									result.Add(new UnityEngine.Vector3(potentialVertex.X, potentialVertex.Z, potentialVertex.Y));
								}
							}
						}
					}
				}
			}

			return result;
		}

		public class PlaneWrapper
		{
			public SourceUtils.Vector3 Normal;
			public float Dist;
		}

		//public static bool IsPointInsidePlanes(NativeArray<BurstPlane> planes, SourceUtils.Vector3 point, double margin)
		//{
		//    for(int i = 0; i < planes.Length; i++)
		//    {
		//        var plane = planes[i];
		//        var dist = (plane.X * point.X + plane.Y * point.Y + plane.Z * point.Z) + (-plane.Dist) - margin;

		//        if (dist > 0)
		//        {
		//            return false;
		//        }
		//    }

		//    return true;
		//}

		public static bool IsPointInsidePlanes(PlaneWrapper[] planeEquations, SourceUtils.Vector3 point, double margin)
		{
			foreach (var plane in planeEquations)
			{
				var dist = (plane.Normal.X * point.X + plane.Normal.Y * point.Y + plane.Normal.Z * point.Z) + (-plane.Dist) - margin;
				//var dist = plane.Normal.Dot(point) + (-plane.Distance) - margin;

				if (dist > 0)
				{
					return false;
				}
			}

			return true;
		}

		public static Texture2D FlipTextureY(Texture2D original)
		{
			Texture2D flipped = new Texture2D(original.width, original.height);

			int xN = original.width;
			int yN = original.height;

			for (int i = 0; i < xN; i++)
			{
				for (int j = 0; j < yN; j++)
				{
					flipped.SetPixel(i, yN - j - 1, original.GetPixel(i, j));
				}
			}

			flipped.Apply();

			return flipped;
		}

		public static Texture2D FlipTextureX(Texture2D original)
		{
			Texture2D flipped = new Texture2D(original.width, original.height);

			int xN = original.width;
			int yN = original.height;

			for (int i = 0; i < xN; i++)
			{
				for (int j = 0; j < yN; j++)
				{
					flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
				}
			}

			flipped.Apply();

			return flipped;
		}

		public enum Rotation { Left, Right, HalfCircle }
		public static Texture2D Rotate(this Texture2D texture, Rotation rotation)
		{
			UnityEngine.Color32[] originalPixels = texture.GetPixels32();
			IEnumerable<UnityEngine.Color32> rotatedPixels;

			if (rotation == Rotation.HalfCircle)
				rotatedPixels = originalPixels.Reverse();
			else
			{
				// Rotate left:
				var firstRowPixelIndeces = Enumerable.Range(0, texture.height).Select(i => i * texture.width).Reverse().ToArray();
				rotatedPixels = Enumerable.Repeat(firstRowPixelIndeces, texture.width).SelectMany(
					(frpi, rowIndex) => frpi.Select(i => originalPixels[i + rowIndex])
				);

				if (rotation == Rotation.Right)
					rotatedPixels = rotatedPixels.Reverse();
			}

			var result = new Texture2D(texture.width, texture.height);
			result.SetPixels32(rotatedPixels.ToArray());
			result.Apply();
			return result;
		}
	}
}