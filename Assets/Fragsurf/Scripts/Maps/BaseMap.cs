using Fragsurf.Actors;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Maps
{
    public abstract class BaseMap
    {

        private static List<string> _newMaps
            = new List<string>()
            {
                "bhop_cyber"
            };

        public string Name;
        public string Author;
        public Texture2D Cover;
        public string FilePath;
        public string MountedGame;

        private string _uniqueId;
        public string UniqueId
        {
            get { return !string.IsNullOrEmpty(_uniqueId) ? _uniqueId : Name; }
            set { _uniqueId = value; }
        }

        public bool IsMounted => !string.IsNullOrWhiteSpace(MountedGame);
        public bool IsNew => _newMaps.Contains(Name.ToLower());

        public List<FSMActor> Actors { get; } = new List<FSMActor>();
        public abstract Texture LoadCoverImage();

        public async Task<MapLoadState> LoadAsync()
        {
            var result = await _LoadAsync();

            if(result == MapLoadState.Loaded)
            {
                Actors.AddRange(GameObject.FindObjectsOfType<FSMActor>(true));

                foreach(var al in GameObject.FindObjectsOfType<AudioListener>(true))
                {
                    if(al == GameCamera.AudioListener)
                    {
                        continue;
                    }
                    GameObject.Destroy(al);
                }
            }

            return result;
        }

        public async Task UnloadAsync()
        {
            await _UnloadAsync();
        }

        public void Tick()
        {
            for (int i = Actors.Count - 1; i >= 0; i--)
            {
                if (!Actors[i])
                {
                    Actors.RemoveAt(i);
                    continue;
                }
                if (!Actors[i].isActiveAndEnabled)
                {
                    continue;
                }
                Actors[i].Tick();
            }
            _Tick();
        }

        protected virtual void _Tick() { }
        protected abstract Task<MapLoadState> _LoadAsync();
        protected abstract Task _UnloadAsync();

        // stuff from previous maploader, just in case...
        //private void InitializeMap()
        //{
        //    _fsmActors = GameObject.FindObjectsOfType<Fragsurf.Actors.FSMActor>();
        //    _spawnPoints = GameObject.FindObjectsOfType<Fragsurf.Actors.FSMSpawnPoint>();

        //    var dynamicActors = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IHasNetProps>();
        //    var uniqueIndex = int.MaxValue;
        //    foreach (var actor in dynamicActors)
        //    {
        //        actor.UniqueId = uniqueIndex;
        //        uniqueIndex--;
        //        if (GameServer.Instance != null)
        //        {
        //            var ent = new ActorSync(actor.UniqueId, GameServer.Instance);
        //            GameServer.Instance.EntityManager.AddEntity(ent);
        //        }
        //    }

        //    foreach (var actor in _fsmActors)
        //    {
        //        if (actor is FSMTrigger trigger)
        //        {
        //            foreach (var collider in trigger.GetComponentsInChildren<Collider>())
        //            {
        //                collider.gameObject.tag = "Trigger";
        //            }
        //        }
        //        if (actor is IProxyActor proxy)
        //        {
        //            var t = Type.GetType(proxy.ProxyTarget);
        //            var targetObj = (FSMActor)actor.gameObject.AddComponent(t);
        //            var srcFields = actor.GetType().GetFields().Where(f => f.IsPublic);
        //            var targetFields = t.GetFields().Where(f => f.IsPublic);
        //            foreach (var srcField in srcFields)
        //            {
        //                var targetField = targetFields.First(x => x.Name == srcField.Name);
        //                targetField.SetValue(targetObj, srcField.GetValue(actor));
        //            }
        //        }
        //    }
        //}

    }
}

