using Fragsurf.Client;
using Fragsurf.FSM.Actors;
using Fragsurf.Movement;
using Fragsurf.Server;
using Fragsurf.Shared.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Shared.Maps
{
    public abstract class BaseMap : IFragsurfMap
    {

        protected FSMActor[] _fsmActors;
        protected FSMSpawnPoint[] _spawnPoints;

        public abstract string Name { get; }
        public abstract MapLoadState State { get; set; }
        public abstract void GetSpawnPoint(out Vector3 position, out Vector3 angles, int teamNumber = 255);
        protected abstract Task<MapLoadState> _LoadAsync();
        public abstract Task UnloadAsync();

        public async Task<MapLoadState> LoadAsync()
        {
            var result = await _LoadAsync();
            if (result == MapLoadState.Loaded)
            {
                InitializeMap();
            }
            return result;
        }

        public virtual void Tick()
        {
            if(_fsmActors == null)
            {
                return;
            }

            foreach (var actor in _fsmActors)
            {
                if (actor.isActiveAndEnabled)
                {
                    actor.Tick();
                }
            }
        }

        public virtual void Hotload()
        {

        }

        private void InitializeMap()
        {
            _fsmActors = GameObject.FindObjectsOfType<Fragsurf.FSM.Actors.FSMActor>();
            _spawnPoints = GameObject.FindObjectsOfType<Fragsurf.FSM.Actors.FSMSpawnPoint>();

            var dynamicActors = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IHasNetProps>();
            var uniqueIndex = int.MaxValue;
            foreach (var actor in dynamicActors)
            {
                actor.UniqueId = uniqueIndex;
                uniqueIndex--;
                if (GameServer.Instance != null)
                {
                    var ent = new ActorSync(actor.UniqueId, GameServer.Instance);
                    GameServer.Instance.EntityManager.AddEntity(ent);
                }
            }

            foreach (var actor in _fsmActors)
            {
                if (actor is FSMTrigger trigger)
                {
                    foreach (var collider in trigger.GetComponentsInChildren<Collider>())
                    {
                        collider.gameObject.tag = "Trigger";
                    }
                }
                if (actor is IProxyActor proxy)
                {
                    var t = Type.GetType(proxy.ProxyTarget);
                    var targetObj = (FSMActor)actor.gameObject.AddComponent(t);
                    var srcFields = actor.GetType().GetFields().Where(f => f.IsPublic);
                    var targetFields = t.GetFields().Where(f => f.IsPublic);
                    foreach (var srcField in srcFields)
                    {
                        var targetField = targetFields.First(x => x.Name == srcField.Name);
                        targetField.SetValue(targetObj, srcField.GetValue(actor));
                    }
                }
            }
        }

    }
}

