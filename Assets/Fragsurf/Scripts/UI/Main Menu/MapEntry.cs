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
        public bool IsNew;
    }
    public class MapEntry : EntryElement<MapEntryData>
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private TMP_Text _mountedGame;
        [SerializeField]
        private Button _button;
        [SerializeField]
        private GameObject _new;

        public BaseMap Map { get; private set; }

        private static MapEntry _selectedTab;

        public override void LoadData(MapEntryData data)
        {
            Map = data.Map;
            _name.text = data.Name;
            _mountedGame.text = data.Map.MountedGame ?? string.Empty;
            _new.SetActive(data.IsNew);
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

