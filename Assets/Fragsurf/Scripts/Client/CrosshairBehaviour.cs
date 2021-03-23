using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Fragsurf.Client
{
    public class CrosshairBehaviour : MonoBehaviour
    {

        [SerializeField]
        private Image _crosshairImage;
        [SerializeField]
        private Outline _crosshairImageOutline;
        [SerializeField]
        private GameObject _hitmarkerObject;

        private void Start()
        {
            _hitmarkerObject.SetActive(false);
        }

        public void SetColor(Color color)
        {
            _crosshairImage.color = color;
        }

        public void SetAlpha(float a)
        {
            var c = _crosshairImage.color;
            c.a = a;
            _crosshairImage.color = c;
        }

        public void SetOutline(bool enabled)
        {
            _crosshairImageOutline.enabled = enabled;
        }

        public void Hitmarker()
        {
            //SoundManager.PlaySound2D("UI/Hitmarker", 1.0f);
            _hitmarkerObject.SetActive(true);
            Invoke("DisableHitmarker", .15f);
        }

        private void DisableHitmarker()
        {
            _hitmarkerObject.SetActive(false);
        }

    }
}

