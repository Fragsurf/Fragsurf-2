using Fragsurf.Client;
using Fragsurf.Server;
using Fragsurf.Shared;
using Lidgren.Network;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fragsurf
{
    public class PlayTest : MonoBehaviour
    {

        public Vector3 SpawnPoint;

        private void Awake()
        {
            if (FSGameLoop.GetGameInstance(true)
                || FSGameLoop.GetGameInstance(false))
            {
                enabled = false;
                return;
            }
            SceneManager.LoadScene("Startup", LoadSceneMode.Additive);
        }

        private async void Start()
        {
            var client = gameObject.AddComponent<GameClient>();
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

