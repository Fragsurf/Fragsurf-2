using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class SettingEnum : SettingElement
    {

        [SerializeField]
        private TMP_Dropdown _dropdown;
        private Type _enumType;
        private string[] _enumNames;

        protected override void _Initialize()
        {
            _dropdown.ClearOptions();
            _enumType = DevConsole.GetVariableType(SettingName);
            if (_enumType == null || !_enumType.IsEnum)
            {
                Debug.LogError("SettingEnum on wrong type: " + SettingName);
                return;
            }
            _dropdown.AddOptions(Enum.GetNames(_enumType).ToList());
            _dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void Update()
        {
            if(SettingName == "screen.mode"
                && !Setting.PendingChanges)
            {
                // anything to do with resolution is fucking jank so let's just make life easy.
                LoadValue();
            }
        }

        private void OnValueChanged(int index)
        {
            Queue($"{SettingName} {_dropdown.options[index].text}");
        }

        public override void LoadValue(string val)
        {
            if (_enumType == null)
            {
                _enumType = DevConsole.GetVariableType(SettingName);
                if (_enumType == null)
                {
                    return;
                }
            }

            if (_enumNames == null)
            {
                _enumNames = Enum.GetNames(_enumType);
            }

            for (int i = 0; i < _enumNames.Length; i++)
            {
                if (_enumNames[i].Equals(val, StringComparison.OrdinalIgnoreCase))
                {
                    _dropdown.SetValueWithoutNotify(i);
                    _dropdown.RefreshShownValue();
                    break;
                }
            }
        }

    }
}

