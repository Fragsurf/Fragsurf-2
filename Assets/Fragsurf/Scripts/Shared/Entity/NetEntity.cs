using System;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public abstract partial class NetEntity
    {

        protected NetEntity(FSGameLoop game)
        {
            Game = game;

            BuildNetProps(this);
        }

        protected Vector3 _origin;
        protected Vector3 _angles;
        private EntityGameObject _entityGameObject;
        private bool _firstTick = true;
        private bool _enabled = true;

        public int EntityId { get; set; }
        public NetEntityState State { get; private set; }
        public byte TypeId { get; private set; }
        public float ChangedTime { get; private set; }
        public int ChangedTick { get; private set; }
        public bool IsLive => State == NetEntityState.Live;
        public FSGameLoop Game { get; private set; }
        public bool HasAuthority { get; protected set; }
        public bool OriginOrRotationChanged { get; set; }
        public int UniqueId { get; set; }

        public EntityGameObject EntityGameObject
        {
            get => _entityGameObject;
            set
            {
                if (_entityGameObject)
                {
                    GameObject.Destroy(_entityGameObject.gameObject);
                }

                _entityGameObject = value;

                if (value)
                {
                    _entityGameObject.Entity = this;
                    _entityGameObject.transform.SetParent(Game.ObjectContainer.transform, true);
                    _entityGameObject.Position = Origin;
                    _entityGameObject.Rotation = Angles;
                }
            }
        }
        public InterpolationMode InterpolationMode { get; set; }
        public bool DisableLagCompensation { get; set; }

        [NetProperty(true)]
        public virtual Vector3 Origin
        {
            get { return _origin; }
            set
            {
                _origin = value;
                OriginOrRotationChanged = true;
                ChangedTime = Game.ElapsedTime;
                ChangedTick = Game.CurrentTick;
            }
        }
        [NetProperty(true)]
        public virtual Vector3 Angles
        {
            get { return _angles; }
            set
            {
                _angles = value;
                OriginOrRotationChanged = true;
                ChangedTime = Game.ElapsedTime;
                ChangedTick = Game.CurrentTick;
            }
        }
        [NetProperty]
        public string EntityName { get; set; } = string.Empty;
        [NetProperty]
        public bool Enabled
        {
            get => _enabled;
            set 
            {
                if(_enabled == value)
                {
                    return;
                }
                _enabled = value;
                (value ? (Action)OnEnabled : OnDisabled)();
            }
        }

        public void Initialize()
        {
            if (State != NetEntityState.Created)
            {
                Debug.LogError("Trying to initialize a bad entity.");
                return;
            }

            InterpolationMode = Game.IsHost
                ? InterpolationMode.Snap
                : InterpolationMode.Network;

            HasAuthority = Game.IsHost;
            UniqueId = EntityId;
            TypeId = GetEntityTypeId(GetType());

            OnInitialized();

            if (Enabled)
            {
                OnEnabled();
            }

            State = NetEntityState.Live;
        }

        public void Update()
        {
            if (!_enabled)
            {
                return;
            }

            OnUpdate();

            if (EntityGameObject)
            {
                EntityGameObject.OnUpdate?.Invoke();
            }
        }

        public void Tick()
        {
            if (!_enabled)
            {
                return;
            }

            if (_firstTick)
            {
                _firstTick = false;
                OnFirstTick();
            }

            if (EntityGameObject)
            {
                EntityGameObject.OnTick?.Invoke();
            }

            Tick_Timeline();
            
            OnTick();
        }

        public void LateUpdate()
        {
            if (!_enabled)
            {
                return;
            }

            OnLateUpdate();
        }

        public void Delete()
        {
            State = NetEntityState.Deleted;
            Enabled = false;

            OnDelete();

            if (EntityGameObject)
            {
                GameObject.Destroy(EntityGameObject.gameObject);
            }
        }

        protected virtual void OnInitialized() { }
        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void OnFirstTick() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        protected virtual void OnTick() { }
        protected virtual void OnDelete() { }

        public override string ToString()
        {
            var type = GetType().Name;
            return '[' + EntityId + ']'
                + '[' + (Game.IsHost ? "HOST" : "CLIENT") + ']'
                + type.ToString();
        }

#if UNITY_EDITOR
        public virtual void OnDrawGizmos()
        {
            if (Game.IsHost)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(Origin, Vector3.one * .15f);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(Origin, Vector3.one * .2f);
            }
        }
#endif

    }
}

