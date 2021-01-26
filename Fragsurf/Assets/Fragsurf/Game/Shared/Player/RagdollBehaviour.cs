using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared.Player
{
    public class RagdollBehaviour : MonoBehaviour
    {

        private class RagdollDefault
        {

            ///// Fields /////

            public Rigidbody Rigidbody;
            public Collider Collider;
            public Vector3 Position;
            public Quaternion Rotation;

            ///// Methods /////

            public void Ragdoll(Vector3 force, Vector3 forcePosition, Vector3 torque = default)
            {
                Collider.isTrigger = false;
                Rigidbody.isKinematic = false;
                Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                Rigidbody.AddForceAtPosition(force, forcePosition, ForceMode.VelocityChange);
                Rigidbody.AddTorque(torque, ForceMode.Impulse);
            }

            public void Reset()
            {
                Collider.isTrigger = true;
                Rigidbody.isKinematic = true;
                Rigidbody.rotation = Rotation;
                Rigidbody.position = Position;
            }

        }

        ///// Fields /////

        [SerializeField]
        private Transform _rootBone;
        [SerializeField]
        private Transform _root;
        [SerializeField]
        private bool _ragdollOnStart;
        private Vector3 _defaultLocalPosition;
        private List<RagdollDefault> _ragdollDefaults = new List<RagdollDefault>();

        ///// Properties /////

        public bool Ragdolled { get; private set; }
        public Vector3 Position => _rootBone.transform.position;

        ///// Methods /////

        private void Awake()
        {
            if(!_root)
            {
                _root = transform;
            }

            if(!_rootBone)
            {
                _rootBone = transform;
            }

            _defaultLocalPosition = _root.localPosition;

            var bodies = _root.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rbody in bodies)
            {
                rbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rbody.isKinematic = true;
                rbody.gameObject.layer = Layers.Ragdoll;

                _ragdollDefaults.Add(new RagdollDefault()
                {
                    Collider = rbody.GetComponent<Collider>(),
                    Rigidbody = rbody,
                    Position = rbody.position,
                    //LocalPosition = rbody.transform.localPosition,
                    Rotation = rbody.rotation,
                    //LocalRotation = rbody.transform.localRotation
                });
            }

            foreach(var d in _ragdollDefaults)
            {
                d.Reset();
            }
        }

        private void Start()
        {
            if (_ragdollOnStart)
            {
                Ragdoll(Vector3.up, transform.position);
            }
        }

        public void Ragdoll(Vector3 force, Vector3 forcePosition, Vector3 torque = default)
        {
            if (Ragdolled)
            {
                Debug.LogWarning("Thing is already ragdolled", gameObject);
                return;
            }

            var animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }

            for (int i = 0; i < _ragdollDefaults.Count; i++)
            {
                _ragdollDefaults[i].Ragdoll(force, forcePosition, torque);
            }

            Ragdolled = true;
        }

        public void UnRagdoll()
        {
            if (!Ragdolled)
            {
                return;
            }

            var animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
            }

            for (int i = 0; i < _ragdollDefaults.Count; i++)
            {
                _ragdollDefaults[i].Reset();
            }

            Ragdolled = false;
            _root.localPosition = _defaultLocalPosition;
        }

    }
}

