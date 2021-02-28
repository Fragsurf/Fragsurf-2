using Fragsurf.Shared;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Modal_BunnyhopScoreboard : UGuiModal
    {

        [Header("Bunnyhop Scoreboard Options")]
        [SerializeField]
        private Button _replaysButton;
        [SerializeField]
        private Button _replayToolsButton;
        [SerializeField]
        private Button _ranksButton;

        private ScoreboardPlayerEntry _playerTemplate;
        private ScoreboardSpectatorEntry _specTemplate;

        private void Start()
        {
            _playerTemplate = gameObject.GetComponentInChildren<ScoreboardPlayerEntry>();
            _playerTemplate.gameObject.SetActive(false);
            _specTemplate = gameObject.GetComponentInChildren<ScoreboardSpectatorEntry>();
            _specTemplate.gameObject.SetActive(false);

            _replaysButton.onClick.AddListener(() => UGuiManager.Instance.ToggleModal<Modal_ReplayList>());
            _replayToolsButton.onClick.AddListener(() => UGuiManager.Instance.ToggleModal<Modal_ReplayTools>());
            _ranksButton.onClick.AddListener(() => UGuiManager.Instance.ToggleModal<Modal_BunnyhopRanks>());
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
            _playerTemplate.Clear();
            _specTemplate.Clear();
            foreach (var player in cl.PlayerManager.Players)
            {
                _playerTemplate.Append(new PlayerEntryData()
                {
                    Player = player
                });
            }
        }

    }
}

