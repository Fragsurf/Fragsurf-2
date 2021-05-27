using Fragsurf.Gamemodes.Bunnyhop;
using Fragsurf.Shared;
using Fragsurf.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Tricksurf
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
        private Modal_TricksurfTrickEntry _trickTemplate;

        private void Start()
        {
            _playerTemplate = gameObject.GetComponentInChildren<ScoreboardPlayerEntry>(true);
            _playerTemplate.gameObject.SetActive(false);
            _specTemplate = gameObject.GetComponentInChildren<ScoreboardSpectatorEntry>(true);
            _specTemplate.gameObject.SetActive(false);
            _trickTemplate = gameObject.GetComponentInChildren<Modal_TricksurfTrickEntry>(true);
            //_trickTemplate.gameObject.SetActive(false);
            _trickTemplate.EntryLimit = 99999;

            _playersButton.onClick.AddListener(() =>
            {
                // switch to players tab
            });

            _tricksButton.onClick.AddListener(() =>
            {
                // switch to players tab
            });

            SpectateController.ScoreboardUpdateNotification += SpectateController_ScoreboardUpdateNotification;

            var ts = FSGameLoop.GetGameInstance(false).Get<SH_Tricksurf>();
            ts.OnTricksLoaded += LoadTricks;
            LoadTricks(ts.TrickData);
        }

        private void LoadTricks(TrickData trickData)
        {
            _trickTemplate.Clear();

            foreach (var trick in trickData.tricks)
            {
                _trickTemplate.Append(new Modal_TricksurfTrickEntry.Data()
                {
                    TrickId = trick.id,
                    TrickName = trick.name,
                    Completed = CL_TrickLog.IsTrickCompleted(trick.id)
                });
            }
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

