using Fragsurf.Client;
using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class HumanGameObject : EntityGameObject
    {

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

        private bool _ragdolled;

        public bool Ragdolled { get; set; } // todo :implement ragdolling
        public GameObject Ragdoll { get; set; }
        public GameObject ViewBody => _viewBody;
        public Transform FeetAttachment => _feetAttachment;
        public Transform HeadAttachment => _headAttachment;
        public Transform HandAttachment => _handAttachment;
        public BoxCollider BoundsCollider => _boundsCollider;
        public Vector3 EyeOffset => new Vector3(0, _eyeOffset, 0);

        public override Vector3 Rotation
        {
            get => _viewBody.transform.rotation.eulerAngles;
            set => _viewBody.transform.rotation = Quaternion.Euler(0, value.y, value.z);
        }

        protected override void Awake()
        {
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

            if(!TryGetComponent(out Rigidbody rb))
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            rb.isKinematic = true;

            base.Awake();
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

                if (AudioSource
                    && GameData.Instance.DeathSound)
                {
                    AudioSource.PlayOneShot(GameData.Instance.DeathSound, 1.0f);
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
            _ragdolled = true;
        }

        private void DestroyRagdoll()
        {
            GameObject.Destroy(Ragdoll);
            _ragdolled = false;
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

        private void OnHumanRunCommand()
        {

        }

    }
}