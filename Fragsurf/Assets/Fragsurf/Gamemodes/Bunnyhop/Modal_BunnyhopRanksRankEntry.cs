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
        public Func<bool> DisableButtons;
    }
    public class Modal_BunnyhopRanksRankEntry : EntryElement<Modal_BunnyhopRanksRankEntryData>
    {

        [SerializeField]
        private TMP_Text _text;
        [SerializeField]
        private Button _replayButton;
        [SerializeField]
        private Button _profileButton;

        private Func<bool> _disableButtons;

        protected override bool ContainsSearch(string input)
        {
            return _text.text.IndexOf(input, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        public override void LoadData(Modal_BunnyhopRanksRankEntryData data)
        {

            var formattedTime = Bunnyhop.FormatTime(data.Entry.TimeMilliseconds);
            _text.text = $"<color=yellow>#{data.Entry.Rank}</color> {data.Entry.DisplayName} | <color=green>{formattedTime}</color> <size=12><color=orange>{data.Entry.Jumps}</color> jmp, <color=orange>{data.Entry.Strafes}</color> str {data.Entry.GetDate()}</size>";
            _replayButton.onClick.AddListener(() =>
            {
                data.OnClickReplay?.Invoke();
            });
            _profileButton.onClick.AddListener(() =>
            {
                data.OnClickProfile?.Invoke();
            });
            _disableButtons = data.DisableButtons;
        }

        private void Update()
        {
            if(_disableButtons != null)
            {
                _replayButton.interactable = !_disableButtons();
            }
        }

    }
}

