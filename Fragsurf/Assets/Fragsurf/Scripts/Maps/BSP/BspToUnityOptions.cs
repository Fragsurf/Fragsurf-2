using System;
using System.Linq;
using UnityEngine;

namespace Fragsurf.BSP
{
	[Serializable]
	public class BspShaderToMaterial
    {
		public string ShaderName;
		public Material Material;
    }
	[Serializable]
	public class BspToUnityOptions
	{
		[Header("Import Options")]
		public string FilePath;
		public float WorldScale = .0254f;
		public bool WithModels = true;
		public bool WithDisplacements = true;
		public bool WithLightmaps = true;
		public bool WithConvexColliders = true;
		public bool WithEntities = true;
		public bool WithSkybox = true;
		public bool WithProps = true;
		public string[] GamesToMount;

		[Header("Generate Options")]
		public Material LightmappedGeneric;
		public Material LightmappedGenericTransparent;
		public Material VertexLitGeneric;
		public Material VertexLitGenericTransparent;
		public Material Unlit;
		public Material WaterMaterial;
		public Material MissingMaterial;
		public Material SkyMaterial;

		[Header("Entity Options")]
		public Light Sun;
	}
}