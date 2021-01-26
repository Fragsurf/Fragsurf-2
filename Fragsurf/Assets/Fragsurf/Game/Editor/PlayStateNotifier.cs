using UnityEditor;
using Fragsurf.Server;
using Fragsurf.Client;
using Steamworks;

[InitializeOnLoad]
public static class PlayStateNotifier
{

    static PlayStateNotifier()
    {
        EditorApplication.playModeStateChanged += ModeChanged;
    }

    static async void ModeChanged(PlayModeStateChange stateChange)
    {
        if(stateChange == PlayModeStateChange.ExitingPlayMode)
        {
            if (SteamServer.IsValid)
            {
                SteamServer.Shutdown();
                SteamServer.RunCallbacks();
                SteamServer.RunCallbacks();
            }
            if (SteamClient.IsValid)
            {
                SteamClient.Shutdown();
                SteamClient.RunCallbacks();
                SteamClient.RunCallbacks();
            }
            System.Threading.Thread.Sleep(250);
        }
    }
}