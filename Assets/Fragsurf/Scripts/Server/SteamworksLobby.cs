using Fragsurf.Maps;
using Fragsurf.Shared;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Fragsurf.Server
{
    public class SteamworksLobby : FSSharedScript
    {

        private Lobby? _lobby;

        protected override async void OnGameLoaded()
        {
            if (!SteamClient.IsValid || Application.isEditor)
            {
                return;
            }

            _lobby = await SteamMatchmaking.CreateLobbyAsync(100);

            if (!_lobby.HasValue)
            {
                DevConsole.WriteLine("Failed to create lobby");
                return;
            }

            var socketMan = Game.Network as SocketManager;
            var lobby = _lobby.Value;
            lobby.SetData("name", socketMan.ServerName);
            lobby.SetData("password", socketMan.ServerPassword ?? string.Empty);
            lobby.SetData("gamemode", Game.GamemodeLoader.Gamemode.Data.Name);
            lobby.SetData("map", Map.Current.Name);
            lobby.SetData("maxplayers", socketMan.MaxPlayers.ToString());
            lobby.SetGameServer(SteamClient.SteamId);
            lobby.SetPublic();
            lobby.SetJoinable(true);
        }

        protected override void _Destroy()
        {
            base._Destroy();

            if (_lobby.HasValue && SteamClient.IsValid)
            {
                _lobby.Value.Leave();
                _lobby = null;
            }
        }

    }
}

