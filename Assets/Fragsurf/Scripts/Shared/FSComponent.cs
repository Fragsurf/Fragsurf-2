using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using System;
using UnityEngine;

namespace Fragsurf.Shared
{
    public abstract class FSComponent : MonoBehaviour, IHasNetProps
    {

        public FSGameLoop Game { get; private set; }
        public BaseGamemode Gamemode { get; set; }
        public bool Started { get; private set; }
        public bool Destroyed { get; private set; }
        public int UniqueId { get; set; }
        public virtual bool HasNetProps => false;
        public virtual bool HasAuthority => Game.IsHost;

        public void Initialize(FSGameLoop game)
        {
            // todo: maybe this isn't a good idea
            // because IHasNetProps is implemented for NetEntity, FSMActor, and FSComponent
            // maybe it's better to build a system for identifying individual instances
            // otherwise, just do our best to make sure there's no conflict and it's 
            // deterministic across machines.
            // Entities = EntityId (1...)
            // Actors = offset + Accumulator
            // Components = stable hash of type Name
            UniqueId = GetType().Name.GetStableHashCode();
            Game = game;

            try
            {
                _Hook();
                _Initialize();

                DevConsole.RegisterObject(this);

                Started = true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to register " + GetType().Name + "\n" + e);
                Started = false;
            }
        }

        public void Unhook()
        {
            _Unhook();
        }

        public void Destroy()
        {
            _Destroy();

            DevConsole.RemoveAll(this);
            Game.RemoveFSComponent(this);
            Game = null;
            Destroyed = true;

            GameObject.Destroy(this);
        }

        public void OnStart()
        {
            try
            {
                _Start();
                if (HasNetProps)
                {
                    BuildNetProps();
                }
            }
           catch(Exception e)
            {
                Debug.LogError("Failed to start " + GetType().Name + ":" + e.Message);
            }
        }

        public void OnUpdate()
        {
            if (Destroyed)
            {
                return;
            }
            _Update();
        }

        public void OnLateUpdate()
        {
            if (Destroyed)
            {
                return;
            }
            _LateUpdate();
        }

        public void Tick()
        {
            if (Destroyed)
            {
                return;
            }
            _Tick();
        }

        protected virtual void _Hook() { }
        protected virtual void _Unhook() { }
        protected virtual void _Initialize() { }
        protected virtual void _Destroy() { }
        protected virtual void _Update() { }
        protected virtual void _LateUpdate() { }
        protected virtual void _Tick() { }
        protected virtual void _Start() { }

#if UNITY_EDITOR
        public virtual void DrawGizmos() { }
#endif

        private void BuildNetProps()
        {
            if (Game.IsHost)
            {
                var actor = new FSComponentSync(Game, UniqueId);
                Game.EntityManager.AddEntity(actor);
            }
        }

    }
}
