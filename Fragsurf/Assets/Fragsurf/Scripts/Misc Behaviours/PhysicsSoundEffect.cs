using Fragsurf.Shared.Entity;
using UnityEngine;

namespace Fragsurf.Misc
{
    [RequireComponent(typeof(AudioSource))]
    public class PhysicsSoundEffect : MonoBehaviour, IClientComponent
    {

        [SerializeField]
        private float _soundDelay = .25f;
        [SerializeField]
        private float _velocityClipSplit = 1f;
        [SerializeField]
        private float _velocityMaxVolume = 2f;
        [SerializeField]
        private AudioClip _softCollisionSound;
        [SerializeField]
        private AudioClip _hardCollisionSound;

        private float _soundTimer;
        private AudioSource _source;

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

            _soundTimer = _soundDelay;

            var vol = Mathf.Lerp(0, 1, coll.relativeVelocity.magnitude / _velocityMaxVolume);
            var sound = coll.relativeVelocity.magnitude < _velocityClipSplit
                ? _softCollisionSound
                : _hardCollisionSound;

            _source.PlayOneShot(sound, vol);
        }

        private void Update()
        {
            _soundTimer -= Time.deltaTime;
        }

    }
}

