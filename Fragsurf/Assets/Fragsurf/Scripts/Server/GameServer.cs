using Fragsurf.Shared;
using Fragsurf.Shared.LagComp;

namespace Fragsurf.Server
{
    public class GameServer : FSGameLoop
    {
        public static GameServer Instance { get; private set; }
        public override bool IsServer => true;
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

    }
}
