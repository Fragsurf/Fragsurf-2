using Fragsurf.Utility;
using Steamworks;
using UnityEngine;

namespace Fragsurf.Client
{
    public class SteamClientManager : SingletonComponent<SteamClientManager>
    {

        public static string ExceptionMessage;
        public static bool Initialized;

        ///// Methods /////

        //private string _test = string.Empty;

        private void Awake()
        {
            if (SteamClient.IsValid)
            {
                return;
            }

            Dispatch.OnException = (e) =>
            {
                UnityEngine.Debug.LogError(e.Message);
                UnityEngine.Debug.Log(e.StackTrace);
            };

            //Steamworks.Dispatch.OnDebugCallback = (type, str, server) =>
            //{
            //    UnityEngine.Debug.Log($"[Callback {type} {(server ? "server" : "client")}]");
            //    UnityEngine.Debug.Log($"{str}");
            //};

           //SteamNetworkingUtils.DebugLevel = NetDebugOutput.Everything;
            //SteamNetworkingUtils.OnDebugOutput += (e1, e2) =>
            //{
            //    UnityEngine.Debug.Log(e1 + ":" + e2);
            //};

            //Steamworks.Dispatch.OnDebugCallback = (type, str, server) =>
            //{
            //    UnityEngine.Debug.Log($"[Callback {type} {(server ? "server" : "client")}]");
            //    UnityEngine.Debug.Log(str);
            //    UnityEngine.Debug.Log($"");

            //    _test += $"[Callback {type} {(server ? "server" : "client")}]\n";
            //    _test += $"{str}\n";
            //    _test += $"\n\n";
            //};

            try
            {
                SteamClient.Init(Structure.AppId);
            }
            catch(System.Exception e)
            {
                ExceptionMessage = e.ToString();
                Debug.LogError(e.ToString());
                return;
            }

            if(!SteamApps.IsSubscribedToApp(Structure.AppId))
            {
                Application.Quit();
            }

            SteamNetworkingUtils.InitRelayNetworkAccess();

            Initialized = true;
        }

        private void Update()
        {
            if (SteamClient.IsValid)
            {
                SteamClient.RunCallbacks();
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                SteamClient.Shutdown();
            }
            catch { }

            //UnityEngine.Debug.Log(_test);
        }

    }
}
