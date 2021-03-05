using Fragsurf.Client;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;
using Fragsurf.Utility;
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
        private string _format = "{time}\n{speed} u/s\n{jumps} jumps\n{strafes} strafes ({sync}%)";

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
                SetCenterHudText(_notStartedText);
                return;
            }

            if (!bhopTimeline.RunIsLive || bhopTimeline.Frames.Count == 0)
            {
                SetCenterHudText(_notStartedText);
                return;
            }

            // todo: check garbage allocations here
            var frame = bhopTimeline.CurrentFrame;
            var sb = new StringBuilder(_format);
            var tracks = cl.Get<BunnyhopTracks>();
            sb.Replace("{time}", $"<color={tracks.TimeColor.HashRGBA()}>{Bunnyhop.FormatTime(frame.Time)}</color>")
                .Replace("{speed}", $"<color={tracks.SpeedColor.HashRGBA()}>{frame.Velocity}</color>")
                .Replace("{jumps}", $"<color={tracks.MiscColor.HashRGBA()}>{frame.Jumps}</color>")
                .Replace("{strafes}", $"<color={tracks.MiscColor.HashRGBA()}>{frame.Strafes}</color>")
                .Replace("{sync}", $"<color={tracks.MiscColor.HashRGBA()}>{frame.FinalSync}</color>")
                .Replace("{tick}", $"<color={tracks.MiscColor.HashRGBA()}>{frame.Tick}</color>");

            SetCenterHudText(sb.ToString());
        }

        private void SetCenterHudText(string text)
        {
            var tracks = FSGameLoop.GetGameInstance(false).Get<BunnyhopTracks>();
            _centerHud.text = $"<color={tracks.MessageColor.HashRGBA()}>{text}</color>";
        }

    }
}

