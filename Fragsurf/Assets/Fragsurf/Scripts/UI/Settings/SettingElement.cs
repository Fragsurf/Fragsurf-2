using Fragsurf.Utility;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public abstract class SettingElement : MonoBehaviour
    {

        public bool Initialized { get; private set; }
        public string SettingName { get; private set; }
        public Modal_SettingsSettingEntry Setting { get; private set; }

        public void Initialize(Modal_SettingsSettingEntry setting, string settingName)
        {
            Setting = setting;
            SettingName = settingName;
            _Initialize();
            Initialized = true;
            LoadValue();
        }

        protected abstract void _Initialize();

        public abstract void LoadValue();

        protected void Queue(string command)
        {
            var settings = UGuiManager.Instance.Find<Modal_Settings>();
            settings.QueueCommand(SettingName, command);
        }

    }
}

