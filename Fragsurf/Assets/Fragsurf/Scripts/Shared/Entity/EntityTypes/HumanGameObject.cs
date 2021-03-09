using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class HumanGameObject : EntityGameObject
    {

        [Header("Body")]

        [SerializeField]
        private GameObject _viewBody;
        [SerializeField]
        private BoxCollider _boundsCollider;
        [SerializeField]
        private Transform _feetAttachment;
        [SerializeField]
        private Transform _headAttachment;
        [SerializeField]
        private Transform _handAttachment;
        [SerializeField]
        private GameObject _ragdollPrefab;
        [SerializeField]
        private float _eyeOffset = 1.5f;
        [SerializeField]
        private float _duckedOffset = .5f;

        [Header("Audio")]

        [SerializeField]
        private AudioClip _deathSound;

        public bool Ragdolled { get; private set; } // todo :implement ragdolling
        public GameObject Ragdoll { get; set; }
        public GameObject ViewBody => _viewBody;
        public Transform FeetAttachment => _feetAttachment;
        public Transform HeadAttachment => _headAttachment;
        public Transform HandAttachment => _handAttachment;
        public BoxCollider BoundsCollider => _boundsCollider;
        public Vector3 EyeOffset => new Vector3(0, _eyeOffset, 0);
        public Vector3 DuckedEyeOffset => new Vector3(0, _eyeOffset - _duckedOffset, 0);
        public GameAudioSource FeetAudioSource { get; private set; }
        public GameAudioSource HandAudioSource { get; private set; }
        public GameAudioSource HeadAudioSource { get; private set; }

        private Vector3 _rot;
        public override Vector3 Rotation
        {
            get => _rot;
            set {
                _rot = value;
                _viewBody.transform.rotation = Quaternion.Euler(0, value.y, value.z);
            }
        }

        public Human Human => Entity as Human;

        protected override void Awake()
        {
            base.Awake();

            if (_viewBody == null)
            {
                _viewBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                _viewBody.DestroyComponentsInChildren<Collider>();
                _viewBody.transform.SetParent(transform, true);
                _viewBody.transform.localScale = Vector3.one;
                _viewBody.transform.localPosition = new Vector3(0, 1f, 0);
                _viewBody.transform.localRotation = Quaternion.identity;
            }

            if (_boundsCollider == null)
            {
                _boundsCollider = gameObject.AddComponent<BoxCollider>();
                _boundsCollider.center = new Vector3(0, 0.915f, 0);
                _boundsCollider.size = new Vector3(.858f, 1.83f, .858f);
            }

            _boundsCollider.gameObject.tag = "Player";

            if (!TryGetComponent(out Rigidbody rb))
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            rb.isKinematic = true;
        }

        protected override void Start()
        {
            base.Start();

            if (_feetAttachment)
            {
                if (!_feetAttachment.TryGetComponent(out GameAudioSource src))
                {
                    src = CreateAudioSource(SoundCategory.Player, 0.25f, 20f);
                }
                FeetAudioSource = src;
            }

            if (_headAttachment)
            {
                if (!_headAttachment.TryGetComponent(out GameAudioSource src))
                {
                    src = CreateAudioSource(SoundCategory.Player, 0.25f, 20f);
                }
                HeadAudioSource = src;
            }

            if (_handAttachment)
            {
                if (!_handAttachment.TryGetComponent(out GameAudioSource src))
                {
                    src = CreateAudioSource(SoundCategory.Player, 0.25f, 20f);
                }
                HandAudioSource = src;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Ragdoll)
            {
                DestroyRagdoll();
            }
        }

        public void SetLayersToIgnore()
        {
            gameObject.layer = Layers.IgnoreRaycast;
            foreach (var hb in Hitboxes)
            {
                hb.gameObject.layer = Layers.IgnoreRaycast;
            }
        }

        public void ResetLayers()
        {
            gameObject.layer = Entity.Game.IsHost
                ? Layers.Host
                : Layers.Client;
        }

        private void OnHumanRevived()
        {
            ResetLayers();

            if (!Entity.Game.IsHost)
            {
                DestroyRagdoll();
                SetVisible(true);
            }
        }

        private void OnHumanKilled(DamageInfo dmgInfo)
        {
            SetLayersToIgnore();

            // todo: dmgInfo on client isn't the networked dmgInfo
            if (!Entity.Game.IsHost)
            {
                SetVisible(false);

                if (HeadAudioSource)
                {
                    HeadAudioSource.PlayClip(_deathSound, Random.Range(0.75f, 1f));
                }

                var killer = Entity.Game.EntityManager.FindEntity(dmgInfo.AttackerEntityId);
                var ragdollDirection = Vector3.zero;

                if (killer != null)
                {
                    ragdollDirection = (Entity.Origin - killer.Origin).normalized;
                }

                var ragdollForce = ((Entity as Human).Velocity * .5f) + (ragdollDirection * 10f);
                SpawnRagdoll(ragdollForce, dmgInfo.HitPoint);
            }
        }

        private void SpawnRagdoll(Vector3 force, Vector3 point, Vector3 torque = default)
        {
            if (Ragdoll)
            {
                DestroyRagdoll();
            }
            Ragdoll = Entity.Game.Instantiate(_ragdollPrefab);
            CopyTransformToRagdoll(transform, Ragdoll.transform);
            Ragdoll.GetComponent<RagdollBehaviour>().Ragdoll(force, point, torque);
            Ragdolled = true;
        }

        private void DestroyRagdoll()
        {
            GameObject.Destroy(Ragdoll);
            Ragdolled = false;
        }

        private void CopyTransformToRagdoll(Transform reference, Transform ragdoll)
        {
            ragdoll.position = reference.position;
            ragdoll.rotation = reference.rotation;

            for (int i = 0; i < reference.childCount; i++)
            {
                if (ragdoll.childCount <= i)
                {
                    break;
                }

                var referenceTransform = reference.GetChild(i);
                var ragdollTransform = ragdoll.GetChild(i);

                if (referenceTransform != null && ragdollTransform != null)
                {
                    CopyTransformToRagdoll(referenceTransform, ragdollTransform);
                }
            }
        }

        private int _groundedCounter;
        private float _fallingTimer = 1.5f;
        private Vector3 _desiredMoveBlend;
        private Vector3 _currentMoveBlend;
        private Vector3 _moveBlendVelocity;

        protected override void _Update()
        {
            base._Update();

            if (!Animator)
            {
                return;
            }

            _desiredMoveBlend = ViewBody.transform.InverseTransformDirection(Human.Velocity);
            _currentMoveBlend = Vector3.SmoothDamp(_currentMoveBlend, _desiredMoveBlend, ref _moveBlendVelocity, .1f);
            Animator.SetFloat("sideways", _currentMoveBlend.x);
            Animator.SetFloat("forward", _currentMoveBlend.z);
        }

        private void OnHumanRunCommand()
        {
            if (!Animator
                || !(Human.MovementController is CSMovementController move))
            {
                return;
            }

            if (!move.GroundObject)
            {
                _groundedCounter = 0;
                _fallingTimer -= Time.deltaTime;
            }
            else
            {
                _groundedCounter++;
                _fallingTimer = 1.5f;
            }

            if (move.MoveData.JustJumped)
            {
                Animator.SetTrigger("jump");
            }

            Animator.SetBool("grounded", _groundedCounter > 1);
            Animator.SetBool("falling", _fallingTimer <= 0);
            Animator.SetBool("crouching", Human.Ducked);
        }

    }
}