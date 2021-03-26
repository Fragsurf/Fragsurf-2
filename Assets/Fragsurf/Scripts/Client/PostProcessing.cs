using Fragsurf.Shared;
using Fragsurf.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Fragsurf.Client
{
    [RequireComponent(typeof(Volume))]
    public class PostProcessing : SingletonComponent<PostProcessing>
    {

        [ConVar("graphics.gamma", "", ConVarFlags.UserSetting)]
        public float Gamma
        {
            get => GetGamma();
            set => SetGamma(value);
        }

        private Volume _vol;

        private void Awake()
        {
            DevConsole.RegisterObject(this);
            _vol = GetComponent<Volume>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DevConsole.RemoveAll(this);
        }

        private float GetGamma()
        {
            if (!_vol
                || !_vol.profile
                || !_vol.profile.TryGet(out LiftGammaGain lgg))
            {
                return 0f;
            }
            return lgg.gamma.value.w;
        }

        private void SetGamma(float value)
        {
            if (!_vol
                || !_vol.profile
                || !_vol.profile.TryGet(out LiftGammaGain lgg))
            {
                return;
            }
            var v = lgg.gamma.value;
            v.w = Mathf.Clamp(value, -1f, 1f);
            lgg.gamma.value = v;
        }

    }
}

