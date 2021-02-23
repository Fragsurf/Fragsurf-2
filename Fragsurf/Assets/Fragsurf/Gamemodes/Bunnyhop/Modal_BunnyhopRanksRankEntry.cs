using Fragsurf.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Modal_BunnyhopRanksRankEntryData
    {
        public LeaderboardEntry Entry;
        public Action OnClickReplay;
        public Action OnClickProfile;
    }
    public class Modal_BunnyhopRanksRankEntry : EntryElement<Modal_BunnyhopRanksRankEntryData>
    {

        [SerializeField]
        private TMP_Text _text;
        [SerializeField]
        private Button _replayButton;
        [SerializeField]
        private Button _profileButton;

        public override void LoadData(Modal_BunnyhopRanksRankEntryData data)
        {

            var formattedTime = Bunnyhop.FormatTime(data.Entry.TimeMilliseconds);
            _text.text = $"<color=yellow>#{data.Entry.Rank}</color> {data.Entry.UserName} | <color=green>{formattedTime}</color>, {data.Entry.Jumps} jumps, {data.Entry.Strafes} strafes";
            _replayButton.onClick.AddListener(() =>
            {
                data.OnClickReplay?.Invoke();
            });
            _profileButton.onClick.AddListener(() =>
            {
                data.OnClickProfile?.Invoke();
            });

        }
    }
}

