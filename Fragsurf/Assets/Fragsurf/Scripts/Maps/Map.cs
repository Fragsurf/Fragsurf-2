using System;
using System.Threading.Tasks;
using UnityEngine;
using Fragsurf.Utility;
using Fragsurf.Actors;

namespace Fragsurf.Maps
{
    public class Map : SingletonComponent<Map>
    {

        public static BaseMap Current { get; private set; }

        [ConVar("map.default", "")]
        public string DefaultMap { get; set; } = "surf_fst_skyworld";

        private void Awake()
        {
            TimeStep.Instance.OnTick.AddListener(OnTick);
        }

        private void OnTick(float a, float b)
        {
            Current?.Tick();
        }

        public static Task<MapLoadState> LoadAsync(BaseMap map)
        {
            return Instance._LoadAsync(map);
        }

        public static Task<MapLoadState> LoadAsync(string mapName)
        {
            return Instance._LoadAsync(mapName);
        }

        public static async Task UnloadAsync()
        {
            await Instance._UnloadAsync();
        }

        public static void GetSpawnPoint(out Vector3 position, out Vector3 angles, int teamNumber = 255)
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

        private async Task<MapLoadState> _LoadAsync(BaseMap map)
        {
            if (Current != null)
            {
                await UnloadAsync();
            }

            var result = await map.LoadAsync();
            if (result == MapLoadState.Loaded)
            {
                Current = map;
                GC.Collect(2, GCCollectionMode.Forced);
            }

            return result;
        }

        private async Task<MapLoadState> _LoadAsync(string mapName)
        {
            if (mapName == "LoadActiveScene")
            {
                return await LoadAsync(new PlayTestMap() { Name = "Playtest" });
            }
            // todo: mapName to BaseMap
            return MapLoadState.Failed;
        }

        private async Task _UnloadAsync()
        {
            if(Current == null)
            {
                Debug.LogError("Trying to unload map when one isn't loaded");
                return;
            }

            await Current.UnloadAsync();
            Current = null;

            Resources.UnloadUnusedAssets();
            GC.Collect(2, GCCollectionMode.Forced);
        }

    }

}
