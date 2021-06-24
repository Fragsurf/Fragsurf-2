using Fragsurf.Client;
using Fragsurf.Server;
using Fragsurf.Shared.Packets;
using Fragsurf.Utility;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Shared
{
    // Servers & Clients are instanced, one per game.
    // So to make a new game, the server & client need to be totally destroyed
    // For map changes we need an external script to destroy then recreate, hence this script
    public class GameCreator : SingletonComponent<GameCreator>
    {

        public bool RetryRequested;

        [ConCommand("map.change", "")]
        public void ChangeMapCmd(string map)
        {
            var sv = FSGameLoop.GetGameInstance(true);
            var gamemode = "Tricksurf";
            if(sv)
            {
                gamemode = sv.GamemodeLoader.Gamemode.Data.Name;
            }
            var name = DevConsole.GetVariable<string>("server.name");
            var pass = DevConsole.GetVariable<string>("server.password");

            CreateGame(name, pass, gamemode, map);
        }

        private void Start()
        {
            DevConsole.RegisterObject(this);
        }

        public async void CreateGame(string name, string password, string gamemode, string map)
        {
            var server = FSGameLoop.GetGameInstance(true);
            if (server)
            {
                server.Network.BroadcastPacket(PacketUtility.TakePacket<RetryOnDisconnect>());
                await Task.Delay(250);
                server.Destroy();
                await Task.Delay(100);
            }

            var client = FSGameLoop.GetGameInstance(false);
            if (client)
            {
                client.Destroy();
                await Task.Delay(100);
            }

            if (!string.IsNullOrEmpty(name))
            {
                DevConsole.ExecuteLine("server.name \"" + name + "\"");
            }
            if (!string.IsNullOrEmpty(password))
            {
                DevConsole.ExecuteLine("server.password \"" + password + "\"");
            }

            var sv = new GameObject("[Server]").AddComponent<GameServer>();
            var serverResult = await sv.GameLoader.CreateGameAsync(map, gamemode);
            if (!Structure.DedicatedServer && serverResult == GameLoadResult.Success)
            {
                sv.IsLocalServer = true;
                var cl = new GameObject("[Client]").AddComponent<GameClient>();
                var joinResult = await cl.GameLoader.JoinGameAsync("localhost", sv.Socket.GameplayPort, sv.Socket.ServerPassword);
            }
        }

    }
}

