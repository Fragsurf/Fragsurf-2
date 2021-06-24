using Fragsurf.Shared;
using Fragsurf.Shared.LagComp;
using UnityEngine;

namespace Fragsurf.Server
{
    public class GameServer : FSGameLoop
    {

        public static GameServer Instance => GetGameInstance(true) as GameServer;
        public override bool IsHost => true;
        public SocketManager Socket => GetFSComponent<SocketManager>();

        [ConVar("server.map", "Default map the server will start on")]
        public string DefaultMap { get; set; } = "surf_ny_bigloop_nf";
        [ConVar("server.gamemode", "Default map the server will start on")]
        public string DefaultGamemode { get; set; } = "Combat Surf";

        protected override void RegisterComponents()
        {
            AddFSComponent<SocketManager>();
            AddFSComponent<LagCompensator>();
            AddFSComponent<ServerPlayerManager>();
            AddFSComponent<SteamworksServer>();
            AddFSComponent<SteamworksLobby>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Debug.Log("Game server destroyed");
        }

        protected override void Initialize()
        {
            Socket.StartServer();
        }

        protected override void OnTick()
        {
            base.OnTick();

            var shouldIdle = PlayerManager.PlayerCount == 0;
            if(Idling != shouldIdle)
            {
                //TimeStep.Instance.SetIdleMode(shouldIdle);
                //Idling = shouldIdle;
            }
        }

    }
}
