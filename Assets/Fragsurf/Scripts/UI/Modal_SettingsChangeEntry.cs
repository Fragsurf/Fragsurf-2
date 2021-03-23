using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class SettingsChangeEntryData
    {
        public string SettingName;
        public string Command;
    }

    public class Modal_SettingsChangeEntry : EntryElement<SettingsChangeEntryData>
    {

        [SerializeField]
        private TMP_Text _text;

        public SettingsChangeEntryData Data { get; private set; }

        public override void LoadData(SettingsChangeEntryData data)
        {
            Data = data;
            _text.text = data.Command;
        }

    }
}

