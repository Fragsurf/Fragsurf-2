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
        [SerializeField]
        private TMP_Text _trackName;

        private string _notStartedText = "Timer not started";
        private string _format = "<color=green>{time}</color> ({tick})\n<color=yellow>{speed} u/s</color>\n{jumps} jumps\n{strafes} strafes ({sync}%)";

        private void Update()
        {
            LayoutRebuilder.MarkLayoutForRebuild(_centerHud.transform.parent as RectTransform);

            if (Human.Local == null || !(Human.Local.Timeline is BunnyhopTimeline bhopTimeline))
            {
                _centerHud.text = _notStartedText;
                _trackName.text = string.Empty;
                return;
            }

            switch (bhopTimeline.Track.TrackType)
            {
                case Actors.FSMTrackType.Linear:
                    _trackName.text = $"CP #{bhopTimeline.Checkpoint}";
                    break;
                case Actors.FSMTrackType.Bonus:
                    _trackName.text = $"Bonus {bhopTimeline.Track.TrackName}";
                    break;
                case Actors.FSMTrackType.Staged:
                    _trackName.text = $"Stage {bhopTimeline.Stage}";
                    break;
            }

            if (!bhopTimeline.RunIsLive || bhopTimeline.Frames.Count == 0)
            {
                _centerHud.text = _notStartedText;
                return;
            }

            // todo: check garbage allocations here
            var frame = bhopTimeline.CurrentFrame;
            var sb = new StringBuilder(_format);
            sb.Replace("{time}", frame.FormattedTime())
                .Replace("{speed}", frame.Velocity.ToString())
                .Replace("{jumps}", frame.Jumps.ToString())
                .Replace("{strafes}", frame.Strafes.ToString())
                .Replace("{sync}", frame.FinalSync.ToString())
                .Replace("{tick}", frame.Tick.ToString());

            _centerHud.text = sb.ToString();
        }

    }
}

