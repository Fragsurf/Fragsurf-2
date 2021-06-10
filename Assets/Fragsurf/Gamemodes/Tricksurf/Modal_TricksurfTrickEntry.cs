using Fragsurf.Shared;
using Fragsurf.UI;
using System.Collections.Generic;
using System.Linq;
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
            public int Points;
            public int PathLength;
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

        public enum TrickSort
        {
            Default,
            MostPoints,
            LeastPoints,
            LongestPath,
            ShortestPath
        }

        [SerializeField]
        private TMP_Text _trickName;
        [SerializeField]
        private Button _button;
        [SerializeField]
        private GameObject _completedCheck;
        [SerializeField]
        private TMP_Text _pointsText;

        [SerializeField]
        private TMP_Text _trickDetailName;
        [SerializeField]
        private TMP_Text _trickDetailPath;
        [SerializeField]
        private Button _trickDetailTrack;

        private Data _data;

        public static TrickFilter Filter = TrickFilter.All;
        public static TrickSort Sort = TrickSort.Default;

        private void Awake()
        {
            _dynScroll = Modal_Trickbook.DscrollTrickData;
        }

        private void Start()
        {
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
                    var tb = GameObject.FindObjectOfType<Modal_Trickbook>();
                    if(tb == null)
                    {
                        return;
                    }
                    tb.SetTrackedTrick(_data.TrickId);
                });
            });
        }

        protected override void SortData(List<Data> data)
        {
            var newData = new List<Data>();
            switch(Sort)
            {
                case TrickSort.Default:
                    return;
                case TrickSort.MostPoints:
                    newData = data.OrderByDescending(x => x.Points).ToList();
                    break;
                case TrickSort.LeastPoints:
                    newData = data.OrderBy(x => x.Points).ToList();
                    break;
                case TrickSort.LongestPath:
                    newData = data.OrderByDescending(x => x.PathLength).ToList();
                    break;
                case TrickSort.ShortestPath:
                    newData = data.OrderBy(x => x.PathLength).ToList();
                    break;
            }
            data.Clear();
            data.AddRange(newData);
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

        public override void LoadData(Data data)
        {
            _data = data;
            _trickName.text = data.TrickName;
            _completedCheck.SetActive(data.Completed);
            _pointsText.text = $"+{data.Points} points"; 
        }

    }
}

