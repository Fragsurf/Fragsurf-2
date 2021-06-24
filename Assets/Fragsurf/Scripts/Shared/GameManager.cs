using Fragsurf.Client;
using Fragsurf.Server;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class GameManager : SingletonComponent<GameManager>
    {

        public bool StartwithClient;
        public bool StartwithServer;

        public GameServer Server { get; private set; }
        public GameClient Client { get; private set; }

        private void Start()
        {
            if (StartwithClient)
            {
                Client = new GameObject("[Client]").AddComponent<GameClient>();
            }
            if (StartwithServer)
            {
                Server = new GameObject("[Server]").AddComponent<GameServer>();
            }
        }

        public void CreateListenServer(string map, string gamemode, string name, string password = null)
        {
        }

        public void CreateServer(string map, string gamemode, string name, string password = null) 
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (Server)
            {
                Server.Destroy();
                Server = null;
            }

            if (Client)
            {
                Client.Destroy();
                Client = null;
            }
        }

    }
}

