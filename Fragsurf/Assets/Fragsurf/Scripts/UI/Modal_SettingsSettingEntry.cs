using Fragsurf.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    [Serializable]
    public class CustomSettingElement
    {
        public string SettingName;
        public SettingElement Element;
    }

    public class Modal_SettingsSettingEntry : EntryElement<string>
    {

        [SerializeField]
        private List<CustomSettingElement> _customElements;
        [SerializeField]
        private SettingNumber _integerElement;
        [SerializeField]
        private SettingNumber _floatElement;
        [SerializeField]
        private SettingBoolean _boolElement;
        [SerializeField]
        private SettingString _stringElement;
        [SerializeField]
        private SettingEnum _enumElement;
        [SerializeField]
        private TMP_Text _label;
        [SerializeField]
        private TMP_Text _description;

        public override void LoadData(string settingName)
        {
            _label.text = settingName;
            var settingElement = GetSettingElement(settingName);
            if(settingElement == null)
            {
                Debug.LogError("Unsupported setting type: " + settingName);
                return;
            }
            SetDescription(string.Empty);
            settingElement.gameObject.SetActive(true);
            settingElement.Initialize(this, settingName);
        }

        public void SetDescription(string text)
        {
            _description.gameObject.SetActive(!string.IsNullOrEmpty(text));
            _description.text = text;
            transform.parent.gameObject.RebuildLayout();
        }

        private SettingElement GetSettingElement(string settingName)
        {
            var custom = _customElements.FirstOrDefault(x => x.SettingName.Equals(settingName, StringComparison.OrdinalIgnoreCase));
            if (custom != null)
            {
                return custom.Element;
            }
            var type = DevConsole.GetVariableType(settingName);
            if (type.IsEnum)
            {
                return _enumElement;
            }
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

