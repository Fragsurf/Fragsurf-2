using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

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

        private AudioSource _src;

        private void Start()
        {
            _src = GetComponent<AudioSource>();
            _src.spatialize = false;
            _src.spatialBlend = 0;
            _src.volume = 1f;
            _src.loop = false;

            StartCoroutine(SetPan());
        }

        private IEnumerator SetPan()
        {
            yield return new WaitForEndOfFrame();
            if (TryGetComponent(out RectTransform rt))
            {
                var arr = new Vector3[4];
                rt.GetWorldCorners(arr);
                var centerx = (arr[0].x + arr[3].x) / 2f;
                if (centerx == 0)
                {
                    yield break;
                }
                var tx = centerx / Screen.width;
                _src.panStereo = Mathf.Lerp(-0.75f, 0.75f, tx);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!_hover)
            {
                return;
            }
            _src.PlayOneShot(_hover, Random.Range(0.9f, 1f));
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!_exit)
            {
                return;
            }
            _src.PlayOneShot(_exit, Random.Range(0.9f, 1f));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_click)
            {
                return;
            }
            _src.PlayOneShot(_click, Random.Range(0.9f, 1f));
        }
    }
}

