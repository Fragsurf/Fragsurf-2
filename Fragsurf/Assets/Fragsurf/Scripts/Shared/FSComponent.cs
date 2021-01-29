using System;
using UnityEngine;

namespace Fragsurf.Shared
{
    public abstract class FSComponent : MonoBehaviour
    {

        public FSGameLoop Game { get; private set; }
        public BaseGamemode Gamemode { get; set; }
        public bool Started { get; private set; }
        public bool Destroyed { get; private set; }

        public void Initialize(FSGameLoop game)
        {
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

    }
}
