using Fragsurf.BSP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fragsurf.Maps
{
    public class BSPMap : BaseMap
    {

        public ulong AppId;

        private BspToUnity _bspToUnity;

        public override Texture LoadCoverImage()
        {
            var imgPath = Path.ChangeExtension(FilePath, "jpg");
            if (File.Exists(imgPath))
            {
                var bytes = File.ReadAllBytes(imgPath);
                var tex = new Texture2D(1280, 720);
                if (ImageConversion.LoadImage(tex, bytes))
                {
                    return tex;
                }
            }
            return null;
        }

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
            loadBsp.Options.GamesToMount = AppId != 0 ? new List<string>() { AppId.ToString() }.ToArray() : new string[0];

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
            SceneManager.UnloadScene("BSPMap");
            //var ao = SceneManager.UnloadSceneAsync("BSPMap");
            //while (!ao.isDone)
            //{
            //    await Task.Delay(100);
            //}
        }
    }
}

