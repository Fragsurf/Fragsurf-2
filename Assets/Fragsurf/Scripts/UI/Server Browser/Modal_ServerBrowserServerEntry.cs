using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_ServerBrowserServerEntry : EntryElement<Modal_ServerBrowserServerEntry.Data>
    {

        public class Data
        {
            public string Name;
            public string Gamemode;
            public string Map;
            public int Players;
            public int MaxPlayers;
            public int Ping;
            public bool Passworded;
            public Action OnClick;
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

        public override void LoadData(Data data)
        {
            _name.text = data.Name;
            _desc.text = data.Gamemode;
            _map.text = data.Map;
            _players.text = $"{data.Players}/{data.MaxPlayers}";
            _ping.text = data.Ping.ToString();
            _lockIcon.gameObject.SetActive(data.Passworded);
            _button.onClick.AddListener(() => data.OnClick?.Invoke());
        }

    }
}

