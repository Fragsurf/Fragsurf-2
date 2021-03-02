using Fragsurf.Shared;
using Fragsurf.Shared.Player;
using System.Collections;
using UnityEngine;

namespace Fragsurf.Client
{
    [Inject(InjectRealm.Client)]
    public class Underwater : FSSharedScript
    {

        private SpectateController _spec;
        private bool _underwaterSet;
        private bool _defaultFog;
        private Color32 _defaultFogColor;
        private float _defaultFogDensity;
        private float _defaultFogStartDistance;
        private float _defaultFogEndDistance;
        private FogMode _defaultFogMode;
        private AudioSource _audioSrc;
        private float _targetVolume;

        protected override void _Initialize()
        {
            _audioSrc = gameObject.AddComponent<AudioSource>();
            _audioSrc.spatialBlend = 0f;
            _audioSrc.loop = true;
            _audioSrc.clip = GameData.Instance.UnderwaterSound;
            _audioSrc.volume = .85f;
        }

        protected override void _Update()
        {
            if(_spec == null) 
            {
                _spec = Game.Get<SpectateController>();
            }

            if(_spec == null || _spec.TargetHuman == null)
            {
                SetUnderwater(false);
                return;
            }

            if(!(_spec.TargetHuman.MovementController is DefaultMovementController move))
            {
                SetUnderwater(false);
                return;
            }

            SetUnderwater(move.MoveData.WaterDepth > 0.83f);
        }

        protected override void OnGameLoaded()
        {
            _defaultFog = RenderSettings.fog;
            _defaultFogColor = RenderSettings.fogColor;
            _defaultFogDensity = RenderSettings.fogDensity;
            _defaultFogMode = RenderSettings.fogMode;
            _defaultFogStartDistance = RenderSettings.fogStartDistance;
            _defaultFogEndDistance = RenderSettings.fogEndDistance;
        }

        private void SetUnderwater(bool under)
        {
            if(under == _underwaterSet)
            {
                return;
            }

            _underwaterSet = under;

            if (under)
            {
                StopAllCoroutines();
                StartCoroutine(FogOn());
                RenderSettings.fogColor = new Color32(37, 59, 89, 255);
                RenderSettings.fog = true;
                RenderSettings.fogDensity = 0.15f;
                RenderSettings.fogMode = FogMode.Exponential;
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(FogOff());
                RenderSettings.fog = _defaultFog;
                RenderSettings.fogColor = _defaultFogColor;
                RenderSettings.fogDensity = _defaultFogDensity;
                RenderSettings.fogMode = _defaultFogMode;
                RenderSettings.fogStartDistance = _defaultFogStartDistance;
                RenderSettings.fogEndDistance = _defaultFogEndDistance;
            }
        }

        private IEnumerator FogOn()
        {
            _audioSrc.volume = 0;
            _audioSrc.Play();
            while (_audioSrc.volume < .85f)
            {
                _audioSrc.volume += Time.deltaTime;
                yield return 0;
            }
        }

        private IEnumerator FogOff()
        {
            if (GameData.Instance.ExitWaterSound)
            {
                _audioSrc.PlayOneShot(GameData.Instance.ExitWaterSound);
            }
            while (_audioSrc.volume > 0)
            {
                _audioSrc.volume -= Time.deltaTime;
                yield return 0;
            }
            _audioSrc.Stop();
        }

    }
}

