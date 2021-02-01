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

            SetDropdownValue();
        }

        private void OnValueChanged(int index)
        {
            var newRes = _dropdown.options[index].text;
            var res = ScreenSettings.StringToResolution(newRes);
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRate);
            StartCoroutine(SetValueAfterFrame());
        }

        private void SetDropdownValue()
        {
            var curResString = ScreenSettings.ResolutionToString(ScreenSettings.CurrentResolution());
            for(int i = 0; i < _dropdown.options.Count; i++)
            {
                if (_dropdown.options[i].text.Equals(curResString))
                {
                    _dropdown.SetValueWithoutNotify(i);
                    _dropdown.RefreshShownValue();
                    break;
                }
            }
        }

        private IEnumerator SetValueAfterFrame()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            SetDropdownValue();
        }

    }
}

