using System;
using System.Collections.Generic;
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
            SetDropdownValue();
        }

        private void OnValueChanged(int index)
        {
            DevConsole.ExecuteLine(SettingName + " " + _dropdown.options[index].text);
            //SetDropdownValue();
        }

        private void SetDropdownValue()
        {
            var val = DevConsole.GetVariableAsString(SettingName);
            var names = Enum.GetNames(_enumType);
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(val, StringComparison.OrdinalIgnoreCase))
                {
                    _dropdown.SetValueWithoutNotify(i);
                    _dropdown.RefreshShownValue();
                    break;
                }
            }
        }

    }
}

