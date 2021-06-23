using Fragsurf.Shared;
using Fragsurf.UI;
using TMPro;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class BunnyhopRecordingCanvas : UGuiModal
    {

        [SerializeField]
        private Canvas _myCanvas;
        [SerializeField]
        private TMP_Text _velocity;

        protected override void OnOpen()
        {
            base.OnOpen();

            ToggleVisibility(true);
        }

        protected override void OnClose()
        {
            base.OnClose();

            ToggleVisibility(false);
        }

        private void Update()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl 
                || !cl.TryGet(out SpectateController spec)
                || spec.TargetHuman == null)
            {
                return;
            }

            _velocity.text = spec.TargetHuman.HammerVelocity().ToString();
        }

        private void ToggleVisibility(bool recorder)
        {
            foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
            {
                if (canvas == _myCanvas)
                {
                    continue;
                }
                if (!canvas.TryGetComponent(out CanvasGroup cg))
                {
                    cg = canvas.gameObject.AddComponent<CanvasGroup>();
                }
                cg.alpha = recorder ? 0 : 1;
            }
        }

    }
}

