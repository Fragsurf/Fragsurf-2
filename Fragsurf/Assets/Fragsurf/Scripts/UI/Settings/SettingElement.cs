using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public abstract class SettingElement : MonoBehaviour
    {

        public string SettingName { get; private set; }
        public Modal_SettingsSettingEntry Setting { get; private set; }

        public void Initialize(Modal_SettingsSettingEntry setting, string settingName)
        {
            Setting = setting;
            SettingName = settingName;
            _Initialize();
        }

        protected abstract void _Initialize();

    }
}

