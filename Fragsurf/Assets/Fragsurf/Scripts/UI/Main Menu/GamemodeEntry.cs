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
        public bool Selected;
    }
    public class GamemodeEntry : EntryElement<GamemodeEntryData>
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private Button _button;

        private static GamemodeEntry _activeTab;

        public override void LoadData(GamemodeEntryData data)
        {
            _name.text = data.Name;
            _button.onClick.AddListener(() =>
            {
                if (_activeTab)
                {
                    _activeTab._button.interactable = true;
                }
                _button.interactable = false;
                data.OnSelect?.Invoke();
            });
            if (data.Selected)
            {
                _button.interactable = false;
            }
        }

    }
}

