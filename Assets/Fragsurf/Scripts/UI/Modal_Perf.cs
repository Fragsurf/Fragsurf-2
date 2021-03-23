using Fragsurf.Client;
using Fragsurf.Shared;
using Fragsurf.Utility;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_Perf : UGuiModal
    {

        public const string Identifier = "Perf";

        [SerializeField]
        private TMP_Text _text;

        private Modal_MainMenu _mm;
        private RectTransform _rt;
        private Vector2 _originalAnchoredPosition;

        private void Start()
        {
            _rt = GetComponent<RectTransform>();
            _originalAnchoredPosition = _rt.anchoredPosition;
            _mm = UGuiManager.Instance.Find<Modal_MainMenu>();
        }

        private void Update()
        {
            var txt = $"{TimeStep.Instance.FPSCounter.AverageFPS} FPS";

            var cl = FSGameLoop.GetGameInstance(false);
            if (cl)
            {
                var cln = cl.Network as ClientSocketManager;
                txt = $"{txt} / {(int)(cln.AverageRoundtripTime * 1000f)} MS";
            }

            _text.text = txt;
            _rt.anchoredPosition = _mm && _mm.IsOpen ? new Vector2(64, -12) : _originalAnchoredPosition;
        }

    }
}

