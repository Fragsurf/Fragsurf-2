using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_SettingsSettingEntry : EntryElement<string>
    {

        [SerializeField]
        private SettingNumber _integerElement;
        [SerializeField]
        private SettingNumber _floatElement;
        [SerializeField]
        private SettingBoolean _boolElement;
        [SerializeField]
        private SettingString _stringElement;
        [SerializeField]
        private TMP_Text _label;

        public override void LoadData(string settingName)
        {
            _label.text = settingName;
            var settingType = DevConsole.GetVariableType(settingName);
            var settingElement = GetSettingElement(settingType);
            if(settingElement == null)
            {
                Debug.LogError("Unsupported setting type: " + settingType.Name);
                return;
            }
            settingElement.gameObject.SetActive(true);
            settingElement.Initialize(settingName);
        }

        private SettingElement GetSettingElement(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Int16:
                    return _integerElement;
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return _floatElement;
                case TypeCode.String:
                    return _stringElement;
                case TypeCode.Boolean:
                    return _boolElement;
                default:
                    return null;
            }
        }

    }
}

