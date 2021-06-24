using Fragsurf.Shared;
using UnityEngine;

namespace Fragsurf.Client
{
    [Inject(InjectRealm.Client)]
    public class ScreenSettings : FSSharedScript
    {

        [ConVar("screen.resolution", "", ConVarFlags.UserSetting)]
        public string Resolution
        {
            get => ResolutionToString(CurrentResolution());
            set
            {
                var res = StringToResolution(value);
                Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRate);
            }
        }

        [ConVar("screen.mode", "", ConVarFlags.UserSetting)]
        public FullScreenMode ScreenMode
        {
            get => Screen.fullScreenMode;
            set => Screen.fullScreenMode = value;
        }

        [ConVar("screen.refreshrate", "", ConVarFlags.UserSetting)]
        public int RefreshRate
        {
            get => CurrentResolution().refreshRate;
            set
            {
                var r = CurrentResolution();
                Screen.SetResolution(r.width, r.height, Screen.fullScreenMode, value);
            }
        }

        [ConVar("screen.vsync", "", ConVarFlags.UserSetting)]
        public int VSyncCount
        {
            get => QualitySettings.vSyncCount;
            set => QualitySettings.vSyncCount = Mathf.Clamp(value, 0, 2);
        }

        public static Resolution CurrentResolution()
        {
            return new Resolution()
            {
                width = Screen.width,
                height = Screen.height,
                refreshRate = Screen.currentResolution.refreshRate
            };
        }

        public static string ResolutionToString(Resolution r)
        {
            return $"{r.width}x{r.height}";
        }

        public static Resolution StringToResolution(string s)
        {
            var split = s.Split('x');

            if(split == null 
                || split.Length != 2
                || !int.TryParse(split[0], out int width)
                || !int.TryParse(split[1], out int height))
            {
                Debug.LogError("Failed to parse resolution: " + s);
                return CurrentResolution();
            }

            return new Resolution()
            {
                width = width,
                height = height,
                refreshRate = CurrentResolution().refreshRate
            };
        }

    }
}

