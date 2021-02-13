using UnityEngine;
using Fragsurf.Shared;

namespace Fragsurf.Client
{
    [Inject(InjectRealm.Shared)]
    public class SoundManager : FSSharedScript
    {

        public enum SoundCategory
        {
            UI,
            Weapon,
            Player,
            Music,
            Voice
        }

        public void PlayAudioClip(AudioClip clip, SoundCategory category, float volume)
        {
            if (!Game.IsHost)
            {
                var cam = Camera.main ? Camera.main : GameCamera.Camera;
                if (!cam.TryGetComponent(out AudioSource src))
                {
                    src = cam.gameObject.AddComponent<AudioSource>();
                }
                src.PlayOneShot(clip, volume);
            }
            else
            {
                // network the sound?
            }
        }

        public void PlayAudioClipAttached(AudioClip clip, SoundCategory category, float volume, Transform transform)
        {
        }

    }
}

