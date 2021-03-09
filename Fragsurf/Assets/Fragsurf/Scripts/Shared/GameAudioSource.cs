using Fragsurf.Shared.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    [RequireComponent(typeof(AudioSource))]
    public class GameAudioSource : MonoBehaviour
    {

        public SoundCategory Category;
        public bool IsHost { get; set; }
        public AudioSource Src { get; private set; }

        private void Awake()
        {
            Src = GetComponent<AudioSource>();
        }

        private void Start()
        {
            var ent = GetComponentInParent<EntityGameObject>();
            if (ent && ent.Entity != null)
            {
                IsHost = ent.Entity.Game.IsHost;
            }
        }

        public void PlayClip(AudioClip clip, float volume = 1f, bool stop = false)
        {
            if (!clip || !Src)
            {
                return;
            }
            var game = FSGameLoop.GetGameInstance(IsHost);
            if (!game)
            {
                if (stop)
                {
                    Src.Stop();
                }
                Src.PlayOneShot(clip, volume);
                return;
            }
            game.Audio.PlayClip(this, clip, volume, stop);
        }

    }
}

