using Fragsurf.UI;
using Mosframe;
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

        [SerializeField]
        private TMP_Text _trickName;
        [SerializeField]
        private Button _button;
        [SerializeField]
        private GameObject _completedCheck;

        private int _trickId;

        private void Start()
        {
            CL_TrickLog.OnNewTrickCompleted += OnTrickCompleted;

            _button.onClick.AddListener(() =>
            {
                Debug.Log("Clicked");
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
            return !string.IsNullOrEmpty(data.TrickName) && data.TrickName.ContainsInsensitive(input);
        }

        private void OnTrickCompleted(int trickId)
        {
            if(_trickId != trickId)
            {
                return;
            }
            _completedCheck.gameObject.SetActive(true);
        }

        public override void LoadData(Data data)
        {
            _trickId = data.TrickId;
            _trickName.text = data.TrickName;
            _completedCheck.SetActive(data.Completed);
        }

    }
}

