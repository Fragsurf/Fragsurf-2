using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fragsurf.Shared.Maps;
using Fragsurf.Utility;
using Fragsurf.Actors;

namespace Fragsurf.Shared
{
    public class MapLoader : SingletonComponent<MapLoader>
    {

        public Action<IFragsurfMap, MapEventType, bool> OnMapEvent;

        private string _nextMap;

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

        private string[] _validExtensions = new string[]
        {
            "fsm"
        };

        private bool SceneExists(string mapName)
        {
            var sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                var sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                if(string.Equals(mapName, sceneName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private IFragsurfMap GetMapLoader(string path)
        {
            if(path == "LoadActiveScene")
            {
                return new ActiveSceneMap();
            }

            FSFileInfo fileInfo = null;

            if (!Path.HasExtension(path) && !ulong.TryParse(path, out ulong workshopId))
            {
                foreach (var ext in _validExtensions)
                {
                    fileInfo = FileSystem.GetOrAcquire($"{path}.{ext}", true);
                    if (fileInfo != null)
                    {
                        break;
                    }
                }
            }
            else
            {
                fileInfo = FileSystem.GetOrAcquire(path, true);
            }

            if(fileInfo == null)
            {
                return null;
            }

            switch (fileInfo.Extension)
            {
                default:
                    return null;
            }
        }

        public async Task<MapLoadState> LoadMapAsync2(string mapName)
        {
            if(CurrentMap != null)
            {
                await UnloadMapAsync();
            }
            CurrentMap = GetMapLoader(mapName);
            if(CurrentMap == null)
            {
                return MapLoadState.Failed;
            }
            var result = await CurrentMap.LoadAsync();
            if(result == MapLoadState.Loaded)
            {
                OnMapLoaded();
            }
            return result;
        }

        public async Task UnloadMapAsync()
        {
            if(CurrentMap == null)
            {
                return;
            }
            await CurrentMap.UnloadAsync();
            OnMapUnloaded();
        }

        private void OnMapLoaded()
        {
            OnMapEvent?.Invoke(CurrentMap, MapEventType.Loaded, false);

            GC.Collect(2, GCCollectionMode.Forced);
        }

        private void OnMapUnloaded()
        {
            var hasNextMap = !string.IsNullOrEmpty(_nextMap);
            OnMapEvent?.Invoke(CurrentMap, MapEventType.Unloaded, hasNextMap);
            CurrentMap = null;

            Resources.UnloadUnusedAssets();
            GC.Collect(2, GCCollectionMode.Forced);
        }

    }

    public class ActiveSceneMap : BaseMap
    {
        public override string Name => SceneManager.GetActiveScene().name;

        public override MapLoadState State { get; set; }

        public override void GetSpawnPoint(out Vector3 position, out Vector3 angles, int teamNumber = 255)
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

            var tp = GameObject.FindObjectOfType<PlayTest>();
            if (tp && tp.SpawnPoint != Vector3.zero)
            {
                position = tp.SpawnPoint;
                angles = Vector3.zero;
            }
        }

        public override async Task UnloadAsync()
        {
            await Task.Delay(100);
        }

        protected override async Task<MapLoadState> _LoadAsync()
        {
            await Task.Delay(100);
            return MapLoadState.Loaded;
        }
    }
}
