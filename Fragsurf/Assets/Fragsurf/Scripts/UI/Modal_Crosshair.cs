using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_Crosshair : UGuiModal
    {

        public const string Identifier = "Crosshair";

        [SerializeField]
        private Image[] _crosshairImages;

        private Color _color;
        private float _alpha;
        private float _outlineAlpha;

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

    }
}

