using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using SurfaceConfigurator;
using UnityEngine;

namespace Fragsurf.Misc
{
    [RequireComponent(typeof(AudioSource))]
    public class PhysicsFidelityObject : MonoBehaviour, IClientComponent
    {

        [SerializeField]
        private float _soundDelay = .25f;
        [SerializeField]
        private float _velocityClipSplit = 1f;
        [SerializeField]
        private float _velocityMaxVolume = 2f;

        private float _soundTimer;
        private AudioSource _source;

        private static Collider[] _overlapResult = new Collider[32];

        private void Start()
        {
            _source = GetComponent<AudioSource>();
        }

        void OnCollisionEnter(Collision coll)
        {
            if (_soundTimer > 0)
            {
                return;
            }

            var overlaps = Physics.OverlapSphereNonAlloc(coll.GetContact(0).point, .1f, _overlapResult, 1 << Layers.Fidelity);
            if(overlaps == 0)
            {
                return;
            }

            var surfaceType = SurfaceType.Concrete;
            if (_overlapResult[0].TryGetComponent(out SurfaceTypeIdentifier sti))
            {
                surfaceType = sti.SurfaceType;
            }

            if(GameData.Instance.TryGetPhysicsSound(surfaceType, out AudioClip clip))
            {
                var vol = Mathf.Lerp(0, 1, coll.relativeVelocity.magnitude / _velocityMaxVolume);
                _source.PlayOneShot(clip, vol);
                _soundTimer = _soundDelay;
            }
        }

        private void Update()
        {
            _soundTimer -= Time.deltaTime;
        }

    }
}

