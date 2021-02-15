using System;
using System.Threading.Tasks;
using UnityEngine;
using Fragsurf.Utility;
using Fragsurf.Actors;

namespace Fragsurf.Maps
{
    public class MapLoader : SingletonComponent<MapLoader>
    {

        public BaseMap CurrentMap { get; private set; }

        [ConVar("map.default", "")]
        public string DefaultMap { get; set; } = "surf_fst_skyworld";

        private void Awake()
        {
            TimeStep.Instance.OnTick.AddListener(OnTick);
        }

        private void OnTick(float a, float b)
        {
            CurrentMap?.Tick();
        }

        public async Task<MapLoadState> LoadMapAsync(BaseMap map)
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
                return await LoadMapAsync(new PlayTestMap() { Name = "Playtest" });
            }
            // todo: mapName to BaseMap
            return MapLoadState.Failed;
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

        public void GetSpawnPoint(out Vector3 position, out Vector3 angles, int teamNumber = 255)
        {
            position = Vector3.zero;
            angles = Vector3.zero;

            var sps = GameObject.FindObjectsOfType<FSMSpawnPoint>();
            if (sps.Length > 0)
            {
                var rnd = sps[UnityEngine.Random.Range(0, sps.Length)];
                position = rnd.transform.position;
                angles = rnd.transform.eulerAngles;
            }
        }

    }

}
