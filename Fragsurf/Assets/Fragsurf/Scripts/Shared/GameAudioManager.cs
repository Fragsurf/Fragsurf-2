using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    [Inject(InjectRealm.Shared)]
    public class GameAudioManager : FSSharedScript
    {

        private float _masterVolume = 1.0f;
        private Dictionary<SoundCategory, float> _volumes = new Dictionary<SoundCategory, float>();

        protected override void _Initialize()
        {
            if (!Game.IsHost)
            {
                DevConsole.RegisterVariable("volume.master", "", () => _masterVolume, (v) =>
                {
                    _masterVolume = Mathf.Clamp(v, 0, 1f);
                }, this, ConVarFlags.UserSetting);

                foreach (SoundCategory cat in Enum.GetValues(typeof(SoundCategory)))
                {
                    _volumes[cat] = 1.0f;
                    var name = $"volume.{cat.ToString().ToLower()}";
                    DevConsole.RegisterVariable(name, "", () => _volumes[cat], (v) =>
                    {
                        _volumes[cat] = Mathf.Clamp(v, 0, 1f);
                    }, this, ConVarFlags.UserSetting);
                }
            }
        }

        public void PlayClip(GameAudioSource src, AudioClip clip, float volume, bool stop = false)
        {
            if (Game.IsHost)
            {
                // network the sound?
                return;
            }
            if (stop)
            {
                src.Src.Stop();
            }
            var volModifier = _volumes.ContainsKey(src.Category) 
                ? _volumes[src.Category] 
                : 1.0f;
            src.Src.PlayOneShot(clip, volume * volModifier * _masterVolume);
        }
    }

    public enum SoundCategory
    {
        None,
        UI,
        Equippable,
        Player,
        Music,
        Voice,
        Effects
    }
}

