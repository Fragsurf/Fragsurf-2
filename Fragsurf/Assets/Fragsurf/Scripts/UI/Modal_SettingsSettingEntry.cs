using Fragsurf.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fragsurf.UI
{
    [Serializable]
    public class CustomSettingElement
    {
        public string SettingName;
        public SettingElement Element;
    }

    public class Modal_SettingsSettingEntry : EntryElement<string>, IPointerDownHandler
    {

        public string SettingName { get; private set; }
        public bool PendingChanges { get; private set; }

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
        private SettingBind _bindElement;
        [SerializeField]
        private SettingColor _colorElement;
        [SerializeField]
        private TMP_Text _label;
        [SerializeField]
        private TMP_Text _description;
        [SerializeField]
        private GameObject _pendingNotifier;

        private string _originalValue;
        private SettingElement _settingElement;

        protected override bool ContainsSearch(string input)
        {
            return (_label.text + _description.text).IndexOf(input, StringComparison.OrdinalIgnoreCase) != -1;
        }

        public override void LoadData(string settingName)
        {
            SettingName = settingName;
            _label.text = settingName;
            _settingElement = GetSettingElement(settingName);
            if(_settingElement == null)
            {
                Debug.LogError("Unsupported setting type: " + settingName);
                return;
            }
            SetDescription(string.Empty);
            _settingElement.gameObject.SetActive(true);
            _settingElement.Initialize(this, settingName);

            foreach(var se in GetComponentsInChildren<SettingElement>())
            {
                if(se == _settingElement)
                {
                    continue;
                }
                GameObject.Destroy(se.gameObject);
            }

            SetPendingChanges(false);

            _originalValue = DevConsole.GetVariableAsString(settingName);
        }

        public void SetLabel(string text)
        {
            _label.text = text;
        }

        public void SetDescription(string text)
        {
            _description.gameObject.SetActive(!string.IsNullOrEmpty(text));
            _description.text = text;
            transform.parent.gameObject.RebuildLayout();
        }

        public void SetPendingChanges(bool pending)
        {
            PendingChanges = pending;

            if (_pendingNotifier)
            {
                _pendingNotifier.SetActive(pending);
            }
        }

        private SettingElement GetSettingElement(string settingName)
        {
            var custom = _customElements.FirstOrDefault(x => x.SettingName.Equals(settingName, StringComparison.OrdinalIgnoreCase));
            if (custom != null)
            {
                return custom.Element;
            }
            if (settingName.StartsWith("bind/"))
            {
                return _bindElement;
            }

            var type = DevConsole.GetVariableType(settingName);

            if(type == null)
            {
                return null;
            }

            if(type.IsEnum)
            {
                return _enumElement;
            }

            if(type == typeof(Color))
            {
                return _colorElement;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
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

        public void OnPointerDown(PointerEventData eventData)
        {
            if(_settingElement == _bindElement
                || eventData.button != PointerEventData.InputButton.Right)
            {
                return;
            }

            var cmd = $"{SettingName} {_originalValue}";
            UGuiManager.Instance.Find<Modal_Settings>().QueueCommand(SettingName, cmd);
            _settingElement.LoadValue(_originalValue);
        }
    }
}

