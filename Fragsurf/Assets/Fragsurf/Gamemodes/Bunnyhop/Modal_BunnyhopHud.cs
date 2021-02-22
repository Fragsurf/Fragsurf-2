using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;
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
        private string _format = "<color=green>{time}</color>\n<color=yellow>{speed} u/s</color>\n{jumps} jumps\n{strafes} strafes";

        private void Update()
        {
            LayoutRebuilder.MarkLayoutForRebuild(_centerHud.transform.parent as RectTransform);

            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl || Human.Local == null)
            {
                _centerHud.text = _notStartedText;
                _trackName.text = string.Empty;
                return;
            }

            var rd = cl.Get<BunnyhopTracks>().GetRunData(Human.Local);

            if(rd != null)
            {
                switch (rd.FSMTrack.TrackType)
                {
                    case Actors.FSMTrackType.Linear:
                        _trackName.text = $"CP #{rd.Checkpoint + 1}";
                        break;
                    case Actors.FSMTrackType.Bonus:
                        _trackName.text = $"Bonus {rd.FSMTrack.TrackName}";
                        break;
                    case Actors.FSMTrackType.Staged:
                        _trackName.text = $"Stage {rd.Stage + 1}";
                        break;
                }

                if (!rd.TimelineTrack.Live)
                {
                    _centerHud.text = _notStartedText;
                    return;
                }
            }
            else
            {
                _centerHud.text = _notStartedText;
                _trackName.text = string.Empty;
                return;
            }

            var frame = rd.TimelineTrack.CurrentFrame;
            var txt = _format.Replace("{time}", frame.FormattedTime())
                .Replace("{speed}", frame.Velocity.ToString())
                .Replace("{jumps}", frame.Jumps.ToString())
                .Replace("{strafes}", frame.Strafes.ToString());

            _centerHud.text = txt;            
        }

    }
}

