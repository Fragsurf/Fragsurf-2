using Fragsurf.Client;
using Fragsurf.Server;
using Fragsurf.Shared;
using Lidgren.Network;
using System.Linq;
using UnityEngine;

namespace Fragsurf
{
    [RequireComponent(typeof(BareGameClient))]
    public class PlayTest : MonoBehaviour
    {

        public Vector3 SpawnPoint;

        private async void Start()
        {
            GameObject.DontDestroyOnLoad(gameObject);

            var client = GetComponent<BareGameClient>();
            var serverResult = await client.GameLoader.CreateServerAsync("LoadActiveScene", "Playtest", "Testing my map!", RandomString(8));
            if(serverResult == GameLoadResult.Success)
            {
                var server = FSGameLoop.GetGameInstance(true) as GameServer;
                var joinResult = await client.GameLoader.JoinGameAsync("localhost", server.Socket.GameplayPort, server.Socket.ServerPassword);
            }
        }

        private static System.Random _random = new System.Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

    }
}

