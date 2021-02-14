using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class GamemodeEntryData
    {
        public string Name;
        public Action OnSelect;
    }
    public class GamemodeEntry : EntryElement<GamemodeEntryData>
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private Button _button; 

        public override void LoadData(GamemodeEntryData data)
        {
            _name.text = data.Name;
            if (_button)
            {
                _button.onClick.AddListener(() =>
                {
                    data.OnSelect?.Invoke();
                });
            }
        }

    }
}

