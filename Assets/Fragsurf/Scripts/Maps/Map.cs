using System;
using System.Threading.Tasks;
using UnityEngine;
using Fragsurf.Utility;
using Fragsurf.Actors;
using System.Collections.Generic;
using System.Linq;

namespace Fragsurf.Maps
{
    public class Map : SingletonComponent<Map>
    {

        public static BaseMap Current { get; private set; }

        [ConVar("map.default", "")]
        public string DefaultMap { get; set; } = "surf_fst_skyworld";

        private static List<IMapProvider> _providers = new List<IMapProvider>()
        {
            new SceneMapProvider(),
            new BSPMapProvider(),
            new ModMapProvider()
        };

        private void Awake()
        {
            TimeStep.Instance.OnTick.AddListener(OnTick);
        }

        private void OnTick(float a, float b)
        {
            Current?.Tick();
        }

        public static async Task<BaseMap> Query(string mapName)
        {
            return (await QueryAll()).FirstOrDefault(x => x.Name.Equals(mapName, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<List<BaseMap>> QueryAll(string prefix = null)
        {
            var result = new List<BaseMap>();

            foreach(var provider in _providers)
            {
                result.AddRange(await provider.GetMapsAsync());
            }

            if(!string.IsNullOrEmpty(prefix))
            {
                result.RemoveAll(x => x.Name == null || !x.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            }

            return result;
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

        private static List<FSMSpawnPoint> _spawnPoints = new List<FSMSpawnPoint>();
        public static void GetSpawnPoint(out Vector3 position, out Vector3 angles, int teamNumber = 255)
        {
            position = Vector3.zero;
            angles = Vector3.zero;

            if (_spawnPoints.Count == 0)
            {
                _spawnPoints = GameObject.FindObjectsOfType<FSMSpawnPoint>().ToList();
                if(_spawnPoints.Count == 0)
                {
                    return;
                }
            }

            var spawnPoint = _spawnPoints[0];
            if(teamNumber > 0 && teamNumber < 255)
            {
                var potentialPoints = _spawnPoints.Where(x => x.TeamNumber == teamNumber);
                spawnPoint = potentialPoints.ElementAt(UnityEngine.Random.Range(0, potentialPoints.Count()));
            }
            else
            {
                spawnPoint = _spawnPoints.ElementAt(UnityEngine.Random.Range(0, _spawnPoints.Count()));
            }

            if (spawnPoint)
            {
                position = spawnPoint.transform.position;
                angles = spawnPoint.transform.eulerAngles;
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
            var map = await Query(mapName);
            if(map == null)
            {
                return MapLoadState.Failed;
            }
            return await LoadAsync(map);
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
