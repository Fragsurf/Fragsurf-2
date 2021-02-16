using Fragsurf.BSP;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fragsurf.Maps
{
    public class BSPMap : BaseMap
    {

        private BspToUnity _bspToUnity;

        protected override async Task<MapLoadState> _LoadAsync()
        {
            if (!File.Exists(FilePath)
                || !Path.GetExtension(FilePath).Equals(".bsp", StringComparison.OrdinalIgnoreCase))
            {
                return MapLoadState.Failed;
            }

            var ao = SceneManager.LoadSceneAsync("BSPMap");

            while (!ao.isDone)
            {
                await Task.Delay(100);
            }

            var loadBsp = GameObject.FindObjectOfType<LoadBSP>();
            loadBsp.MapDirectory = string.Empty;
            loadBsp.Options.FilePath = FilePath;

            try
            {
                loadBsp.Load();
                _bspToUnity = loadBsp.BspToUnity;
            }
            catch(Exception e)
            {
                loadBsp.BspToUnity?.Dispose();
                Debug.LogError("Failed to load BSP : " + e.Message);
                return MapLoadState.Failed;
            }

            return MapLoadState.Loaded;
        }

        protected override async Task _UnloadAsync()
        {
            _bspToUnity?.Dispose();
            _bspToUnity = null;
            var ao = SceneManager.UnloadSceneAsync("BSPMap");
            while (!ao.isDone)
            {
                await Task.Delay(100);
            }
        }
    }
}

