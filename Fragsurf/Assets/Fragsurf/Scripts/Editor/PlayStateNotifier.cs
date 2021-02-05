using UnityEditor;
using Fragsurf.Server;
using Fragsurf.Client;
using Steamworks;
using UnityEngine;
using Fragsurf;
using Fragsurf.Movement;

[InitializeOnLoad]
public static class PlayStateNotifier
{

    [MenuItem("Fragsurf/Play/Toggle PlayTest")]
    public static void TogglePlayTest()
    {
        var pref = EditorPrefs.GetBool("Fragsurf.PlayTest", true);
        EditorPrefs.SetBool("Fragsurf.PlayTest", !pref);
        var str = !pref
            ? "PlayTesting has been enabled, Fragsurf will load up when you enter play mode."
            : "PlayTesting has been disabled, Fragsurf will NOT load up when you enter play mode.";
        EditorUtility.DisplayDialog("PlayTest", str, "Ok");
    }

    [MenuItem("Fragsurf/Play/Clear Play From Position")]
    public static void ClearPlayFrom()
    {
        PlayFrom = Vector3.zero;
        EditorUtility.DisplayDialog("Play From Cleared", "Your Play From position has been cleared, you will now spawn at a spawn point when entering play mode.", "Ok");
    }

    public static Vector3 PlayFrom
    {
        get => VectorExtensions.StringToVector3(EditorPrefs.GetString("Fragsurf.PlayFrom", Vector3.zero.ToString()));
        set => EditorPrefs.SetString("Fragsurf.PlayFrom", value.ToString());
    }

    static PlayStateNotifier()
    {
        EditorApplication.playModeStateChanged += ModeChanged;
    }

    static async void ModeChanged(PlayModeStateChange stateChange)
    {
        if(stateChange == PlayModeStateChange.EnteredPlayMode
            && EditorPrefs.GetBool("Fragsurf.PlayTest", true))
        {
            new GameObject("[PlayTest]").AddComponent<PlayTest>().SpawnPoint = PlayFrom;
        }

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