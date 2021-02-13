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
        }

    }
}

