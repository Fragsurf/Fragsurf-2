using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_ServerBrowserServerEntry : EntryElement<Modal_ServerBrowserServerEntry.Data>, IPointerClickHandler
    {

        public class Data
        {
            public string Address;
            public int Port;
            public string Name;
            public string Gamemode;
            public string Map;
            public int Players;
            public int MaxPlayers;
            public int Ping;
            public bool Passworded;
            public Action OnClick;
            public Action OnDoubleClick;
        }

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private TMP_Text _desc;
        [SerializeField]
        private TMP_Text _map;
        [SerializeField]
        private TMP_Text _players;
        [SerializeField]
        private TMP_Text _ping;
        [SerializeField]
        private GameObject _lockIcon;
        [SerializeField]
        private Button _button;

        private Action _onClick;
        private Action _onDoubleClick;
        private float _lastClickTime;
        private static Modal_ServerBrowserServerEntry _selectedEntry;

        public override void LoadData(Data data)
        {
            _name.text = data.Name;
            _desc.text = data.Gamemode;
            _map.text = data.Map;
            _players.text = $"{data.Players}/{data.MaxPlayers}";
            _ping.text = data.Ping.ToString();
            _lockIcon.gameObject.SetActive(data.Passworded);
            _onClick = data.OnClick;
            _onDoubleClick = data.OnDoubleClick;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_selectedEntry)
            {
                _selectedEntry._button.interactable = true;
                _selectedEntry = null;
            }
            _selectedEntry = this;
            _selectedEntry._button.interactable = false;

            _onClick?.Invoke();
            if (Time.realtimeSinceStartup - _lastClickTime <= .25f)
            {
                _onDoubleClick?.Invoke();
            }
            _lastClickTime = Time.realtimeSinceStartup;
        }
    }
}

