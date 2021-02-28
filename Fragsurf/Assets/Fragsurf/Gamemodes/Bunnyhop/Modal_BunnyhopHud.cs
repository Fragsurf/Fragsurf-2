using Fragsurf.Client;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Modal_BunnyhopHud : UGuiModal
    {

        [Header("Bunnyhop HUD")]

        [SerializeField]
        private TMP_Text _centerHud;

        private string _notStartedText = "Timer not started";
        private string _format = "<color=green>{time}</color>\n<color=yellow>{speed} u/s</color>\n{jumps} jumps\n{strafes} strafes ({sync}%)";

        private void Update()
        {
            LayoutRebuilder.MarkLayoutForRebuild(_centerHud.transform.parent as RectTransform);

            var cl = FSGameLoop.GetGameInstance(false);
            if(!cl || !cl.TryGet(out SpectateController spec))
            {
                return;
            }

            var target = spec.TargetHuman;

            if (target == null || !(target.Timeline is BunnyhopTimeline bhopTimeline))
            {
                _centerHud.text = _notStartedText;
                return;
            }

            if (!bhopTimeline.RunIsLive || bhopTimeline.Frames.Count == 0)
            {
                _centerHud.text = _notStartedText;
                return;
            }

            // todo: check garbage allocations here
            var frame = bhopTimeline.CurrentFrame;
            var sb = new StringBuilder(_format);
            sb.Replace("{time}", Bunnyhop.FormatTime(frame.Time))
                .Replace("{speed}", frame.Velocity.ToString())
                .Replace("{jumps}", frame.Jumps.ToString())
                .Replace("{strafes}", frame.Strafes.ToString())
                .Replace("{sync}", frame.FinalSync.ToString())
                .Replace("{tick}", frame.Tick.ToString());

            _centerHud.text = sb.ToString();
        }

    }
}

