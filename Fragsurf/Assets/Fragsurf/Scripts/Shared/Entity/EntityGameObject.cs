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

        protected GameObject _audioSourceContainer;

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
                    SetAudioSourceRealms(_entity.Game.IsServer);
                    if (_animator)
                    {
                        _animator.cullingMode = _entity.Game.IsServer
                            ? AnimatorCullingMode.AlwaysAnimate
                            : AnimatorCullingMode.CullCompletely;
                    }
                }
            }
        }

        public GameAudioSource AudioSource { get; protected set; }
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
            AudioSource = GetComponentInChildren<GameAudioSource>();
            Renderers = gameObject.GetComponentsInChildren<Renderer>();
            Hitboxes = gameObject.GetComponentsInChildren<HitboxBehaviour>();
        }

        protected virtual void Start()
        {
            OnUpdate.AddListener(() =>
            {
                if (!Entity.IsLive)
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
            if (_entity != null && _entity.Game.IsServer)
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

        protected GameAudioSource CreateAudioSource(SoundCategory cat, float minDistance, float maxDistance, AnimationCurve customRolloff = null)
        {
            minDistance = Mathf.Max(minDistance, 0.1f);

            if (!_audioSourceContainer)
            {
                _audioSourceContainer = new GameObject("[Audio Sources]");
                _audioSourceContainer.transform.SetParent(transform);
            }

            var obj = new GameObject("_");
            obj.transform.SetParent(_audioSourceContainer.transform);
            var result = obj.AddComponent<GameAudioSource>();
            result.Category = cat;
            result.Src.rolloffMode = AudioRolloffMode.Linear;
            result.Src.spatialBlend = 1f;
            result.Src.maxDistance = maxDistance;
            result.Src.minDistance = minDistance;
            if(Entity != null)
            {
                result.IsHost = Entity.Game.IsServer;
            }
            if (customRolloff != null)
            {
                result.Src.rolloffMode = AudioRolloffMode.Custom;
                result.Src.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customRolloff);
            }
            return result;
        }

        private void SetAudioSourceRealms(bool isHost)
        {
            if (AudioSource)
            {
                AudioSource.IsHost = isHost;
            }
            if (_audioSourceContainer)
            {
                foreach (var src in _audioSourceContainer.GetComponentsInChildren<GameAudioSource>(true))
                {
                    src.IsHost = isHost;
                }
            }
        }

        public void SetColor(Color color)
        {
            if(Renderers == null)
            {
                return;
            }

            foreach(var r in Renderers)
            {
                r.material.color = color;
            }
        }

    }
}