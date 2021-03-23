using Fragsurf.Client;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class SettingResolution : SettingElement
    {

        [SerializeField]
        private TMP_Dropdown _dropdown;

        protected override void _Initialize()
        {
            var resolutions = new List<string>();

            for(int i = 0; i < Screen.resolutions.Length; i++)
            {
                var resString = ScreenSettings.ResolutionToString(Screen.resolutions[i]);
                if (!resolutions.Contains(resString))
                {
                    resolutions.Add(resString);
                }
            }

            _dropdown.ClearOptions();
            _dropdown.AddOptions(resolutions);
            _dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void Update()
        {
            if (!Setting.PendingChanges)
            {
                LoadValue();
            }
        }

        private void OnValueChanged(int index)
        {
            Queue($"{SettingName} {_dropdown.options[index].text}");
        }

        public override void LoadValue(string val)
        {
            for (int i = 0; i < _dropdown.options.Count; i++)
            {
                if (_dropdown.options[i].text.Equals(val))
                {
                    _dropdown.SetValueWithoutNotify(i);
                    _dropdown.RefreshShownValue();
                    break;
                }
            }
        }

    }
}

