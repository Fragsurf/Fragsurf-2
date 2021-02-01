using Fragsurf.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class SettingsPageData
    {
        public List<string> SettingNames;
    }

    public class Modal_SettingsPageEntry : EntryElement<SettingsPageData>
    {

        private Modal_SettingsSettingEntry _settingTemplate;

        private void OnEnable()
        {
            StartCoroutine(RebuildAfterFrame());
        }

        public override void LoadData(SettingsPageData data)
        {
            _settingTemplate = GetComponentInChildren<Modal_SettingsSettingEntry>(true);
            _settingTemplate.gameObject.SetActive(false);

            foreach (var settingName in data.SettingNames)
            {
                _settingTemplate.Append(settingName);
            }
        }

        private IEnumerator RebuildAfterFrame()
        {
            yield return new WaitForEndOfFrame();
            gameObject.RebuildLayout();
            var sr = GetComponentInChildren<ScrollRect>();
            if (sr)
            {
                sr.enabled = !sr.enabled;
                sr.enabled = !sr.enabled;
            }
        }

    }
}

