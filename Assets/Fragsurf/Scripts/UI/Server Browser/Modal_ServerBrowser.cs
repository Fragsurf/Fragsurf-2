using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_ServerBrowser : UGuiModal
    {

        [SerializeField]
        private TMP_InputField _searchFilter;
        [SerializeField]
        private Toggle _hideEmpty;
        [SerializeField]
        private Toggle _hideFull;
        [SerializeField]
        private Toggle _hidePrivate;
        [SerializeField]
        private Button _refresh;
        [SerializeField]
        private Button _connect;

        private Modal_ServerBrowserPlayerEntry _playerTemplate;
        private Modal_ServerBrowserServerEntry _serverTemplate;

        private void Start()
        {
            _playerTemplate = gameObject.GetComponentInChildren<Modal_ServerBrowserPlayerEntry>();
            _playerTemplate.gameObject.SetActive(false);
            _serverTemplate = gameObject.GetComponentInChildren<Modal_ServerBrowserServerEntry>();
            _serverTemplate.gameObject.SetActive(false);

            _refresh.onClick.AddListener(RefreshServers);
            _searchFilter.onSubmit.AddListener((v) => ApplyFilters());
            _hideEmpty.onValueChanged.AddListener((v) => ApplyFilters());
            _hideFull.onValueChanged.AddListener((v) => ApplyFilters());
            _hidePrivate.onValueChanged.AddListener((v) => ApplyFilters());
            _connect.onClick.AddListener(() =>
            {
                throw new NotImplementedException();
            });
        }

        protected override void OnOpen()
        {
            RefreshServers();
        }

        public async void RefreshServers()
        {
            _playerTemplate.Clear();

            if (!SteamClient.IsValid)
            {
                return;
            }

            using (var list = new Steamworks.ServerList.Internet())
            {
                var result = await list.RunQueryAsync();
                if (!result)
                {
                    return;
                }
                _serverTemplate.Clear();
                list.Responsive.AddRange(list.Unresponsive);
                foreach(var server in list.Responsive)
                {
                    _serverTemplate.Append(new Modal_ServerBrowserServerEntry.Data()
                    {
                        Name = server.Name,
                        Gamemode = server.TagString ?? string.Empty,
                        Map = server.Map,
                        MaxPlayers = server.MaxPlayers,
                        Players = server.Players,
                        Passworded = server.Passworded,
                        Ping = server.Ping,
                        OnClick = () => { }
                    });
                }
            }

            ApplyFilters();
        }

        private void ApplyFilters()
        {

        }

    }
}

