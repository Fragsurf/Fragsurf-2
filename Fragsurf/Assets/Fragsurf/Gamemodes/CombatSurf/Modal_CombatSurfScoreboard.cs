using Fragsurf.Shared;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class Modal_CombatSurfScoreboard : UGuiModal
    {

        private Modal_CombatSurfScoreboardTeamEntry _teamTemplate;

        private void Start()
        {
            _teamTemplate = gameObject.GetComponentInChildren<Modal_CombatSurfScoreboardTeamEntry>(true);
            _teamTemplate.gameObject.SetActive(false);
        }

        protected override void OnOpen()
        {
            BuildTeams();
        }

        private HashSet<int> _teams = new HashSet<int>();
        private void BuildTeams()
        {
            _teamTemplate.Clear();

            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl)
            {
                return;
            }

            _teams.Clear();
            foreach (var player in cl.PlayerManager.Players)
            {
                if (!_teams.Contains(player.Team))
                {
                    _teams.Add(player.Team);
                }
            }

            foreach(var team in _teams)
            {
                if(team == 0)
                {
                    continue;
                }
                _teamTemplate.Append(new Modal_CombatSurfScoreboardTeamEntry.Data()
                {
                    TeamName = string.Empty,
                    TeamNumber = team
                });
            }
        }

    }
}

