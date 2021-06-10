using Fragsurf.Shared;
using Fragsurf.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Tricksurf
{
    public class Modal_Trickbook : UGuiModal
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

        private Modal_TricksurfTrickEntry _trickTemplate;

        public static DynamicScrollDataContainer<Modal_TricksurfTrickEntry.Data> DscrollTrickData;

        protected override void Awake()
        {
            base.Awake();

            DscrollTrickData = new DynamicScrollDataContainer<Modal_TricksurfTrickEntry.Data>();
        }

        private void Start()
        {
            _trickTemplate = gameObject.GetComponentInChildren<Modal_TricksurfTrickEntry>(true);
            //_trickTemplate.gameObject.SetActive(false);
            _trickTemplate.EntryLimit = 99999;

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

    }
}

