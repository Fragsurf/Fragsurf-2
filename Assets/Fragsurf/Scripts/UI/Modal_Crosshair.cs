using Fragsurf.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_Crosshair : UGuiModal
    {

        public const string Identifier = "Crosshair";

        [SerializeField]
        private GameObject _scopeOverlay;
        [SerializeField]
        private Image[] _crosshairImages;

        private SpectateController _spec;
        private Color _color;
        private float _alpha = 1;
        private float _outlineAlpha = 1;
        private bool _scoped;

        [ConVar("crosshair.color", "", ConVarFlags.UserSetting)]
        public Color Color
        {
            get => _color;
            set => SetColor(value);
        }

        [ConVar("crosshair.alpha", "", ConVarFlags.UserSetting)]
        public float Alpha
        {
            get => _alpha;
            set => SetAlpha(value);
        }

        [ConVar("crosshair.outlinealpha", "", ConVarFlags.UserSetting)]
        public float OutlineAlpha
        {
            get => _outlineAlpha;
            set => SetOutlineAlpha(value);
        }

        private bool _crosshairDisabled;
        private void DisableCrosshair(bool disabled)
        {
            if(_crosshairDisabled == disabled)
            {
                return;
            }
            foreach (var img in _crosshairImages)
            {
                img.gameObject.SetActive(!disabled);
            }
            _crosshairDisabled = disabled;
        }

        private void Start()
        {
            if (_scopeOverlay)
            {
                _scopeOverlay.gameObject.SetActive(false);
            }
        }

        private void SetColor(Color color)
        {
            _color = color;
            foreach (var img in _crosshairImages)
            {
                img.color = color;
            }
        }

        private void SetAlpha(float alpha)
        {
            _alpha = Mathf.Clamp(alpha, 0, 1);
            foreach (var img in _crosshairImages)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, _alpha);
            }
        }

        private void SetOutlineAlpha(float alpha)
        {
            _outlineAlpha = Mathf.Clamp(alpha, 0, 1);
            foreach (var img in _crosshairImages)
            {
                if (img.TryGetComponent(out Outline outline))
                {
                    outline.effectColor = new Color(outline.effectColor.r, outline.effectColor.g, outline.effectColor.b, _outlineAlpha);
                }
            }
        }

        private void Update()
        {
            if (!_scopeOverlay)
            {
                return;
            }

            CheckScope(out bool scopedIn, out bool disableCrosshair);

            if(scopedIn != _scoped)
            {
                _scopeOverlay.gameObject.SetActive(scopedIn);
                _scoped = scopedIn;
            }

            DisableCrosshair(scopedIn || disableCrosshair);
        }

        private void CheckScope(out bool scopedIn, out bool disableCrosshair)
        {
            scopedIn = false;
            disableCrosshair = false;

            if (!_spec)
            {
                var cl = FSGameLoop.GetGameInstance(false);
                if (cl)
                {
                    _spec = cl.Get<SpectateController>();
                }
            }

            if (!_spec
                || _spec.TargetHuman == null
                || _spec.TargetHuman.Equippables.Equipped == null
                || !(_spec.TargetHuman.Equippables.Equipped.EquippableGameObject is GunEquippable gun))
            {
                return;
            }

            scopedIn = !Mathf.Approximately(gun.GetMagnification(), 1f);
            disableCrosshair = gun.GunData.DisableCrosshair;
        }

    }
}

