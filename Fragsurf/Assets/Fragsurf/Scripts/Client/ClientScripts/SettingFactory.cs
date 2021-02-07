using System;
using UnityEngine;
using Fragsurf.Shared;

namespace Fragsurf.Client
{
    public class SettingFactory : FSComponent
    {
        public static event Action MonitorNumberChanged; 

        private string _resolution = string.Empty;
        private int _refreshRate = 144;
        private string _screenMode = "ExclusiveFullScreen";
        private float _fov = 75;
        private bool _mount = true;
        private bool _toggleAds = true;
        private float _adsSensModifier = 1.0f;
        private float _sensitivity = 1.0f;
        private float _pitchModifier = 1.0f;
        private bool _fancy = true;
        private bool _verticalVelocity;
        private int _clipDistance = 500;
        private float _voiceOutputVolume = 1f;
        private bool _thirdPerson;
        private bool _thirdPersonShoulder;
        private bool _canSetResolution = false;

        protected override void _Initialize()
        {
            _refreshRate = FixedRefreshRate(Screen.currentResolution.refreshRate);
            _resolution = ResolutionToString(Screen.currentResolution);

            MapLoader.Instance.OnMapEvent += (a, b, c) =>
            {
                ApplyFancy();
            };

            DevConsole.RegisterVariable("game.verticalvelocity", "Enable this to show vertical velocity in the speedometer", () => _verticalVelocity, v => _verticalVelocity = v, this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("input.toggleads", "Toggle ADS as opposed to hold", () => _toggleAds, v => _toggleAds = v, this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("input.adsmodifier", "Sensitivity modifier while in ADS mode", () => _adsSensModifier, v => _adsSensModifier = v, this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("sensitivity", "Mouse sensitivity", () => _sensitivity, v => { _sensitivity = v; }, this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("input.sensitivity", "Mouse sensitivity", () => _sensitivity, v => { _sensitivity = v; }, this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("input.pitchmodifier", "Y axis sensitivity modifier", () => _pitchModifier, v => { _pitchModifier = v; }, this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("audio.voiceoutput", "Volume for voice chat output", () => _voiceOutputVolume, v => _voiceOutputVolume = v, this, ConVarFlags.UserSetting);
            //FSLog.RegisterVariable("graphics.gamma", "", v => SetGamma(v), () => _gamma, this, FSLog.VarFlags.UserSetting);
            //FSLog.RegisterVariable("graphics.post.motionblur", "Blurs frames in motion", v => SetMotionBlur(v), () => _motionBlur, this, FSLog.VarFlags.UserSetting);
            DevConsole.RegisterVariable("graphics.fancy", "Turn this off to disable performance heavy objects in the map, such as particles and grass.", () => _fancy, v => SetFancy(v), this, ConVarFlags.UserSetting);
            DevConsole.RegisterCommand("graphics.potato", "Turn on potato mode", this, SetPotatoMode, false);
            DevConsole.RegisterVariable("video.resolution", "Your screen resolution (i.e. 1280x720)", () => _resolution, v => SetResolution(v), this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("video.refreshrate", "Monitor refresh rate.", () => _refreshRate, v => SetRefreshRate(v), this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("video.screenmode", "Your screen mode", () => _screenMode, v => SetScreenMode(v), this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("video.vsynccount", "Vertical sync count (set to 0 to disable for highest framerate)", () => QualitySettings.vSyncCount, v => SetVsyncCount(v), this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("video.gpuframequeue", "GPU frame queue", () => QualitySettings.maxQueuedFrames, v => QualitySettings.maxQueuedFrames = v, this, ConVarFlags.UserSetting);

            DevConsole.RegisterVariable("video.monitornumber", "Display # for multiple monitor setups",
            () =>
            {
                for (int i = 0; i < Display.displays.Length; i++)
                {
                    if (Display.displays[i].active)
                    {
                        return i;
                    }
                }
                return 0;
            },
            v =>
            {
                v = Mathf.Clamp(v, 0, Display.displays.Length - 1);
                if (!Display.displays[v].active)
                {
                    var d = Display.displays[v];
                    d.Activate();
                    Screen.SetResolution(d.systemWidth, d.systemHeight, Screen.fullScreenMode);
                }
                MonitorNumberChanged?.Invoke();
            }, 
            this, ConVarFlags.UserSetting);

            DevConsole.RegisterVariable("cam.fov", "Your camera's field of view", () => _fov, v => SetFov(v), this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("cam.thirdperson", "Enables third person camera", () => _thirdPerson, v => _thirdPerson = v, this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("cam.thirdpersonshoulder", "Enables third person over the shoulder camera", () => _thirdPersonShoulder, v => _thirdPersonShoulder = v, this, ConVarFlags.UserSetting);
            DevConsole.RegisterVariable("map.mount", "Attempt to mount CS:S and CS:GO content", () => _mount, v => _mount = v, this, ConVarFlags.UserSetting);

            DevConsole.RegisterVariable("graphics.antialiasing",
                "This softens edges of your geometry, so they’re not jagged or flickering.  Can affect performance.",
                () => QualitySettings.antiAliasing,
                v =>
                {
                    QualitySettings.antiAliasing = Mathf.Clamp(v, 0, 8);
                },
                this,
                ConVarFlags.UserSetting);

            DevConsole.RegisterVariable("graphics.shadows",
                "Uncheck this box to disable all shadows.  Disable for performance.",
                () => QualitySettings.shadows != ShadowQuality.Disable,
                v =>
                {
                    QualitySettings.shadows = v ? ShadowQuality.All : ShadowQuality.Disable;
                },
                this,
                ConVarFlags.UserSetting);

            DevConsole.RegisterVariable("graphics.shadowdistance",
                "This controls how far ahead of the camera objects cast shadows, in Unity units.  Reduce for performance.",
                () => QualitySettings.shadowDistance,
                v =>
                {
                    QualitySettings.shadowDistance = v;
                },
                this,
                ConVarFlags.UserSetting);

            DevConsole.RegisterVariable("graphics.shadowcascades",
                "A high number of cascades gives you more detailed shadows nearer the camera.  Disable for performance.",
                () => QualitySettings.shadowCascades,
                v =>
                {
                    QualitySettings.shadowCascades = v;
                },
                this,
                ConVarFlags.UserSetting);

            DevConsole.RegisterVariable("graphics.qualitylevel",
                "General quality level.  Lower this for better performance",
                () => QualitySettings.GetQualityLevel(),
                v =>
                {
                    v = Mathf.Clamp(v, 0, 5);
                    QualitySettings.SetQualityLevel(v);
                },
                this,
                ConVarFlags.UserSetting);
        }

        protected override void _Start()
        {
            _canSetResolution = true;

            foreach (var userSetting in DevConsole.GetVariablesWithFlags(ConVarFlags.UserSetting))
            {
                DevConsole.ExecuteLine(userSetting + " \"" + DevConsole.GetVariableAsString(userSetting) + "\"");
            }
        }

        public void ApplyFancy()
        {
            SetFancy(_fancy);
        }

        private void SetPotatoMode(string[] args)
        {
            QualitySettings.vSyncCount = 0;
            QualitySettings.maxQueuedFrames = 2;
            QualitySettings.maximumLODLevel = 0;
            QualitySettings.skinWeights = SkinWeights.OneBone;
            SetFancy(false);
            DevConsole.ExecuteLine("graphics.postprocessing false");
            DevConsole.ExecuteLine("graphics.antialiasing 1");
            DevConsole.ExecuteLine("graphics.shadows false");
        }

        private void SetFancy(bool value)
        {
            _fancy = value;

            var fancy = GameObject.Find("_Fancy");
            if(fancy != null)
            {
                foreach(Transform tr in fancy.transform)
                {
                    tr.gameObject.SetActive(value);
                }
            }
        }

        private void SetFov(float value)
        {
            _fov = Mathf.Clamp(value, 60f, 90f);
            if(GameCamera.Camera != null)
            {
                GameCamera.Camera.fieldOfView = _fov;
            }
        }

        private void SetVsyncCount(int value)
        {
            int amt = Mathf.Clamp(value, 0, 2);
            QualitySettings.vSyncCount = amt;
        }

        private void SetMotionBlur(bool value)
        {
            //_motionBlur = value;
            //GameClient.Instance.CamController.Cam.GetComponent<Volume>().profile.TryGet(out MotionBlur setting);
            //setting.active = value;
        }

        public static int FixedRefreshRate(int value)
        {
            if (value == 23 
                || value == 59 
                || value == 84 
                || value == 99 
                || value == 119 
                || value == 143 
                || value == 164 
                || value == 239)
            {
                value++;
            }
            return value;
        }

        private void SetRefreshRate(int value)
        {
            _refreshRate = FixedRefreshRate(value);

            if (!_canSetResolution)
            {
                return;
            }

            var curRes = Screen.currentResolution;
            Screen.SetResolution(curRes.width, curRes.height, Screen.fullScreenMode, _refreshRate);
        }

        private void SetResolution(string value)
        {
            _resolution = value;

            if (!_canSetResolution)
            {
                return;
            }

            var res = StringToResolution(value);
            var screenMode = Screen.fullScreenMode;
            if(Enum.TryParse(_screenMode, out FullScreenMode definedScreenMode))
            {
                screenMode = definedScreenMode;
            }
            Screen.SetResolution(res.width, res.height, screenMode, Screen.currentResolution.refreshRate);
        }

        public static string ResolutionToString(Resolution res)
        {
            return $"{res.width}x{res.height}";
        }

        public static Resolution StringToResolution(string str)
        {
            try
            {
                var nums = str.Split('x');
                var width = int.Parse(nums[0].Trim());
                var height = int.Parse(nums[1].Trim());
                //var refreshRate = int.Parse(nums[2].Trim());
                return new Resolution()
                {
                    width = width,
                    height = height,
                    refreshRate = Screen.currentResolution.refreshRate
                };
            }
            catch(Exception e) { Debug.LogError(e.Message); }
            return Screen.currentResolution;
        }

        private void SetScreenMode(string value)
        {
            if(Enum.TryParse(value, true, out FullScreenMode mode))
            {
                _screenMode = value;
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, mode, Screen.currentResolution.refreshRate);
                PlayerPrefs.SetString("ScreenMode", value);
            }
        }

    }
}

