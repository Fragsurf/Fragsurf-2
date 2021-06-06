using System;
using System.Collections.Generic;
using UnityEngine;
using UVector3 = UnityEngine.Vector3;
using UVector2 = UnityEngine.Vector2;
using SVector2 = SourceUtils.Vector2;
using SVector3 = SourceUtils.Vector3;
using SourceUtils.ValveBsp;
using Fragsurf.Movement;

namespace Fragsurf.BSP
{
	public partial class BspToUnity
	{

		private void GenerateDisplacements()
		{
			var displacementContainer = CreateGameObject("[Displacements]");

			for (int m = 0; m < _bsp.Models.Length; m++)
			{
				var model = _bsp.Models[m];
				for (int i = model.FirstFace; i < model.FirstFace + model.NumFaces; i++)
				{
					if (_bsp.Faces[i].DispInfo == -1)
						continue;
					var obj = GenerateDisplacement(i, _bsp.Faces[i].DispInfo, m);
					obj.transform.SetParent(displacementContainer.transform, true);
				}
			}
		}

		private GameObject GenerateDisplacement(int faceIndex, int dispInfoId, int modelId)
		{
			var face = _bsp.Faces[faceIndex];
			var disp_verts = new List<UVector3>();
			var UVs = new List<UVector2>();
			var UV2s = new List<UVector2>();
			var cols = new List<Color>();
			var indices = new List<int>();
			var normals = new List<UVector3>();

			var texInfo = _bsp.TextureInfos[face.TexInfo];
			var texData = _bsp.TextureData[texInfo.TexData];

			_bsp.LightmapLayout.GetUvs(faceIndex, out SVector2 lmMin, out SVector2 lmSize);

			var disp = _bsp.DisplacementManager[dispInfoId];
			disp.GetCorners(out SVector3 c0, out SVector3 c1, out SVector3 c2, out SVector3 c3);

			var texScale = new SVector2(1f / Math.Max(texData.Width, 1), 1f / Math.Max(texData.Height, 1));

			var uv00 = GetUv(c0, texInfo.TextureUAxis, texInfo.TextureVAxis) * texScale;
			var uv10 = GetUv(c3, texInfo.TextureUAxis, texInfo.TextureVAxis) * texScale;
			var uv01 = GetUv(c1, texInfo.TextureUAxis, texInfo.TextureVAxis) * texScale;
			var uv11 = GetUv(c2, texInfo.TextureUAxis, texInfo.TextureVAxis) * texScale;

			var subDivMul = 1f / disp.Subdivisions;
			for (var y = 0; y < disp.Subdivisions; ++y)
			{
				var v0 = (y + 0) * subDivMul;
				var v1 = (y + 1) * subDivMul;

				for (var x = 0; x < disp.Size; ++x)
				{
					var u = x * subDivMul;

					var vert1 = disp.GetPosition(x, y + 0).ToUVector();
					var uv1 = ((uv00 * (1f - u) + uv10 * u) * (1f - v0) + (uv01 * (1f - u) + uv11 * u) * v0).ToUVector();
					var uv21 = (new SVector2(u, v0) * lmSize + lmMin).ToUVector();
					var normal1 = disp.GetNormal(x, y + 0).ToUVector();
					var color1 = new Color(disp.GetAlpha(x, y + 0) / 4f, 0, 0, 0);

					disp_verts.Add(vert1);
					UVs.Add(uv1);
					UV2s.Add(uv21);
					normals.Add(normal1);
					cols.Add(color1);

					var vert2 = disp.GetPosition(x, y + 1).ToUVector();
					var uv2 = ((uv00 * (1f - u) + uv10 * u) * (1f - v1) + (uv01 * (1f - u) + uv11 * u) * v1).ToUVector();
					var uv22 = (new SVector2(u, v1) * lmSize + lmMin).ToUVector();
					var normal2 = disp.GetNormal(x, y + 1).ToUVector();
					var color2 = new Color(disp.GetAlpha(x, y + 1) / 4f, 0, 0, 0);

					disp_verts.Add(vert2);
					UVs.Add(uv2);
					UV2s.Add(uv22);
					normals.Add(normal2);
					cols.Add(color2);

					if (x != disp.Size - 1)
					{
						var j = disp_verts.Count - 2;
						indices.Add(j);
						indices.Add(j + 1);
						indices.Add(j + 2);
						indices.Add(j + 3);
						indices.Add(j + 2);
						indices.Add(j + 1);
					}
				}
			}

			for (int i = 0; i < disp_verts.Count; i++)
			{
				disp_verts[i] *= Options.WorldScale;
			}

			GameObject faceObject = new GameObject("DispFace: " + modelId);

			MeshRenderer mr = faceObject.AddComponent<MeshRenderer>();
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.receiveShadows = true;
			mr.lightmapIndex = _currentLightmap;

			MeshFilter mf = faceObject.AddComponent<MeshFilter>();
			mf.sharedMesh = new Mesh();
			mf.sharedMesh.name = "DispFace " + modelId;
			mf.sharedMesh.vertices = disp_verts.ToArray();
			mf.sharedMesh.triangles = indices.ToArray();
			mf.sharedMesh.uv = UVs.ToArray();
			mf.sharedMesh.uv2 = UV2s.ToArray();
			mf.sharedMesh.normals = normals.ToArray();
			mf.sharedMesh.colors = cols.ToArray();
			mf.sharedMesh.RecalculateTangents();
			mf.sharedMesh.RecalculateBounds();
			// todo: blend textures on vert R value
			//mf.sharedMesh.colors = cols.ToArray();

			if(disp.Solid)
            {
				faceObject.gameObject.AddComponent<MeshCollider>();
			}

			var texString = _bsp.GetTextureString(texData.NameStringTableId);
			var materialPath = "materials/" + texString + ".vmt";

			var vmtInfo = ApplyMaterial(mr, materialPath);
			if (vmtInfo != null)
			{
				CreateSurfacePropIdentifier(faceObject, vmtInfo.SurfaceProp);
			}

			return faceObject;
		}

	}
}
