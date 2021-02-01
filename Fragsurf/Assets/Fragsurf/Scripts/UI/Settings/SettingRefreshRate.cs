using Fragsurf.Client;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class SettingRefreshRate : SettingElement
    {

        [SerializeField]
        private TMP_Dropdown _dropdown;

        //private int[] _stupidRefreshRates = { 23, 59, 84, 99, 119, 143 };

        protected override void _Initialize()
        {
            var options = new List<string>();
            foreach(var res in Screen.resolutions)
            {
                var rr = res.refreshRate;
                var rrs = rr.ToString();
                if (!options.Contains(rrs))
                {
                    options.Add(rrs);
                }
            }

            _dropdown.ClearOptions();
            _dropdown.AddOptions(options);
            _dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void Update()
        {
            LoadValue();
            var label = Screen.fullScreenMode != FullScreenMode.ExclusiveFullScreen
                ? "Refresh rate only applies in ExclusiveFullScreen mode!"
                : string.Empty;
            Setting.SetDescription(label);
        }

        private void OnValueChanged(int idx)
        {
            Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreenMode, int.Parse(_dropdown.options[idx].text));
        }

        public override void LoadValue()
        {
            var curRR = Screen.currentResolution.refreshRate.ToString();
            var curRRp1 = (Screen.currentResolution.refreshRate + 1).ToString();
            var curRRm1 = (Screen.currentResolution.refreshRate - 1).ToString();
            for(int i = 0; i < _dropdown.options.Count; i++)
            {
                if (_dropdown.options[i].text.Equals(curRR)
                    || _dropdown.options[i].text.Equals(curRRp1)
                    || _dropdown.options[i].text.Equals(curRRm1))
                {
                    _dropdown.SetValueWithoutNotify(i);
                    return;
                }
            }
        }

    }
}

