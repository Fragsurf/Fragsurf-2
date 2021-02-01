using System.IO;
using UnityEngine;
using SharpConfig;
using System;
using Fragsurf.Utility;

namespace Fragsurf.Client
{
    public class UserSettings : SingletonComponent<UserSettings>
    {

        private Configuration _config;
        public static ConsoleBinds Binds { get; } = new ConsoleBinds();

        private void Start()
        {
            Load();
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void Update()
        {
            Binds.Update();
        }

        public string GetRawValue(string key)
        {
            return _config["UserSettings"].Contains(key)
                ? _config["UserSettings"][key].RawValue
                : null;
        }

        public void UpdateUserSetting(string settingName, string value)
        {
            if (_config["UserSettings"][settingName] == null)
            {
                _config["UserSettings"].Add(settingName, value);
            }
            else
            {
                _config["UserSettings"][settingName].RawValue = value;
            }
        }

        public void Load()
        {
            var filePath = Path.Combine(Application.persistentDataPath, "UserSettings.cfg");

            try
            {
                if (!File.Exists(filePath))
                {
                    _config = new Configuration();
                    _config.Add("Binds");
                    _config.Add("UserSettings");
                    ExecuteDefaultSettings();
                    _config.SaveToFile(filePath);
                }
                else
                {
                    _config = Configuration.LoadFromFile(filePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                _config = new Configuration();
                _config.Add("Binds");
                _config.Add("UserSettings");
                ExecuteDefaultSettings();
                _config.SaveToFile(filePath);
            }

            try
            {
                ExecuteUserSettings();
            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }

            try
            {
                ExecuteConfigBinds();
            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public void Save(bool bindsOnly = false)
        {
            if(!bindsOnly)
            {
                var userSettings = DevConsole.GetVariablesWithFlags(ConVarFlags.UserSetting);
                foreach (string varName in userSettings)
                {
                    UpdateUserSetting(varName, DevConsole.GetVariableAsString(varName));
                }
            }
            SaveBinds();

            var filePath = Application.persistentDataPath + "/UserSettings.cfg";
            _config.SaveToFile(filePath);
        }

        private void ExecuteUserSettings()
        {
            foreach (var s in _config["UserSettings"])
            {
                var line = $"{s.Name} {s.StringValue}";
                DevConsole.ExecuteLine(line);
            }
        }

        private void ExecuteConfigBinds()
        {
            foreach (Setting setting in _config["Binds"])
            {
                Binds.Unbind(setting.Name);
                Binds.Bind(setting.Name, setting.StringValue);
            }
        }

        private void SaveBinds()
        {
            _config.RemoveAllNamed("Binds");
            var bindSection = _config.Add("Binds");
            foreach (var bind in Binds.Binds)
            {
                var settingName = bind.Key.ToString();
                var settingValue = bind.Command;
                bindSection.Add(settingName, settingValue);
            }
        }

        public void ExecuteDefaultBinds()
        {
            Binds.Clear();

            _config["Binds"].Clear();
            _config["Binds"].Add("mouse0", "+input handaction");
            _config["Binds"].Add("mouse1", "+input handaction2");
            _config["Binds"].Add("mouse3", "+input yaw 160");
            _config["Binds"].Add("mouse4", "+input yaw -160");
            _config["Binds"].Add("w", "+input moveforward");
            _config["Binds"].Add("a", "+input moveleft");
            _config["Binds"].Add("s", "+input moveback");
            _config["Binds"].Add("d", "+input moveright");
            _config["Binds"].Add("e", "+input interact");
            _config["Binds"].Add("alpha0", "+input slot0");
            _config["Binds"].Add("alpha1", "+input slot1");
            _config["Binds"].Add("alpha2", "+input slot2");
            _config["Binds"].Add("alpha3", "+input slot3");
            _config["Binds"].Add("alpha4", "+input slot4");
            _config["Binds"].Add("alpha5", "+input slot5");
            _config["Binds"].Add("r", "+input reload");
            _config["Binds"].Add("space", "+input jump");
            _config["Binds"].Add("leftshift", "+input speed");
            _config["Binds"].Add("leftcontrol", "+input duck");
            _config["Binds"].Add("g", "+input drop");
            _config["Binds"].Add("q", "+input previtem");
            _config["Binds"].Add("k", "+voicerecord");

            // Modal defaults
            _config["Binds"].Add("backquote", "modal.toggle console");
            _config["Binds"].Add("y", "modal.toggle chatbox");
            _config["Binds"].Add("f1", "modal.toggle perf");

            ExecuteConfigBinds();
        }

        public void ExecuteDefaultSettings()
        {
            ExecuteDefaultBinds();

            _config["UserSettings"].Clear();
            // ClientInput defaults
            _config["UserSettings"].Add("input.sensitivity", 1);
            _config["UserSettings"].Add("input.pitchmodifier", 1.0f);
            _config["UserSettings"].Add("input.adsmodifier", 1.0f);
            _config["UserSettings"].Add("input.toggleads", true);
            _config["UserSettings"].Add("input.confinecursor", false);

            // Graphics/video defaults
            _config["UserSettings"].Add("video.vsynccount", 0);
            _config["UserSettings"].Add("video.gpuframequeue", 2);
            _config["UserSettings"].Add("video.screenmode", "ExclusiveFullScreen");
            _config["UserSettings"].Add("game.targetfps", 300);
            _config["UserSettings"].Add("crosshair.hitmarker", true);
            _config["UserSettings"].Add("game.horizontalvelocity", true);
            _config["UserSettings"].Add("graphics.qualitylevel", 1);
            _config["UserSettings"].Add("graphics.postprocessing", true);
            _config["UserSettings"].Add("crosshair.color", "lime");
            _config["UserSettings"].Add("crosshair.outline", true);
            _config["UserSettings"].Add("crosshair.alpha", 1f);
            _config["UserSettings"].Add("cam.fov", 75);
            _config["UserSettings"].Add("cam.clipdistance", 500);

            ExecuteUserSettings();
        }

    }
}
