using Fragsurf.Shared.Player;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class Modal_CombatSurfScoreboardPlayerEntry : EntryElement<Modal_CombatSurfScoreboardPlayerEntry.Data>
    {

        public class Data 
        {
            public IPlayer Player;
        }

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private TMP_Text _score;
        [SerializeField]
        private SteamAvatar _steamAvatar;

        public override void LoadData(Data data)
        {
            _name.text = data.Player.DisplayName;
            _score.text = $"128ms  <color=green>0</color> kills  <color=red>0</color> deaths";
            if (_steamAvatar)
            {
                _steamAvatar.SteamId = data.Player.SteamId;
                _steamAvatar.Fetch();
            }
        }

    }
}

