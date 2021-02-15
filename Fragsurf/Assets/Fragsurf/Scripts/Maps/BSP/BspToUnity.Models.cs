using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SourceUtils;

using UVector3 = UnityEngine.Vector3;
using UVector2 = UnityEngine.Vector2;
using SVector2 = SourceUtils.Vector2;
using SVector3 = SourceUtils.Vector3;
using SourceUtils.ValveBsp;
using System;

namespace Fragsurf.BSP
{
	public partial class BspToUnity
	{

		public Dictionary<GameObject, string> SurfaceMaterials = new Dictionary<GameObject, string>();

		private void GenerateModels(int start, int count)
		{
			var modelsObject = CreateGameObject("[Models]");
			_models = new Dictionary<int, GameObject>();

			var batchMap = BuildBatchMap(start, count);

			foreach (KeyValuePair<int, List<ModelBatch>> kvp in batchMap)
			{
				var modelRoot = CreateGameObject("*" + kvp.Key.ToString(), modelsObject);
				foreach (ModelBatch batch in kvp.Value)
				{
					var obj = BatchToObject(batch);
					obj.transform.SetParent(modelRoot.transform);
				}
				_models.Add(kvp.Key, modelRoot);
			}
		}

		private GameObject BatchToObject(ModelBatch batch)
		{
			var verts = new UVector3[batch.VertexCount];
			var normals = new UVector3[batch.VertexCount];
			var tris = new int[batch.TriCount];
			var uv = new UVector2[batch.VertexCount];
			var uv2 = new UVector2[batch.VertexCount];
			var vertOffset = 0;
			var triOffset = 0;

			foreach (int faceIndex in batch.FaceIds)
			{
				var face = _bsp.Faces[faceIndex];
				var ti = _bsp.TextureInfos[face.TexInfo];

				if (ti.Flags.HasFlag(SurfFlags.NODRAW)
					|| ti.Flags.HasFlag(SurfFlags.SKIP)
					|| face.DispInfo != -1)
				{
					continue;
				}

				BuildFace(faceIndex, ref verts, ref tris, ref uv, ref uv2, ref normals, vertOffset, triOffset);

				vertOffset += face.NumEdges;
				triOffset += (face.NumEdges - 2) * 3;
			}

			var dataId = _bsp.TextureData[batch.Ti.TexData].NameStringTableId;
			var materialPath = $"materials/{_bsp.GetTextureString(dataId)}.vmt";
			var obj = CreateGameObject(materialPath);
			var mr = obj.AddComponent<MeshRenderer>();
			var mf = obj.AddComponent<MeshFilter>();
			mf.mesh = new Mesh();
			mf.mesh.vertices = verts;
			mf.mesh.triangles = tris;
			mf.mesh.normals = normals;
			mf.mesh.uv = uv;
			mf.mesh.uv2 = uv2;
			//mf.mesh.RecalculateNormals();
			//mf.mesh.RecalculateTangents();
			mf.mesh.RecalculateBounds();

			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.receiveShadows = true;
			mr.lightmapIndex = _currentLightmap;

			var vmtInfo = ApplyMaterial(mr, materialPath);
			if(vmtInfo != null)
            {
				CreateSurfacePropIdentifier(obj, vmtInfo.SurfaceProp);
            }

			return obj;
		}

		private void BuildFace(int faceIndex, ref UVector3[] verts, ref int[] tris, ref UVector2[] uv, ref UVector2[] uv2, ref UVector3[] normals, int vertOffset, int triOffset)
		{
			Face face = _bsp.Faces[faceIndex];
			var plane = _bsp.Planes[face.PlaneNum];
			int startEdge = face.FirstEdge;
			int nEdges = face.NumEdges;

			for (int i = 0; i < nEdges; i++)
			{
				verts[vertOffset + i] = _bsp.GetVertexFromSurfEdgeId(startEdge + i).ToUVector();
				normals[vertOffset + i] = plane.Normal.ToUVector();
			}

			int j = triOffset;
			for (int i = 0; i < nEdges - 2; i++)
			{
				tris[j] = vertOffset;
				tris[j + 1] = vertOffset + i + 1;
				tris[j + 2] = vertOffset + i + 2;
				j += 3;
			}

			var texInfo = _bsp.TextureInfos[face.TexInfo];
			var scaleU = _bsp.TextureData[texInfo.TexData].Width;
			var scaleV = _bsp.TextureData[texInfo.TexData].Height;

			for (int i = 0; i < nEdges; i++)
			{
				var uNormal = texInfo.TextureUAxis.Normal.ToUVector();
				var vNormal = texInfo.TextureVAxis.Normal.ToUVector();
				var tU = UVector3.Dot(verts[vertOffset + i], uNormal) + texInfo.TextureUAxis.Offset;
				var tV = UVector3.Dot(verts[vertOffset + i], vNormal) + texInfo.TextureVAxis.Offset;
				uv[vertOffset + i] = new UVector2(tU / scaleU, tV / scaleV);
			}

			LightmapFace(faceIndex, vertOffset, ref verts, ref uv2);

			for (int i = 0; i < nEdges; i++)
			{
				verts[vertOffset + i] *= Options.WorldScale;
			}
		}

		private void LightmapFace(int faceIndex, int vertOffset, ref UVector3[] verts, ref UVector2[] uv2)
		{
			var face = _bsp.Faces[faceIndex];
			var texInfo = _bsp.TextureInfos[face.TexInfo];

			_bsp.LightmapLayout.GetUvs(faceIndex, out SVector2 lmMin, out SVector2 lmSize);

			// uv2 (lightmap uvs)
			for (int i = 0; i < face.NumEdges; i++)
			{
				var suv2 = GetUv(verts[vertOffset + i].FromUVector(), texInfo.LightmapUAxis, texInfo.LightmapVAxis);

				suv2.X -= face.LightMapOffsetX;
				suv2.Y -= face.LightMapOffsetY;
				suv2.X /= Math.Max(face.LightMapSizeX, 1);
				suv2.Y /= Math.Max(face.LightMapSizeY, 1);

				suv2 *= lmSize;
				suv2 += lmMin;

				uv2[vertOffset + i] = new UVector2(suv2.X, suv2.Y);
			}
		}

		private SVector2 GetUv(SVector3 pos, TexAxis uAxis, TexAxis vAxis)
		{
			return new SVector2(
				pos.Dot(uAxis.Normal) + uAxis.Offset,
				pos.Dot(vAxis.Normal) + vAxis.Offset);
		}

		private Dictionary<int, List<ModelBatch>> BuildBatchMap(int start, int count)
		{
			var result = new Dictionary<int, List<ModelBatch>>();
			for (int i = start; i < count; i++)
			{
				var batches = BuildBatches(i);
				result.Add(i, batches);
			}
			return result;
		}

		private List<ModelBatch> BuildBatches(int modelIndex)
		{
			var model = _bsp.Models[modelIndex];
			var batches = new List<ModelBatch>();
			var firstFace = model.FirstFace;
			var numFaces = model.NumFaces;

			// WorldSpawn is static geometry, so batch it together by material
			for (int i = firstFace; i < firstFace + numFaces; i++)
			{
				var face = _bsp.Faces[i];
				var ti = _bsp.TextureInfos[face.TexInfo];

				ModelBatch batch = batches.FindLast(b => b.Ti.TexData == ti.TexData);
				if (batch == null)
				{
					batch = new ModelBatch();
					batches.Add(batch);
				}

				batch.FaceIds.Add(i);
				batch.Ti = ti;
				batch.VertexCount += face.NumEdges;
				batch.TriCount += (face.NumEdges - 2) * 3;
			}

			return batches;
		}

		private void CreateSurfacePropIdentifier(GameObject obj, string surfaceProp)
        {
			if(obj.TryGetComponent(out MeshFilter mf))
            {
				var colliderObj = new GameObject();
				colliderObj.transform.SetParent(obj.transform);
				colliderObj.transform.localScale = UVector3.one;
				colliderObj.transform.localPosition = UVector3.zero;
				colliderObj.layer = LayerMask.NameToLayer("TransparentFX");
				colliderObj.AddComponent<MeshFilter>().sharedMesh = mf.mesh;
				colliderObj.AddComponent<MeshCollider>();
				SurfaceMaterials.Add(colliderObj, surfaceProp);
			}
		}

		private class ModelBatch
		{
			public List<int> FaceIds = new List<int>();
			public TextureInfo Ti;
			public int VertexCount;
			public int TriCount;
			public int test;
		}

	}
}