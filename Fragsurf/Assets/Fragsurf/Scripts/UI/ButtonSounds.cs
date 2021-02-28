using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class ButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {

        [SerializeField]
        private AudioClip _click;
        [SerializeField]
        private AudioClip _hover;
        [SerializeField]
        private AudioClip _exit;

        private Button _btn;
        private AudioSource _src;

        private void Start()
        {
            _btn = GetComponent<Button>();
            _src = GetComponent<AudioSource>();
            _src.spatialize = false;
            _src.spatialBlend = 0;
            _src.volume = 1f;
            _src.loop = false;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!_hover || (_btn && !_btn.interactable))
            {
                return;
            }
            SetPan();
            _src.PlayOneShot(_hover, Random.Range(0.9f, 1f));
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!_exit || (_btn && !_btn.interactable))
            {
                return;
            }
            SetPan();
            _src.PlayOneShot(_exit, Random.Range(0.9f, 1f));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_click || (_btn && !_btn.interactable))
            {
                return;
            }
            SetPan();
            _src.PlayOneShot(_click, Random.Range(0.9f, 1f));
        }

        private void SetPan()
        {
            var px = Input.mousePosition.x / Screen.width;
            _src.panStereo = Mathf.Lerp(-0.75f, 0.75f, px);
        }

    }
}

