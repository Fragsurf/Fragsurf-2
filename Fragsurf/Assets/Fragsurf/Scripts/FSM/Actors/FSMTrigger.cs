using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared;
using UnityEngine.Events;

namespace Fragsurf.FSM.Actors
{
    [SelectionBase]
    [ExecuteAlways]
    public class FSMTrigger : FSMActor
    {

        public enum FilterType
        {
            None,
            Allow,
            Negate
        } 

        [Header("Trigger Options")]
        [Tooltip("None - No filtering.\nAllow - Only trigger if EntityName matches.\nNegate - Don't trigger if EntityName matches.")]
        public FilterType Filter;
        [Tooltip("EntityName to compare against")]
        public string FilteredEntityName;
        public float MinimumAllowedSpeed;
        public float MaximumAllowedSpeed;
        public FSMTriggerCondition TriggerCondition;

        [Header("Trigger Events")]
        public UnityEvent<NetEntity> OnTriggerEnter = new UnityEvent<NetEntity>();
        public UnityEvent<NetEntity> OnTriggerStay = new UnityEvent<NetEntity>();
        public UnityEvent<NetEntity> OnTriggerExit = new UnityEvent<NetEntity>();

        private HashSet<StayingData> _staying = new HashSet<StayingData>();

        protected override void _Start()
        {
            if (!Application.isPlaying)
            {
                foreach (var r in GetComponentsInChildren<Renderer>())
                {
                    r.material = Resources.Load<Material>("FSM/Materials/FSMTrigger Material"); 
                }
                EnableRenderers(true);
                return;
            }

            var childrenColliders = GetComponentsInChildren<Collider>();
            foreach (var collider in childrenColliders)
            {
                if(collider is MeshCollider mc)
                {
                    mc.convex = true;
                }
                collider.isTrigger = true;
            }
            if(childrenColliders.Length > 0 && childrenColliders[0].gameObject != gameObject)
            {
                if(!TryGetComponent(out Rigidbody rb))
                {
                    rb = gameObject.AddComponent<Rigidbody>();
                }
                rb.isKinematic = true;
            }
        }

        public void OnInteract(int entityId, bool isHost)
        {
            var entity = FSGameLoop.GetGameInstance(isHost).EntityManager.FindEntity(entityId);
            if(entity != null && PassesCondition(entity))
            {
                _TriggerInteract(entity);
            }
        }

        public void OnStartTouch(int entityId, bool isHost)
        {
            var entity = FSGameLoop.GetGameInstance(isHost).EntityManager.FindEntity(entityId);
            if (entity != null)
            {
                var sd = StayingData.Get(isHost, entityId);
                if (_staying.Contains(sd) || !PassesCondition(entity))
                {
                    return;
                }
                _TriggerEnter(entity);
                _staying.Add(sd);
                OnTriggerEnter?.Invoke(entity);
            }
        }

        public void OnEndTouch(int entityId, bool isHost)
        {
            var entity = FSGameLoop.GetGameInstance(isHost).EntityManager.FindEntity(entityId);
            var sd = StayingData.Get(isHost, entityId);
            if (entity != null && _staying.Contains(sd))
            {
                _TriggerExit(entity);
                _staying.Remove(sd);
                OnTriggerExit?.Invoke(entity);
            }
        }

        public void OnTouch(int entityId, bool isHost)
        {
            var entity = FSGameLoop.GetGameInstance(isHost).EntityManager.FindEntity(entityId);
            var sd = StayingData.Get(isHost, entityId);
            if (entity != null)
            {
                if(PassesCondition(entity))
                {
                    if(!_staying.Contains(sd))
                    {
                        _TriggerEnter(entity);
                        _staying.Add(sd);
                        OnTriggerEnter?.Invoke(entity);
                    }
                    else
                    {
                        _TriggerStay(entity);
                        OnTriggerStay?.Invoke(entity);
                    }
                }
                else
                {
                    if(_staying.Contains(sd))
                    {
                        _TriggerExit(entity);
                        _staying.Remove(sd);
                        OnTriggerExit?.Invoke(entity);
                    }
                }
            }
        }

        protected virtual void _TriggerInteract(NetEntity entity) { }
        protected virtual void _TriggerEnter(NetEntity entity) { }
        protected virtual void _TriggerStay(NetEntity entity) { }
        protected virtual void _TriggerExit(NetEntity entity) { }

        private bool PassesCondition(NetEntity entity)
        {
            // todo: more generic way to access MoveData etc

            if(!(entity is Human human)
                || !(human.MovementController is ISurfControllable surfer))
            {
                return false;
            }

            if(Filter != FilterType.None)
            {
                if(Filter == FilterType.Allow && entity.EntityName != FilteredEntityName)
                {
                    return false;
                }
                else if(Filter == FilterType.Negate && entity.EntityName == FilteredEntityName)
                {
                    return false;
                }
            }

            var vel = surfer.MoveData.Velocity;
            vel.y = 0;
            var speed = vel.magnitude;

            if(MinimumAllowedSpeed != 0 && speed < MinimumAllowedSpeed)
            {
                return false;
            }

            if(MaximumAllowedSpeed != 0 && speed > MaximumAllowedSpeed)
            {
                return false;
            }

            if (TriggerCondition == FSMTriggerCondition.None)
            {
                return true;
            }

            if(TriggerCondition == FSMTriggerCondition.InAir)
            {
                return surfer.GroundObject == null && !surfer.MoveData.Surfing;
            }

            if(TriggerCondition == FSMTriggerCondition.Sliding)
            {
                return surfer.MoveData.Sliding;
            }

            if(TriggerCondition == FSMTriggerCondition.Surfing)
            {
                return surfer.MoveData.Surfing;
            }

            if(TriggerCondition == FSMTriggerCondition.Grounded)
            {
                if(surfer.GroundObject != null 
                    || surfer.MoveData.JustJumped
                    || surfer.MoveData.JustGrounded
                    || surfer.MoveData.Sliding)
                {
                    return true;
                }
                return false;
            }

            if(TriggerCondition == FSMTriggerCondition.GroundedTwice)
            {
                return surfer.GroundObject != null && !surfer.MoveData.JustGrounded;
            }

            return true;
        }

        private struct StayingData
        {
            public bool Host;
            public int EntityId;

            public static StayingData Get(bool host, int entityId)
            {
                return new StayingData()
                {
                    Host = host,
                    EntityId = entityId
                };
            }

            public static bool operator ==(StayingData a, StayingData b)
            {
                return a.Host == b.Host && a.EntityId == b.EntityId;
            }

            public static bool operator !=(StayingData a, StayingData b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                return obj is StayingData c && this == c;
            }

            public override int GetHashCode()
            {
                return Host.GetHashCode() ^ EntityId.GetHashCode();
            }
        }

    }
}

