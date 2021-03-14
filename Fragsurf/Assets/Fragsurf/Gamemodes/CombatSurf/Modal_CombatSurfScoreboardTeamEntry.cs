using Fragsurf.Shared;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class Modal_CombatSurfScoreboardTeamEntry : EntryElement<Modal_CombatSurfScoreboardTeamEntry.Data>
    {

        public class Data
        {
            public string TeamName;
            public int TeamNumber;
        }

        [SerializeField]
        private TMP_Text _teamName;

        private Modal_CombatSurfScoreboardPlayerEntry _playerTemplate;

        public override void LoadData(Data data)
        {
            _playerTemplate = gameObject.GetComponentInChildren<Modal_CombatSurfScoreboardPlayerEntry>(true);
            _playerTemplate.gameObject.SetActive(false);
            _teamName.text = $"Team {data.TeamNumber}";
            LoadPlayers(data.TeamNumber);
        }

        private void LoadPlayers(int teamNumber)
        {
            _playerTemplate.Clear();

            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl)
            {
                return;
            }

            foreach(var player in cl.PlayerManager.Players)
            {
                if(player.Team == teamNumber)
                {
                    _playerTemplate.Append(new Modal_CombatSurfScoreboardPlayerEntry.Data()
                    {
                        Player = player
                    });
                }
            }
        }

    }
}

