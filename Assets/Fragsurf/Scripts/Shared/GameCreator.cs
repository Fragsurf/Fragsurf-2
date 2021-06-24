using Fragsurf.Client;
using Fragsurf.Server;
using Fragsurf.Shared.Packets;
using Fragsurf.Utility;
using System.Diagnostics;
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

        [ConCommand("map.change", "")]
        public void ChangeMapCmd(string map)
        {
            var sv = FSGameLoop.GetGameInstance(true) as GameServer;
            var gamemode = "Tricksurf";
            var name = DevConsole.GetVariable<string>("server.name");
            var pass = DevConsole.GetVariable<string>("server.password");

            if (sv)
            {
                gamemode = sv.GamemodeLoader.Gamemode.Data.Name;
                name = sv.Socket.ServerName;
                pass = sv.Socket.ServerPassword;
            }

            CreateGame(name, pass, gamemode, map);
        }

        private void Start()
        {
            DevConsole.RegisterObject(this);
        }

        public async Task<bool> CreateGame(string name, string password, string gamemode, string map)
        {
            var sv = FSGameLoop.GetGameInstance(true) as GameServer;
            if (sv)
            {
                sv.Socket.BroadcastPacket(PacketUtility.TakePacket<RetryOnDisconnect>());
                await Task.Delay(500);
                sv.Destroy();
            }

            var cl = FSGameLoop.GetGameInstance(false) as GameClient;
            if (cl)
            {
                cl.MenuOverride = GameData.Instance.EmptyScene;
                cl.Destroy();
            }

            sv = new GameObject("[Server]").AddComponent<GameServer>();
            sv.IsLocalServer = !Structure.DedicatedServer;

            if (!string.IsNullOrEmpty(name))
            {
                DevConsole.ExecuteLine("server.name \"" + name + "\"");
            }

            if (!string.IsNullOrEmpty(password))
            {
                DevConsole.ExecuteLine("server.password \"" + password + "\"");
            }

            if(await sv.GameLoader.CreateGameAsync(map, gamemode) != GameLoadResult.Success)
            {
                return false;
            }

            if (sv.IsLocalServer)
            {
                GameLoader.RetryRequested = false;
                cl = FSGameLoop.GetGameInstance(false) as GameClient;
                if(!cl || cl.GameLoader.State != GameLoaderState.New)
                {
                    if (cl) cl.Destroy();
                    cl = new GameObject("[Client]").AddComponent<GameClient>();
                }

                if (!string.IsNullOrEmpty(name))
                {
                    DevConsole.ExecuteLine("server.name \"" + name + "\"");
                }

                if (!string.IsNullOrEmpty(password))
                {
                    DevConsole.ExecuteLine("server.password \"" + password + "\"");
                }

                if (await cl.GameLoader.JoinGameAsync("localhost", sv.Socket.GameplayPort, sv.Socket.ServerPassword) != GameLoadResult.Success)
                {
                    cl.Destroy();
                    sv.Destroy();
                    return false;
                }
            }

            return true;
        }

    }
}

