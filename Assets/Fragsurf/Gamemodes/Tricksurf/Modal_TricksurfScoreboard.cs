using UnityEngine;
using Fragsurf.Gamemodes.Bunnyhop;
using Fragsurf.Shared;
using Fragsurf.UI;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Tricksurf
{
    public class Modal_TricksurfScoreboard : UGuiModal
    {

        [SerializeField]
        private Button _trickbookButton;

        private ScoreboardPlayerEntry _playerTemplate;
        private ScoreboardSpectatorEntry _specTemplate;

        private void Start()
        {
            SpectateController.ScoreboardUpdateNotification += SpectateController_ScoreboardUpdateNotification;

            _playerTemplate = gameObject.GetComponentInChildren<ScoreboardPlayerEntry>(true);
            _playerTemplate.gameObject.SetActive(false);
            _specTemplate = gameObject.GetComponentInChildren<ScoreboardSpectatorEntry>(true);
            _specTemplate.gameObject.SetActive(false);

            _trickbookButton.onClick.AddListener(() =>
            {
                UGuiManager.Instance.ToggleModal("Trickbook");
            });
        }

        protected override void OnDestroy()
        {
            SpectateController.ScoreboardUpdateNotification -= SpectateController_ScoreboardUpdateNotification;

            base.OnDestroy();
        }

        private void SpectateController_ScoreboardUpdateNotification()
        {
            if (IsOpen)
            {
                RefreshPlayerList();
            }
        }

        protected override void OnOpen()
        {
            RefreshPlayerList();
        }

        private void RefreshPlayerList()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl)
            {
                return;
            }

            var specController = cl.Get<SpectateController>();

            _playerTemplate.Clear();
            _specTemplate.Clear();
            foreach (var player in cl.PlayerManager.Players)
            {
                var plData = new PlayerEntryData()
                {
                    Player = player
                };
                if (!specController.IsSpectating(player.ClientIndex))
                {
                    _playerTemplate.Append(plData);
                }
                else
                {
                    _specTemplate.Append(plData);
                }
            }
        }

    }
}

