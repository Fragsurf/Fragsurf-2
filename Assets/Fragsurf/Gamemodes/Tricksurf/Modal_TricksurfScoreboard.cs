using Fragsurf.Gamemodes.Bunnyhop;
using Fragsurf.Shared;
using Fragsurf.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Tricksurf
{
    public class Modal_TricksurfScoreboard : UGuiModal
    {

        public static int TrackedTrick = -1;

        [Header("Tricksurf Scoreboard Options")]
        [SerializeField]
        private Button _allTricks;
        [SerializeField]
        private Button _completedTricks;
        [SerializeField]
        private Button _incompleteTricks;
        [SerializeField]
        private Button _untrackButton;
        [SerializeField]
        private TMP_Text _activeTrickText;

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

            SpectateController.ScoreboardUpdateNotification += SpectateController_ScoreboardUpdateNotification;

            _allTricks.onClick.AddListener(() =>
            {
                Modal_TricksurfTrickEntry.Filter = Modal_TricksurfTrickEntry.TrickFilter.All;
                _trickTemplate.SearchField.onValueChanged.Invoke(_trickTemplate.SearchField.text);
            });

            _completedTricks.onClick.AddListener(() =>
            {
                Modal_TricksurfTrickEntry.Filter = Modal_TricksurfTrickEntry.TrickFilter.Completed;
                _trickTemplate.SearchField.onValueChanged.Invoke(_trickTemplate.SearchField.text);
            });

            _incompleteTricks.onClick.AddListener(() =>
            {
                Modal_TricksurfTrickEntry.Filter = Modal_TricksurfTrickEntry.TrickFilter.Incomplete;
                _trickTemplate.SearchField.onValueChanged.Invoke(_trickTemplate.SearchField.text);
            });

            _untrackButton.onClick.AddListener(() =>
            {
                SetTrackedTrick(-1);
            });

            var ts = FSGameLoop.GetGameInstance(false).Get<SH_Tricksurf>();
            ts.OnTricksLoaded += LoadTricks;
            LoadTricks(ts.TrickData);
        }

        public void SetTrackedTrick(int trickId)
        {
            TrackedTrick = trickId;

            var cl = FSGameLoop.GetGameInstance(false);
            var t = cl.Get<SH_Tricksurf>().TrickData.GetTrick(trickId);
            if (trickId == -1 || t == null)
            {
                _untrackButton.interactable = false;
                _activeTrickText.text = string.Empty;
                return;
            }

            _untrackButton.interactable = true;
            _activeTrickText.text = t.name;
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

