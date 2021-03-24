using Fragsurf.Shared;
using Fragsurf.Shared.LagComp;
using Fragsurf.Utility;

namespace Fragsurf.Server
{
    public class GameServer : FSGameLoop
    {
        public static GameServer Instance { get; private set; }
        public override bool IsHost => true;
        public SocketManager Socket => GetFSComponent<SocketManager>();

        protected override void RegisterComponents()
        {
            AddFSComponent<SocketManager>();
            AddFSComponent<LagCompensator>();
            AddFSComponent<ServerPlayerManager>();
        }

        protected override void Initialize()
        {
            Instance = this;
            Socket.StartServer();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
        }

        protected override void OnTick()
        {
            base.OnTick();

            var shouldIdle = PlayerManager.PlayerCount == 0;
            if(Idling != shouldIdle)
            {
                TimeStep.Instance.SetIdleMode(shouldIdle);
                Idling = shouldIdle;
            }
        }

    }
}
