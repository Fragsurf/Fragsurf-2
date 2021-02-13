using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Fragsurf.Shared.Entity
{
    public class EntityGameObject : MonoBehaviour, IHasNetProps
    {

        public UnityEvent OnUpdate = new UnityEvent();
        public UnityEvent OnTick = new UnityEvent();

        [SerializeField]
        protected Animator _animator;

        public virtual Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public virtual Vector3 Rotation
        {
            get => transform.eulerAngles;
            set => transform.eulerAngles = value;
        }

        private NetEntity _entity;
        public NetEntity Entity
        {
            get { return _entity; }
            set
            {
                _entity = value;
                if(_entity != null)
                {
                    _entity.BuildNetProps(this);
                    transform.SetLayerRecursively(Entity.Game.ScopeLayer);
                    foreach (var hb in Hitboxes)
                    {
                        hb.EntityId = _entity.EntityId;
                    }
                }
            }
        }

        public AudioSource AudioSource { get; private set; }
        public Animator Animator => _animator;
        public HitboxBehaviour[] Hitboxes { get; private set; }
        public Renderer[] Renderers { get; private set; }
        public bool IsVisible { get; private set; } = true;

        public int UniqueId { get => ((IHasNetProps)_entity).UniqueId; set => ((IHasNetProps)_entity).UniqueId = value; }
        public bool HasAuthority => ((IHasNetProps)_entity).HasAuthority;

        protected virtual void Awake()
        {
            if (!_animator)
            {
                _animator = GetComponentInChildren<Animator>();
            }
            AudioSource = GetComponentInChildren<AudioSource>();
            Renderers = gameObject.GetComponentsInChildren<Renderer>();
            Hitboxes = gameObject.GetComponentsInChildren<HitboxBehaviour>();
        }

        protected virtual void Start()
        {
            OnUpdate.AddListener(() =>
            {
                if (!Entity.IsValid())
                {
                    GameObject.Destroy(gameObject);
                    return;
                }
                _Update();
            });

            OnTick.AddListener(() =>
            {
                _Tick();
            });
        }

        protected virtual void OnDestroy()
        {
            _entity = null;
        }

        protected void CleanRealm()
        {
            if (_entity != null && _entity.Game.IsHost)
            {
                foreach (Object component in GetComponentsInChildren<IClientComponent>(true))
                {
                    GameObject.Destroy(component);
                }
            }
        }

        protected virtual void _Update() { }
        protected virtual void _Tick() { }

        public void SetVisible(bool visible/*, bool useLayer = false*/)
        {
            IsVisible = visible;
            foreach (Renderer r in Renderers)
            {
                r.enabled = visible;
            }
        }

    }
}