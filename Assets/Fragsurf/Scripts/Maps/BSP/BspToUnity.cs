using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SourceUtils;
using SurfaceConfigurator;

namespace Fragsurf.BSP
{
	public partial class BspToUnity : IDisposable
	{

        private ResourceLoader _resourceLoader;
		private ValveBspFile _bsp;
		private GameObject _rootObject;
		private Dictionary<int, GameObject> _models;
		private int _currentLightmap = 0;

		public BspToUnityOptions Options { get; }

		public BspToUnity(BspToUnityOptions options) 
		{
			if (options.FilePath.StartsWith("sa://", StringComparison.InvariantCultureIgnoreCase))
			{
				options.FilePath = Path.Combine(Application.streamingAssetsPath, options.FilePath.Remove(0, 5));
			}
			if (!options.FilePath.EndsWith(".bsp"))
			{
				options.FilePath = options.FilePath + ".bsp";
			}
			Options = options;
			_bsp = new ValveBspFile(options.FilePath);
		}

        public void Dispose()
        {
			_bsp?.Dispose();
        }

        public GameObject Generate()
		{
			_rootObject = new GameObject(_bsp.Name);
			_resourceLoader = new ResourceLoader();
			_resourceLoader.AddResourceProvider(_bsp.PakFile);

            try
            {
				foreach (var appid in Options.GamesToMount)
				{
					if (SourceMounter.Mount(appid))
					{
						foreach (var vpk in SourceMounter.MountedContent[appid])
						{
							_resourceLoader.AddResourceProvider(vpk);
						}
					}
				}
			}
			catch(Exception e)
            {
				Debug.LogError(e);
            }

   //         try
   //         {
			//	var path = Application.streamingAssetsPath + "/Pack/cstrike_pak_dir.vpk";
			//	var vpk = new ValvePackage(path);
			//	_resourceLoader.AddResourceProvider(vpk);
			//}
			//catch(Exception e) { }

			var t = new System.Diagnostics.Stopwatch();
			t.Start();

			if (Options.WithModels)
			{
				try
				{
					GenerateModels(0, _bsp.Models.Count());
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
				Debug.Log("Models: " + t.ElapsedMilliseconds + "ms");
				t.Restart();
			}

			if (Options.WithDisplacements)
			{
				try
				{
					GenerateDisplacements();
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
				Debug.Log("Displacements: " + t.ElapsedMilliseconds + "ms");
				t.Restart();
			}

			if (Options.WithLightmaps)
			{
				try
				{
					GenerateLightmapPixels();
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
				Debug.Log("Lightmaps: " + t.ElapsedMilliseconds + "ms");
				t.Restart();
			}

			if (Options.WithConvexColliders)
			{
				try
				{
					GeneratePhysModels();
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
				Debug.Log("Colliders: " + t.ElapsedMilliseconds + "ms");
				t.Restart();
			}
			else
			{
				// add mesh colliders
			}

			if (Options.WithEntities)
			{
				try
				{
					GenerateEntities();
				}
				catch(Exception e)
                {
					Debug.LogError(e);
                }
				Debug.Log("Entities: " + t.ElapsedMilliseconds + "ms");
				t.Restart();
			}

            if (Options.WithSkybox)
            {
                try
                {
					GenerateSkybox();
				}
				catch(Exception e)
                {
					Debug.LogError(e);
                }
				Debug.Log("Skybox: " + t.ElapsedMilliseconds + "ms");
				t.Restart();
			}

            if (Options.WithProps)
            {
				try
				{
					GeneratePropModels();
					GenerateStaticProps();
				}
				catch(Exception e)
                {
					Debug.LogError(e);
                }
				Debug.Log("Props: " + t.ElapsedMilliseconds + "ms");
				t.Restart();
			}

			foreach(var kvp in SurfaceMaterials)
            {
				if(!Enum.TryParse(kvp.Value, true, out SurfaceType surfaceType))
                {
					surfaceType = UnsupportedSurfaceType(kvp.Value);
                }
				kvp.Key.AddComponent<SurfaceTypeIdentifier>().SurfaceType = surfaceType;
            }

			Debug.Log("SurfaceMats: " + t.ElapsedMilliseconds + "ms");
			t.Restart();

			return _rootObject;
		}

		public SurfaceType UnsupportedSurfaceType(string surf)
        {
            switch (surf.ToLower())
            {
				case "paper":
				case "plaster":
				case "cardboard":
					return SurfaceType.Carpet;
				case "rock":
				case "boulder":
				case "stone":
				case "brick":
					return SurfaceType.Concrete;
				case "wood_plank":
					return SurfaceType.Wood;
				case "metalpanel":
					return SurfaceType.Metal;
			}
			return SurfaceType.Concrete;
        }

		private GameObject CreateGameObject(string name = null, GameObject parent = null)
		{
			var obj = new GameObject();
			obj.name = name ?? obj.name;
			obj.transform.SetParent(parent ? parent.transform : _rootObject.transform);
			obj.transform.localPosition = UnityEngine.Vector3.zero;
			obj.transform.localScale = UnityEngine.Vector3.one;
			return obj;
		}

	}
}