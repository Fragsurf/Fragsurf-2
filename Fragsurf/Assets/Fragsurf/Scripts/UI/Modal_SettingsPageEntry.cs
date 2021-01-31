using System.Collections.Generic;

namespace Fragsurf.UI
{
    public class SettingsPageData
    {
        public List<string> SettingNames;
    }

    public class Modal_SettingsPageEntry : EntryElement<SettingsPageData>
    {

        private Modal_SettingsSettingEntry _settingTemplate;

        public override void LoadData(SettingsPageData data)
        {
            _settingTemplate = GetComponentInChildren<Modal_SettingsSettingEntry>(true);
            _settingTemplate.gameObject.SetActive(false);

            foreach (var settingName in data.SettingNames)
            {
                _settingTemplate.Append(settingName);
            }
        }
    }
}

