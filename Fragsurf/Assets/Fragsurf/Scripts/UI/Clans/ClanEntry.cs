using Steamworks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class ClanEntryData
    {
        public Clan Clan;
        public Action OnClick;
        public bool Selected;
    }

    public class ClanEntry : EntryElement<ClanEntryData>
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private Button _button;

        private static ClanEntry _activeTab;

        public override void LoadData(ClanEntryData data)
        {
            _name.text = data.Clan.Name;
            _button.onClick.AddListener(() =>
            {
                if (_activeTab)
                {
                    _activeTab._button.interactable = true;
                }
                _activeTab = this;
                _button.interactable = false;
                data.OnClick?.Invoke();
            });

            if (data.Selected)
            {
                _button.interactable = false;
                if (_activeTab)
                {
                    _activeTab._button.interactable = true;
                }
                _activeTab = this;
            }
        }

    }
}

