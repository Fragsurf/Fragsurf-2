using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Shared.Player
{
    public class ModelBehaviour : MonoBehaviour
    {

        public GameObject SkinRoot;
        public Renderer TeamColorRenderer;
        public Vector3 EyeOffset;
        public Collider BoundsCollider;
        public GameObject ViewBody;
        public Animator Animator;
        public Vector3 CustomPivot;
        public bool LockXRotation;

        [Header("Attachment Points")]
        public Transform Head;
        public Transform Feet;
        public Transform Hand;

        [HideInInspector]
        public int EntityId;

        private Renderer[] _renderers;
        [Header("Choose one or the other")]
        [SerializeField]
        private RagdollBehaviour _ragdoll;
        [SerializeField]
        private GameObject _ragdollPrefab;
        private int _defaultLayer;

        public FSGameLoop Game { get; set; }
        public HitboxBehaviour[] Hitboxes { get; private set; }
        public RagdollBehaviour Ragdoll => _ragdoll;
        public bool Visible { get; private set; } = true;
        public Vector3 Position => transform.position;
        public Vector3 Angles { get; private set; }

        private void Awake()
        {
            _renderers = ViewBody.GetComponentsInChildren<Renderer>(true);
            Hitboxes = ViewBody.GetComponentsInChildren<HitboxBehaviour>(true);

            // if interpolation isn't set to none the object will drift away from its parent for some odd reason..
            if(_ragdoll)
            {
                var rb = _ragdoll.GetComponent<Rigidbody>();
                if(rb)
                {
                    rb.interpolation = RigidbodyInterpolation.None;
                }
            }
        }

        private void Start()
        {
            _defaultLayer = gameObject.layer;

            if(Game != null && Game.IsHost)
            {
                SetVis(false);
            }

            foreach (var anim in ViewBody.GetComponentsInChildren<Animator>())
            {
                anim.cullingMode = Game.LagCompensator != null && Game.LagCompensator.Enabled ? AnimatorCullingMode.AlwaysAnimate : AnimatorCullingMode.CullCompletely;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.TransformPoint(CustomPivot), Vector3.one * 0.1f);
        }

        private int _currentLayer = 0;
        public void SetAnimationLayer(int layerIndex)
        {
            Animator.SetLayerWeight(_currentLayer, 0);
            Animator.SetLayerWeight(layerIndex, 1);
            _currentLayer = layerIndex;
        }

        public void SetColor(Color color)
        {
            if(TeamColorRenderer)
            {
                TeamColorRenderer.material.color = color;
            }
        }

        public void SetPosition(Vector3 position)
        {
            if(position.IsNaN())
            {
                return;
            }
            transform.position = position;
        }

        public void SetAngles(Vector3 angles)
        {
            Angles = angles;
            if (LockXRotation) angles.x = 0;
            var bodyRot = Quaternion.Euler(angles);
            ViewBody.transform.rotation = bodyRot;
        }

        public void Set(Vector3 position, Vector3 angles)
        {
            SetPosition(position);
            SetAngles(angles);
        }

        public void SetVisibleLayer(bool visible)
        {
            foreach (Renderer r in _renderers)
            {
                // todo: error occurs without the null ref check, when HandItem is destroyed.
                // check the stack trace and clean it up.
                if (r == null)
                    continue;

                r.gameObject.layer = visible ? _defaultLayer : Layers.Invisible;
            }
        }

        public void SetVisible(bool visible)
        {
            if (Game != null && Game.IsHost)
            {
                return;
            }

            SetVis(visible);
        }

        public void SetLayersToIgnore()
        {
            gameObject.layer = Layers.IgnoreRaycast;
            foreach(var hb in Hitboxes)
            {
                hb.gameObject.layer = Layers.IgnoreRaycast;
            }
        }

        public void ResetLayers()
        {
            gameObject.layer = Game.IsHost
                ? Layers.Host
                : Layers.Client;
        }

        public void StartRagdoll(Vector3 force, Vector3 forcePosition, Vector3 torque = default)
        {
            if (_ragdollPrefab != null)
            {
                if (_ragdoll != null)
                {
                    GameObject.Destroy(_ragdoll.gameObject);
                }
                _ragdoll = GameObject.Instantiate(_ragdollPrefab).GetComponent<RagdollBehaviour>();
                SetVis(false);
                SetAnimator(false);
            }
            CopyTransformToRagdoll(transform, _ragdoll.transform);
            _ragdoll.Ragdoll(force, forcePosition, torque);
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

                var ref_t = reference.GetChild(i);
                var rag_t = ragdoll.GetChild(i);

                if (ref_t != null && rag_t != null)
                {
                    CopyTransformToRagdoll(ref_t, rag_t);
                }
            }
        }

        public void EndRagdoll()
        {
            SetAnimator(true);

            if (_ragdoll == null)
            {
                return;
            }

            if(_ragdollPrefab != null)
            {
                GameObject.Destroy(_ragdoll.gameObject);
                SetVis(true);
            }
            else
            {
                _ragdoll.UnRagdoll();
            }
        }

        private void SetVis(bool visible)
        {
            Visible = visible;
            foreach (Renderer r in _renderers)
            {
                // todo: error occurs without the null ref check, when HandItem is destroyed.
                // check the stack trace and clean it up.
                if (r == null)
                    continue;

                r.enabled = visible;
            }
            if(!Game.IsHost)
            {
                SetAnimator(visible);
            }
        }

        private void SetAnimator(bool enabled)
        {
            if(Animator)
            {
                Animator.enabled = enabled;
            }
        }

    }
}

