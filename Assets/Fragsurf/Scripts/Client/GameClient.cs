using Fragsurf.Maps;
using Fragsurf.Server;
using Fragsurf.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fragsurf.Client
{
    public class GameClient : FSGameLoop
    {
        public override bool IsHost => false;
        public ClientSocketManager Socket => GetFSComponent<ClientSocketManager>();
        public GameServer LocalServer { get; set; }

        protected override void RegisterComponents()
        { 
            AddFSComponent<ClientSocketManager>(); // INetworkInterface
            AddFSComponent<EntityInterpolator>();
            AddFSComponent<ClientPlayerManager>();
            AddFSComponent<ClientInput>();
        }

        protected override void Initialize()
        {
            // I feel like I put this here on purpose for absolutely no reason other than I could
            // but idk.  I like it so it stays
            UserSettings.Instance.useGUILayout = UserSettings.Instance.useGUILayout;
            UserSettings.Instance.Load();

            TryRetry();
        }

        private async void TryRetry()
        {
            if (GameLoader.RetryRequested)
            {
                GameLoader.RetryRequested = false;

                var result = await GameLoader.JoinGameAsync(GameLoader.LastJoinedAddress, GameLoader.LastJoinedPort, GameLoader.LastJoinedPassword);

                if(result != GameLoadResult.Success)
                {
                    Destroy();
                }
            }
        }

        private bool _quitting;
        private void OnApplicationQuit()
        {
            _quitting = true;
        }

        protected override void OnDestroy()
        {
            if (UserSettings.Instance)
            {
                UserSettings.Instance.Save();
            }

            if (!_quitting)
            {
                if (Map.Current != null)
                {
                    Map.UnloadAsync();
                }

                SceneManager.LoadScene(GameData.Instance.MainMenu);
            }

            base.OnDestroy();
        }

    }
}

