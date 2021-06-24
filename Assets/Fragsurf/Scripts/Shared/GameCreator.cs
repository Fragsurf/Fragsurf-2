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

    // Side note: Main Menu has a GameClient component by default.  It's kind of a headache always maintaining 
    // 1 valid GameClient at all times, could do for some refactor or some shit, maybe GameClient persists but 
    // can just start fresh when it needs to
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

        public async Task<bool> CreateGame(string name, string password, string gamemode, string map)
        {
            var server = FSGameLoop.GetGameInstance(true);
            if (server)
            {
                server.Network.BroadcastPacket(PacketUtility.TakePacket<RetryOnDisconnect>());
                await Task.Delay(250);
                server.Destroy();
                await Task.Delay(100);
            }

            var sv = new GameObject("[Server]").AddComponent<GameServer>();

            if (!string.IsNullOrEmpty(name))
            {
                DevConsole.ExecuteLine("server.name \"" + name + "\"");
            }

            if (!string.IsNullOrEmpty(password))
            {
                DevConsole.ExecuteLine("server.password \"" + password + "\"");
            }

            var serverResult = await sv.GameLoader.CreateGameAsync(map, gamemode);

            if(serverResult == GameLoadResult.Success && !Structure.DedicatedServer)
            {
                sv.IsLocalServer = true;
                JoinGame("localhost", sv.Socket.GameplayPort, sv.Socket.ServerPassword);
                return true;
            }

            return false;
        }

        private void JoinGame(string address, int port, string password = null)
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if(!cl || cl.GameLoader.State != GameLoaderState.New)
            {
                if (cl)
                {
                    cl.Destroy();
                }
                cl = new GameObject("[Client]").AddComponent<GameClient>();
            }
            cl.GameLoader.JoinGameAsync(address, port, password);
        }

    }
}

