using Fragsurf.Client.Interpolation;
using Fragsurf.Shared;

namespace Fragsurf.Client
{
    public class BareGameClient : FSGameLoop
    {
        public override bool IsHost => false;
        public ClientSocketManager Socket => GetFSComponent<ClientSocketManager>();
        public override INetworkInterface Network => Socket;

        protected override void RegisterComponents()
        { 
            AddFSComponent<ClientSocketManager>();
            AddFSComponent<EntityInterpolator>();
            AddFSComponent<ClientPlayerManager>();
            AddFSComponent<ClientInput>();
        }

        protected override void Initialize()
        {
            UserSettings.Instance.useGUILayout = UserSettings.Instance.useGUILayout;
        }

        protected override void OnDestroy()
        {
            if (UserSettings.Instance)
            {
                UserSettings.Instance.Save();
            }

            base.OnDestroy();
        }

    }
}

