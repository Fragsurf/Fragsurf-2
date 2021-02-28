using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class ScoreboardSpectatorEntry : EntryElement<PlayerEntryData>
    {

        [SerializeField]
        private TMP_Text _name;

        public override void LoadData(PlayerEntryData data)
        {
            _name.text = data.Player.DisplayName;
        }

    }
}
