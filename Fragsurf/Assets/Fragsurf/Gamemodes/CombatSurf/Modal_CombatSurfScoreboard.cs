using Fragsurf.Shared;
using Fragsurf.UI;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class Modal_CombatSurfScoreboard : UGuiModal
    {

        private Modal_CombatSurfScoreboardTeamEntry _teamTemplate;

        private void Start()
        {
            _teamTemplate = gameObject.GetComponentInChildren<Modal_CombatSurfScoreboardTeamEntry>(true);
            _teamTemplate.gameObject.SetActive(false);

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

