using Fragsurf.Shared;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Modal_TricksurfScoreboard : UGuiModal
    {

        [Header("Tricksurf Scoreboard Options")]
        [SerializeField]
        private Button _playersButton;
        [SerializeField]
        private Button _tricksButton;

        private ScoreboardPlayerEntry _playerTemplate;
        private ScoreboardSpectatorEntry _specTemplate;

        private void Start()
        {
            _playerTemplate = gameObject.GetComponentInChildren<ScoreboardPlayerEntry>();
            _playerTemplate.gameObject.SetActive(false);
            _specTemplate = gameObject.GetComponentInChildren<ScoreboardSpectatorEntry>();
            _specTemplate.gameObject.SetActive(false);

            _playersButton.onClick.AddListener(() =>
            {
                // switch to players tab
            });

            _tricksButton.onClick.AddListener(() =>
            {
                // switch to players tab
            });

            SpectateController.ScoreboardUpdateNotification += SpectateController_ScoreboardUpdateNotification;
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

