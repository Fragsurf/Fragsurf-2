using System;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Movement;
using Fragsurf.Utility;
using Fragsurf.Actors;

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
        protected bool _autoTickTimeline = true;
        private EntityGameObject _entityGameObject;
        private bool _firstTick = true;

        public int EntityId { get; set; }
        public NetEntityState State { get; private set; }
        public byte TypeId { get; private set; }
        public float ChangedTime { get; private set; }
        public int ChangedTick { get; private set; }
        public bool Started => State == NetEntityState.Started;
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
        public InterpolationMode InterpolationMode { get; protected set; }
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

        public void Start()
        {
            if (State == NetEntityState.Deleted)
            {
                Debug.LogError("Trying to start a deleted entity.");
                return;
            }

            InterpolationMode = Game.IsHost
                ? InterpolationMode.Snap
                : InterpolationMode.Network;

            HasAuthority = Game.IsHost;
            UniqueId = EntityId;

            TypeId = GetEntityTypeId(GetType());
            _Start();

            State = NetEntityState.Started;
        }

        public void Update()
        {
            if (EntityGameObject)
            {
                EntityGameObject.OnUpdate?.Invoke();
            }
            _Update();
        }

        public void Tick()
        {
            if (_firstTick)
            {
                _firstTick = false;
                _FirstTick();
            }

            if (EntityGameObject)
            {
                EntityGameObject.OnTick?.Invoke();
            }

            if (_autoTickTimeline)
            {
                Timeline?.Tick(this);
            }
            
            _Tick();
        }

        public void LateUpdate()
        {
            _LateUpdate();
        }

        public void Delete()
        {
            State = NetEntityState.Deleted;

            _Delete();

            GameObject.Destroy(EntityGameObject.gameObject);
        }

        protected virtual void _Start() { }
        protected virtual void _FirstTick() { }
        protected virtual void _Update() { }
        protected virtual void _LateUpdate() { }
        protected virtual void _Tick() { }
        protected virtual void _Delete() { }

        public virtual bool IsValid()
        {
            if (State == NetEntityState.Deleted)
            {
                return false;
            }
            return true;
        }

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

        public static Dictionary<byte, Type> EntityTypeIdMap;
        public static byte GetEntityTypeId(Type type)
        {
            if (EntityTypeIdMap == null)
            {
                EntityTypeIdMap = BuildEntityTypeIdMap();
            }
            foreach (var kvp in EntityTypeIdMap)
            {
                if (type == kvp.Value)
                {
                    return kvp.Key;
                }
            }
            return 0;
        }

        public static NetEntity CreateInstanceOfEntity(FSGameLoop game, byte typeId)
        {
            if (EntityTypeIdMap == null)
            {
                EntityTypeIdMap = BuildEntityTypeIdMap();
            }
            if (!EntityTypeIdMap.ContainsKey(typeId))
            {
                return null;
            }
            return Activator.CreateInstance(EntityTypeIdMap[typeId], args: game) as NetEntity;
        }

        private static Dictionary<byte, Type> BuildEntityTypeIdMap()
        {
            var result = new Dictionary<byte, Type>();
            byte index = 0;
            foreach (var t in ReflectionExtensions.GetTypesOf<NetEntity>())
            {
                result.Add(index, t);
                index++;
            }
            return result;
        }

    }
}

