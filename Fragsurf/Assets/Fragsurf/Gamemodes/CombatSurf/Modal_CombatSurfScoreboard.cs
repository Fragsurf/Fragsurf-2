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

            var cl = FSGameLoop.GetGameInstance(false);
            if (cl)
            {
                cl.PlayerManager.OnPlayerChangedTeam += PlayerManager_OnPlayerChangedTeam;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            var cl = FSGameLoop.GetGameInstance(false);
            if (cl)
            {
                cl.PlayerManager.OnPlayerChangedTeam -= PlayerManager_OnPlayerChangedTeam;
            }
        }

        private void PlayerManager_OnPlayerChangedTeam(Shared.Player.BasePlayer obj)
        {
            if (IsOpen)
            {
                BuildTeams();
            }
        }

        protected override void OnOpen()
        {
            BuildTeams();
        }

        private void BuildTeams()
        {
            _teamTemplate.Clear();

            _teamTemplate.Append(new Modal_CombatSurfScoreboardTeamEntry.Data()
            {
                TeamName = string.Empty,
                TeamNumber = 1
            });

            _teamTemplate.Append(new Modal_CombatSurfScoreboardTeamEntry.Data()
            {
                TeamName = string.Empty,
                TeamNumber = 2
            });
        }

    }
}

