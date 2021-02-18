using Fragsurf.Maps;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class MapEntryData
    {
        public string Name;
        public Action OnClick;
        public BaseMap Map;
        public bool Selected;
    }
    public class MapEntry : EntryElement<MapEntryData>
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private Button _button;

        public BaseMap Map { get; private set; }

        private static MapEntry _selectedTab;

        public override void LoadData(MapEntryData data)
        {
            Map = data.Map;
            _name.text = data.Name;
            _button.onClick.AddListener(() =>
            {
                if (_selectedTab)
                {
                    _selectedTab._button.interactable = true;
                }
                _selectedTab = this;
                _button.interactable = false;
                data.OnClick?.Invoke();
            });
            if (data.Selected)
            {
                _button.interactable = false;
            }
        }

    }
}

