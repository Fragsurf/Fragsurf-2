using Fragsurf.Client;
using Fragsurf.Shared;
using Steamworks;
using Steamworks.Data;
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

        public enum ServerType
        {
            Official,
            Community,
            Lobby
        }

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
        [SerializeField]
        private Button _officialTab;
        [SerializeField]
        private Button _communityTab;
        [SerializeField]
        private Button _lobbyTab;
        [SerializeField]
        private Button _emptyButton;

        [SerializeField]
        private ButtonTabs _mainnavTabs;
        [SerializeField]
        private Button _createGameTabButton;

        private Modal_ServerBrowserPlayerEntry _playerTemplate;
        private Modal_ServerBrowserServerEntry _serverTemplate;
        private ServerType _selectedType;
        private Button _selectedTypeTab;

        private void Start()
        {
            _playerTemplate = gameObject.GetComponentInChildren<Modal_ServerBrowserPlayerEntry>(true);
            _playerTemplate.gameObject.SetActive(false);
            _serverTemplate = gameObject.GetComponentInChildren<Modal_ServerBrowserServerEntry>(true);
            _serverTemplate.gameObject.SetActive(false);
            _emptyButton.gameObject.SetActive(false);

            _refresh.onClick.AddListener(() => RefreshServers(_selectedType));
            _searchFilter.onSubmit.AddListener((v) => ApplyFilters());
            _hideEmpty.onValueChanged.AddListener((v) => ApplyFilters());
            _hideFull.onValueChanged.AddListener((v) => ApplyFilters());
            _hidePrivate.onValueChanged.AddListener((v) => ApplyFilters());
            _connect.onClick.AddListener(JoinSelectedServer);

            _emptyButton.onClick.AddListener(() =>
            {
                _mainnavTabs.SetActiveTab(_createGameTabButton);
            });

            _officialTab.onClick.AddListener(() =>
            {
                SetServerType(_officialTab, ServerType.Official);
            });

            _communityTab.onClick.AddListener(() =>
            {
                SetServerType(_communityTab, ServerType.Community);
            });

            _lobbyTab.onClick.AddListener(() =>
            {
                SetServerType(_lobbyTab, ServerType.Lobby);
            });

            SetServerType(_lobbyTab, ServerType.Lobby);
        }

        private void SetServerType(Button btn, ServerType type)
        {
            _selectedType = type;
            if (_selectedTypeTab)
            {
                _selectedTypeTab.interactable = true;
            }
            _selectedTypeTab = btn;
            btn.interactable = false;
            RefreshServers(type);
        }

        protected override void OnOpen()
        {
            RefreshServers(_selectedType);
        }

        private Modal_ServerBrowserServerEntry.Data _selectedServer;

        public async void RefreshServers(ServerType type)
        {
            _selectedServer = null;
            _playerTemplate.Clear();
            _serverTemplate.Clear();

            if (!SteamClient.IsValid)
            {
                return;
            }

            bool switched() => _selectedType != type;

            switch (type)
            {
                case ServerType.Official:
                case ServerType.Community:
                    using (var list = new Steamworks.ServerList.Internet())
                    {
                        var result = await list.RunQueryAsync();
                        if (!result || switched())
                        {
                            break;
                        }
                        _serverTemplate.Clear();
                        foreach (var server in list.Responsive)
                        {
                            var data = new Modal_ServerBrowserServerEntry.Data()
                            {
                                Address = server.Address.ToString(),
                                Port = server.ConnectionPort,
                                Name = server.Name,
                                Gamemode = server.TagString ?? string.Empty,
                                Map = server.Map,
                                MaxPlayers = server.MaxPlayers,
                                Players = server.Players,
                                Passworded = server.Passworded,
                                Ping = server.Ping
                            };
                            data.OnClick = () => _selectedServer = data;
                            data.OnDoubleClick = JoinSelectedServer;
                            _serverTemplate.Append(data);
                        }
                    }
                    break;
                case ServerType.Lobby:
                    var lobbies = await SteamMatchmaking.LobbyList.RequestAsync();
                    if (lobbies == null || switched())
                    {
                        break;
                    }
                    _serverTemplate.Clear();
                    foreach (var lobby in lobbies)
                    {
                        uint ip = 0;
                        ushort port = 0;
                        SteamId steamid = default;
                        lobby.GetGameServer(ref ip, ref port, ref steamid);
                        var data = new Modal_ServerBrowserServerEntry.Data()
                        {
                            Address = steamid.ToString(),
                            Port = port,
                            Name = GetLobbyData(lobby, "name"),
                            Gamemode = GetLobbyData(lobby, "gamemode"),
                            Map = GetLobbyData(lobby, "map"),
                            MaxPlayers = GetLobbyDataInt(lobby, "maxplayers"),
                            Players = GetLobbyDataInt(lobby, "players"),
                            Passworded = GetLobbyData(lobby, "password") != string.Empty,
                            Ping = 0
                        };
                        data.OnClick = () => _selectedServer = data;
                        data.OnDoubleClick = JoinSelectedServer;
                        _serverTemplate.Append(data);
                    }
                    break;
            }

            ApplyFilters();
        }

        private void JoinSelectedServer()
        {
            if(_selectedServer == null)
            {
                return;
            }

            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl)
            {
                cl = new GameObject("[Client]").AddComponent<GameClient>();
            }

            if(_selectedServer.Passworded)
            {
                var dialog = UGuiManager.Instance.Find<Modal_Dialog>();
                dialog.TakeInput("Enter Password", "This game requires a password: ", (s) =>
                {
                    cl.GameLoader.JoinGameAsync(_selectedServer.Address, _selectedServer.Port, s);
                });
                return;
            }

            cl.GameLoader.JoinGameAsync(_selectedServer.Address, _selectedServer.Port);
        }

        private void ApplyFilters()
        {
            _emptyButton.gameObject.SetActive(_serverTemplate.Children.Count == 0);
        }

        private string GetLobbyData(Lobby lobby, string key)
        {
            foreach(var kvp in lobby.Data)
            {
                if(kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
            return string.Empty;
        }

        private int GetLobbyDataInt(Lobby lobby, string key)
        {
            var result = 0;
            foreach (var kvp in lobby.Data)
            {
                if (kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(kvp.Value, out result);
                }
            }
            return result;
        }

    }
}

