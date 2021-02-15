using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fragsurf.Utility;

namespace Fragsurf.Maps
{
    public class MapLoader : SingletonComponent<MapLoader>
    {

        public IFragsurfMap CurrentMap { get; private set; }
        public MapLoadState State { get; private set; }

        [ConVar("map.default", "")]
        public string DefaultMap { get; set; } = "surf_fst_skyworld";

        private void Awake()
        {
            TimeStep.Instance.OnTick.AddListener((a, b) =>
            {
                if(CurrentMap != null && CurrentMap.State == MapLoadState.Loaded)
                {
                    CurrentMap.Tick();
                }
            });
        }

        private bool SceneExists(string name)
        {
            var sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                var sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                if(string.Equals(name, sceneName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        //private IFragsurfMap GetMapLoader(string path)
        //{
        //    if(path == "LoadActiveScene")
        //    {
        //        return new PlayTestMap();
        //    }

        //    FSFileInfo fileInfo = null;

        //    if (!Path.HasExtension(path) && !ulong.TryParse(path, out ulong workshopId))
        //    {
        //        foreach (var ext in _validExtensions)
        //        {
        //            fileInfo = FileSystem.GetOrAcquire($"{path}.{ext}", true);
        //            if (fileInfo != null)
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        fileInfo = FileSystem.GetOrAcquire(path, true);
        //    }

        //    if(fileInfo == null)
        //    {
        //        return null;
        //    }

        //    switch (fileInfo.Extension)
        //    {
        //        default:
        //            return null;
        //    }
        //}

        public async Task<MapLoadState> LoadMapAsync(IFragsurfMap map)
        {
            if (CurrentMap != null)
            {
                await UnloadMapAsync();
            }

            var result = await map.LoadAsync();
            if (result == MapLoadState.Loaded)
            {
                CurrentMap = map;
                GC.Collect(2, GCCollectionMode.Forced);
            }

            return result;
        }

        public async Task<MapLoadState> LoadMapAsync(string mapName)
        {
            if (mapName == "LoadActiveScene")
            {
                return await LoadMapAsync(new PlayTestMap());
            }
            // todo: mapName to MapData
            // todo: MapData.GetFragsurfMap
            return await LoadMapAsync(new MapData().GetFragsurfMap());
        }

        public async Task UnloadMapAsync()
        {
            if(CurrentMap == null)
            {
                Debug.LogError("Trying to unload map when one isn't loaded");
                return;
            }

            await CurrentMap.UnloadAsync();
            CurrentMap = null;

            Resources.UnloadUnusedAssets();
            GC.Collect(2, GCCollectionMode.Forced);
        }

    }

}
