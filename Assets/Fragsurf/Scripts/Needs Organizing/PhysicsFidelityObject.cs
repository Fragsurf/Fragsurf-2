using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using UnityEngine;

namespace Fragsurf.Misc
{
    public class PhysicsFidelityObject : MonoBehaviour, IClientComponent
    {

        [SerializeField]
        private AudioSource _audioSrc;
        [SerializeField]
        private float _soundDelay = .25f;
        //[SerializeField]
        //private float _velocityClipSplit = 1f;
        [SerializeField]
        private float _velocityMaxVolume = 2f;

        private float _soundTimer;

        private static Collider[] _overlapResult = new Collider[32];

        private void Start()
        {
            if (!_audioSrc)
            {
                _audioSrc = gameObject.AddComponent<AudioSource>();
            }
            _audioSrc.rolloffMode = AudioRolloffMode.Linear;
            _audioSrc.spatialBlend = 1.0f;
            _audioSrc.minDistance = .1f;
            _audioSrc.maxDistance = 8f;
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

            if(GameData.Instance.TryGetPhysicsSound(surfaceType, out AudioClip clip))
            {
                var vol = Mathf.Lerp(0, 1, coll.relativeVelocity.magnitude / _velocityMaxVolume);
                _audioSrc.PlayOneShot(clip, vol);
                _soundTimer = _soundDelay;
            }
        }

        private void Update()
        {
            _soundTimer -= Time.deltaTime;
        }

    }
}

