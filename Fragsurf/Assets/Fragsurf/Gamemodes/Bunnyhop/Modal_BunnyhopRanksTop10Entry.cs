using Fragsurf.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{

    public class Modal_BunnyhopRanksTop10EntryData
    {
        public Top10Entry Entry;
    }

    public class Modal_BunnyhopRanksTop10Entry : EntryElement<Modal_BunnyhopRanksTop10EntryData>
    {

        [SerializeField]
        private TMP_Text _text;

        protected override bool ContainsSearch(string input)
        {
            return _text.text.IndexOf(input, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        public override void LoadData(Modal_BunnyhopRanksTop10EntryData data)
        {
            var time = Bunnyhop.FormatTime(data.Entry.TimeMilliseconds);
            var improvement = Bunnyhop.FormatTime(data.Entry.ImprovementMilliseconds);

            _text.text = $"<color=yellow>Rank {data.Entry.NewRank}</color> <- <color=yellow>Rank {data.Entry.OldRank}</color> | {data.Entry.DisplayName} | <color=green>{time}s</color>/<color=red>-{improvement}s</color>, {data.Entry.Jumps} jumps, {data.Entry.Strafes} strafes | {data.Entry.GetDate()}";
        }

    }
}
