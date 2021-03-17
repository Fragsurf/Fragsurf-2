using UnityEngine;
using Fragsurf.Shared.Player;
using Fragsurf.UI;
using TMPro;
using UnityEngine.UI;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared;
using Fragsurf.Client;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class PlayerEntryData
    {
        public BasePlayer Player;
    }

    public class ScoreboardPlayerEntry : EntryElement<PlayerEntryData>
    {

        [SerializeField]
        private TMP_Text _nameText;
        [SerializeField]
        private TMP_Text _activityText;
        [SerializeField]
        private Button _specButton;
        [SerializeField]
        private SteamAvatar _avatar;

        private BasePlayer _playerRef;

        public override void LoadData(PlayerEntryData data)
        {
            _playerRef = data.Player;
            _nameText.text = data.Player.DisplayName;
            _activityText.text = string.Empty;

            _specButton.onClick.AddListener(() =>
            {
                if(_playerRef != null && _playerRef.Entity != null && _playerRef.Entity is Human hu)
                {
                    var cl = FSGameLoop.GetGameInstance(false);
                    cl.Get<SpectateController>().TargetHuman = hu;
                }
            });

            if (_avatar && data.Player.SteamId != 0)
            {
                _avatar.SteamId = data.Player.SteamId;
                _avatar.Fetch();
            }
        }

        private void Update()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if (_playerRef != null 
                && _playerRef.Entity == cl.Get<SpectateController>().TargetHuman)
            {
                _specButton.interactable = false;
            }
            else
            {
                _specButton.interactable = true;
            }

            if (_playerRef == null 
                || _playerRef.Entity == null 
                || !(_playerRef.Entity.Timeline is BunnyhopTimeline bhop)
                || !bhop.Track)
            {
                _activityText.text = string.Empty;
                return;
            }

            var trackName = bhop.Track.TrackType == Actors.FSMTrackType.Bonus
                ? "Bonus"
                : "Main";
            var frame = bhop.LastFrame;
            _activityText.text = $"[{trackName}] {Bunnyhop.FormatTime(bhop.LastFrame.Time)}s / {frame.Jumps} jumps / {frame.Strafes} strafes";
        }

    }
}

