using UnityEngine;

namespace Fragsurf.Shared
{
    [Inject(InjectRealm.Shared)]
    public class GameAudioManager : FSSharedScript
    {
        public void PlayClip(GameAudioSource src, AudioClip clip, float volume, bool stop = false)
        {
            if (Game.IsHost)
            {
                // network the sound?
                return;
            }
            if(src.Category == SoundCategory.UI)
            {
                volume = Random.Range(0, 1f);
            }
            if (stop)
            {
                src.Src.Stop();
            }
            src.Src.PlayOneShot(clip, volume);
        }
    }

    public enum SoundCategory
    {
        None,
        UI,
        Weapon,
        Player,
        Music,
        Voice,
        Effects
    }
}

