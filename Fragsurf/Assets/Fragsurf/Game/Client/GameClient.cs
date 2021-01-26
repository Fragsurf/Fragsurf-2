using Fragsurf.Shared;
using Fragsurf.Client.Interpolation;
using System;
using UnityEngine;

namespace Fragsurf.Client
{
    public class GameClient : FSGameLoop
    {
        public static GameClient Instance { get; private set; }
        public override bool IsHost => false;
        public ClientSocketManager Socket => GetFSComponent<ClientSocketManager>();
        public override INetworkInterface Network => Socket;

        protected override void RegisterComponents()
        {
            // order may be important
            // some components require references to exist.  should maybe take a look at this later
            AddFSComponent<ClientSocketManager>();
            AddFSComponent<ClientInput>();
            AddFSComponent<FileDownloader>();
            AddFSComponent<SettingFactory>();
            AddFSComponent<ClientPlayerManager>();
            AddFSComponent<EntityInterpolator>();
            AddFSComponent<ConnectLaunchParam>();

            foreach (var clientBehaviour in FindObjectsOfType<FSClientBehaviour>())
            {
                AddFSComponent(clientBehaviour);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
            UserSettings.Instance.Save();
        }

        protected override void Initialize()
        {
            Instance = this;
        }

        private void Start()
        {
            try
            {
                var resString = DevConsole.GetVariable<string>("video.resolution");
                var refreshRate = DevConsole.GetVariable<int>("video.refreshrate");
                var fsmString = DevConsole.GetVariable<string>("video.screenmode");
                var res = SettingFactory.StringToResolution(resString);
                Enum.TryParse(fsmString, out FullScreenMode fsm);
                Screen.SetResolution(res.width, res.height, fsm, refreshRate);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
        }

    }
}
