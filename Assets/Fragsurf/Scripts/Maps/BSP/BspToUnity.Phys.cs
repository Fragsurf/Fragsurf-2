using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UVector3 = UnityEngine.Vector3;
using UVector2 = UnityEngine.Vector2;
using SVector2 = SourceUtils.Vector2;
using SVector3 = SourceUtils.Vector3;
using SourceUtils;
using System.IO;
using SourceUtils.ValveBsp;

namespace Fragsurf.BSP
{
	public partial class BspToUnity
	{

		public List<MeshCoroutineProcess> CollidersToProcess = new List<MeshCoroutineProcess>();

		public class MeshCoroutineProcess
        {
			public Mesh Mesh;
			public MeshCollider Mc;
			public bool Convex;
			public bool ReverseNormals;
			public bool Trigger;
        }

		private void GeneratePhysModels()
		{
			var lump = _bsp.GetLumpStream(ValveBspFile.LumpType.PHYSCOLLIDE);
			var physModels = new List<PhysModel>();

			using (var br = new BinaryReader(lump))
			{
				while (true)
				{
					var modelIndex = br.ReadInt32();

					if (modelIndex == -1)
						break;

					var dataSize = br.ReadInt32();
					var keyDataSize = br.ReadInt32();
					var solidCount = br.ReadInt32();
					var collisionData = br.ReadBytes(dataSize);
					var keyData = br.ReadBytes(keyDataSize);
					var physModel = new PhysModel(modelIndex, solidCount, collisionData, keyData);
					physModels.Add(physModel);
				}
			}

			foreach (var pm in physModels)
			{
				if (!_models.ContainsKey(pm.ModelIndex))
				{
					Debug.LogWarning("Attempt to load a strat PhysModel");
					continue;
				}

				var modelParent = _models[pm.ModelIndex];
				var solidData = new Dictionary<int, string>();

				// this is wrong
				foreach (var kvp in pm.KeyValues)
				{
                    if (int.TryParse(kvp.Value["index"], out int indx))
                    {
                        solidData.Add(indx, kvp.Key);
                    }
                }

                var physModelContainer = CreateGameObject($"PhysModel #{pm.ModelIndex}", modelParent);
				var solidIdx = -1;

				foreach (var solid in pm.Solids)
				{
					solidIdx++;
					// i don't think this was right
					//var contents = GetBrushContents(pm.ModelIndex, solidIdx);
					//var isWater = contents.HasFlag(BrushContents.WATER) || solidData.ContainsKey(solidIdx) && solidData[solidIdx] == "fluid";
					solid.ConvexContainer = CreateGameObject($"Solid #{solidIdx}", physModelContainer);

					var convexIdx = -1;
					foreach (var cc in solid.Convexes)
					{
						convexIdx++;
						if (cc.Skip || cc.Verts.Count < 4)
						{
							if (cc.Verts.Count < 4)
							{
								Debug.Log("SKIPPERINO");
							}
							continue;
						}

						var solidObj = CreateGameObject($"Convex {cc}", solid.ConvexContainer);
						var tris = cc.Triangles;
						var verts = new UVector3[cc.Verts.Count];
						var pivot = new UVector3(cc.Verts[0].x, -cc.Verts[0].y, cc.Verts[0].z);

						for (int i = 0; i < cc.Verts.Count; i++)
						{
							verts[i] = new UVector3(cc.Verts[i].x, -cc.Verts[i].y, cc.Verts[i].z) - pivot;
						}

						var mf = solidObj.AddComponent<MeshFilter>();
						mf.mesh = new Mesh();
						mf.mesh.vertices = verts;
						mf.mesh.triangles = tris.ToArray();
						mf.mesh.ReverseNormals();
						var mc = solidObj.AddComponent<MeshCollider>();
						mc.sharedMesh = mf.mesh;

						var p = new MeshCoroutineProcess()
						{
							Mesh = mf.mesh,
							Convex = true,
							Mc = mc,
							ReverseNormals = true
						};

						if (_bsp.Brushes[cc.BrushIndex].Contents.HasFlag(BrushContents.LADDER))
						{
							mc.gameObject.tag = "Ladder";
						}

						if (_bsp.Brushes[cc.BrushIndex].Contents.HasFlag(BrushContents.WATER))
						{
							mc.gameObject.layer = LayerMask.NameToLayer("Water");
							p.Trigger = true;
						}

						CollidersToProcess.Add(p);

						solidObj.transform.position = pivot;
					}
				}
			}
		}



	}
}