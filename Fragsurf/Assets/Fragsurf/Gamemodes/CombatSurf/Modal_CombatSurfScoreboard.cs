using UnityEngine;
using Fragsurf.Shared;
using Fragsurf.UI;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using Fragsurf.Shared.Packets;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class Modal_CombatSurfScoreboard : UGuiModal
    {

        [SerializeField]
        private Button _spectateButton;
        [SerializeField]
        private TMP_Text _spectators;

        private Modal_CombatSurfScoreboardTeamEntry _teamTemplate;

        private void Start()
        {
            _teamTemplate = gameObject.GetComponentInChildren<Modal_CombatSurfScoreboardTeamEntry>(true);
            _teamTemplate.gameObject.SetActive(false);

            _spectateButton.onClick.AddListener(() =>
            {
                var cl = FSGameLoop.GetGameInstance(false);
                if (!cl)
                {
                    return;
                }
                var chooseTeam = PacketUtility.TakePacket<ChooseTeam>();
                chooseTeam.TeamNumber = 0;
                cl.Network.BroadcastPacket(chooseTeam);
            });

            SpectateController.ScoreboardUpdateNotification += SpectateController_ScoreboardUpdateNotification;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SpectateController.ScoreboardUpdateNotification -= SpectateController_ScoreboardUpdateNotification;
        }

        private void SpectateController_ScoreboardUpdateNotification()
        {
            BuildTeams();
        }

        protected override void OnOpen()
        {
            BuildTeams();
        }

        private void BuildTeams()
        {
            _spectators.text = string.Empty;
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

            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl)
            {
                return;
            }
            var specc = cl.Get<SpectateController>();
            var specs = cl.PlayerManager.Players
                .Where(x => specc.IsSpectating(x.ClientIndex))
                .Select(x => x.DisplayName);
            _spectators.text = string.Join(", ", specs);
        }

    }
}

