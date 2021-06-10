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
        [SerializeField]
        private TMP_Dropdown _sortDropdown;

        private Modal_TricksurfTrickEntry _trickTemplate;
        private Button _activeBtn;

        public static DynamicScrollDataContainer<Modal_TricksurfTrickEntry.Data> DscrollTrickData;

        protected override void Awake()
        {
            base.Awake();

            DscrollTrickData = new DynamicScrollDataContainer<Modal_TricksurfTrickEntry.Data>();
        }

        private void SetActiveBtn(Button btn)
        {
            if(_activeBtn)
            {
                _activeBtn.interactable = true;
                _activeBtn = null;
            }
            if(btn)
            {
                btn.interactable = false;
                _activeBtn = btn;
            }
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

                SetActiveBtn(_allTricks);
            });

            _completedTricks.onClick.AddListener(() =>
            {
                Modal_TricksurfTrickEntry.Filter = Modal_TricksurfTrickEntry.TrickFilter.Completed;
                _trickTemplate.SearchField.onValueChanged.Invoke(_trickTemplate.SearchField.text);

                SetActiveBtn(_completedTricks);
            });

            _incompleteTricks.onClick.AddListener(() =>
            {
                Modal_TricksurfTrickEntry.Filter = Modal_TricksurfTrickEntry.TrickFilter.Incomplete;
                _trickTemplate.SearchField.onValueChanged.Invoke(_trickTemplate.SearchField.text);

                SetActiveBtn(_incompleteTricks);
            });

            _untrackButton.onClick.AddListener(() =>
            {
                SetTrackedTrick(-1);
            });

            _sortDropdown.onValueChanged.AddListener((x) =>
            {
                var v = _sortDropdown.options[x].text;
                switch(v.ToLower())
                {
                    case "default":
                        Modal_TricksurfTrickEntry.Sort = Modal_TricksurfTrickEntry.TrickSort.Default;
                        break;
                    case "most points":
                        Modal_TricksurfTrickEntry.Sort = Modal_TricksurfTrickEntry.TrickSort.MostPoints;
                        break;
                    case "least points":
                        Modal_TricksurfTrickEntry.Sort = Modal_TricksurfTrickEntry.TrickSort.LeastPoints;
                        break;
                    case "longest path":
                        Modal_TricksurfTrickEntry.Sort = Modal_TricksurfTrickEntry.TrickSort.LongestPath;
                        break;
                    case "shortest path":
                        Modal_TricksurfTrickEntry.Sort = Modal_TricksurfTrickEntry.TrickSort.ShortestPath;
                        break;
                }
                _trickTemplate.SearchField.onValueChanged.Invoke(_trickTemplate.SearchField.text);
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
                    Completed = CL_TrickLog.IsTrickCompleted(trick.id),
                    Points = trick.points,
                    PathLength = trick.path.Count
                });
            }
        }

    }
}

