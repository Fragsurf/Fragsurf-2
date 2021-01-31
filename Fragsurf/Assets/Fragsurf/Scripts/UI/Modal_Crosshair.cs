using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_Crosshair : UGuiModal
    {

        [SerializeField]
        private Image[] _crosshairImages;

        [ConCommand("crosshair.color", "Sets the crosshair color")]
        public void SetColor(string colorInput)
        {
            if(ColorUtility.TryParseHtmlString(colorInput, out Color color))
            {
                foreach(var img in _crosshairImages)
                {
                    img.color = color;
                }
            }
        }

        [ConCommand("crosshair.alpha", "Sets the crosshair alpha")]
        public void SetAlpha(float alpha)
        {
            alpha = Mathf.Clamp(alpha, 0, 1);
            foreach (var img in _crosshairImages)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
            }
        }

        [ConCommand("crosshair.outlinealpha", "Sets the crosshair outline alpha")]
        public void SetOutlineAlpha(float alpha)
        {
            alpha = Mathf.Clamp(alpha, 0, 1);
            foreach (var img in _crosshairImages)
            {
                if (img.TryGetComponent(out Outline outline))
                {
                    outline.effectColor = new Color(outline.effectColor.r, outline.effectColor.g, outline.effectColor.b, alpha);
                }
            }
        }

    }
}

