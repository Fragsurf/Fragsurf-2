using Fragsurf.Client;
using Fragsurf.Shared.Entity;
using UnityEngine;

namespace Fragsurf.Misc
{
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

            SoundManager.PlaySoundAttached(sound, vol, transform);
        }

        private void Update()
        {
            _soundTimer -= Time.deltaTime;
        }

    }
}

