using Fragsurf.Shared;
using Fragsurf.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Tricksurf
{
    public class Modal_TricksurfTrickEntry : EntryElement<Modal_TricksurfTrickEntry.Data>
    {

        public class Data
        {
            public int TrickId;
            public string TrickName;
            public bool Completed;
        }

        public enum TrickFilter
        {
            All,
            Completed,
            Incomplete
        }

        [SerializeField]
        private TMP_Text _trickName;
        [SerializeField]
        private Button _button;
        [SerializeField]
        private GameObject _completedCheck;

        [SerializeField]
        private TMP_Text _trickDetailName;
        [SerializeField]
        private TMP_Text _trickDetailPath;
        [SerializeField]
        private Button _trickDetailTrack;

        private Data _data;

        public static TrickFilter Filter = TrickFilter.All;

        private void Awake()
        {
            _dynScroll = Modal_TricksurfScoreboard.DscrollTrickData;
        }

        private void Start()
        {
            CL_TrickLog.OnNewTrickCompleted += OnTrickCompleted;

            _button.onClick.AddListener(() =>
            {
                if(_data == null)
                {
                    return;
                }
                var cl = FSGameLoop.GetGameInstance(false);
                if(cl == null)
                {
                    return;
                }

                var ts = cl.Get<SH_Tricksurf>();
                if(ts == null)
                {
                    return;
                }

                var trick = ts.TrickData.GetTrick(_data.TrickId);
                if(trick == null)
                {
                    return;
                }

                var path = string.Empty;
                var idx = 1;
                foreach(var triggerId in trick.path)
                {
                    var triggerName = ts.TrickData.GetTriggerName(triggerId);
                    path += $"<b>{idx}</b> - {triggerName}, ";
                    idx++;
                }

                path = path.Substring(0, path.Length - 2);

                _trickDetailName.text = $"{_data.TrickName}\nCompleted: {_data.Completed}";
                _trickDetailPath.text = path;
                _trickDetailTrack.onClick.RemoveAllListeners();
                _trickDetailTrack.onClick.AddListener(() =>
                {
                    var sc = GameObject.FindObjectOfType<Modal_TricksurfScoreboard>();
                    if(sc == null)
                    {
                        return;
                    }
                    sc.SetTrackedTrick(_data.TrickId);
                });
            });
        }

        private void OnDestroy()
        {
            CL_TrickLog.OnNewTrickCompleted -= OnTrickCompleted;
        }

        protected override bool ContainsSearch(string input)
        {
            return _trickName.text.ContainsInsensitive(input);
        }

        protected override bool DataContainsSearch(Data data, string input)
        {
            if(!data.Completed && Filter == TrickFilter.Completed)
            {
                return false;
            }
            if(data.Completed && Filter == TrickFilter.Incomplete)
            {
                return false;
            }
            return !string.IsNullOrEmpty(data.TrickName) && data.TrickName.ContainsInsensitive(input);
        }

        private void OnTrickCompleted(int trickId)
        {
            if(_data == null || _data.TrickId != trickId)
            {
                return;
            }
            _completedCheck.gameObject.SetActive(true);
        }

        public override void LoadData(Data data)
        {
            _data = data;
            _trickName.text = data.TrickName;
            _completedCheck.SetActive(data.Completed);
        }

    }
}

