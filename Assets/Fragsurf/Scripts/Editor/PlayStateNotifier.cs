using UnityEditor;
using Fragsurf.Server;
using Fragsurf.Client;
using Steamworks;
using UnityEngine;
using Fragsurf;
using Fragsurf.Movement;
using DTCommandPalette;
using Fragsurf.Utility;

[InitializeOnLoad]
public static class PlayStateNotifier
{

    static PlayStateNotifier()
    {
        EditorApplication.playModeStateChanged += ModeChanged;
        SetPlaytestChecked();
    }

    [MenuItem("Fragsurf/Play/Toggle PlayTest")]
    public static void TogglePlayTest()
    {
        var pref = EditorPrefs.GetBool("Fragsurf.PlayTest", true);
        EditorPrefs.SetBool("Fragsurf.PlayTest", !pref);
        var str = !pref 
            ? "PlayTesting has been enabled, Fragsurf will load up when you enter play mode."
            : "PlayTesting has been disabled, Fragsurf will NOT load up when you enter play mode.";
        EditorUtility.DisplayDialog("PlayTest", str, "Ok");
        SetPlaytestChecked();
    }

    private static void SetPlaytestChecked()
    {
        var pref = EditorPrefs.GetBool("Fragsurf.PlayTest", true);
        Menu.SetChecked("Fragsurf/Play/Toggle PlayTest", pref);
    }

    [MethodCommand("Clear Play From")]
    [MenuItem("Fragsurf/Play/Clear Play From Position")]
    public static void ClearPlayFrom()
    {
        PlayFrom = Vector3.zero;
        EditorUtility.DisplayDialog("Play From Cleared", "Your Play From position has been cleared, you will now spawn at a spawn point when entering play mode.", "Ok");
    }

    public static Vector3 PlayFrom
    {
        get => UnityExtensions.StringToVector3(EditorPrefs.GetString("Fragsurf.PlayFrom", Vector3.zero.ToString()));
        set => EditorPrefs.SetString("Fragsurf.PlayFrom", value.ToString());
    }

    static async void ModeChanged(PlayModeStateChange stateChange)
    {
        if(stateChange == PlayModeStateChange.EnteredPlayMode
            && EditorPrefs.GetBool("Fragsurf.PlayTest", true))
        {
            var pt = GameObject.FindObjectOfType<PlayTest>(false);
            if (!pt)
            {
                new GameObject("[PlayTest]").AddComponent<PlayTest>().SpawnPoint = PlayFrom;
            }
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
                if (UserSettings.Instance)
                {
                    UserSettings.Instance.Save();
                }
                SteamClient.Shutdown();
                SteamClient.RunCallbacks();
                SteamClient.RunCallbacks();
            }
            System.Threading.Thread.Sleep(250);
        }
    }
}