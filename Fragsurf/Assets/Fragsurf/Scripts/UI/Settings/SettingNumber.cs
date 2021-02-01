using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class SettingNumber : SettingElement
    {

        [SerializeField]
        private TMP_InputField _input;
        [SerializeField]
        private bool _allowDecimal;


        protected override void _Initialize()
        {
            _input.onEndEdit.AddListener(OnEndEdit);
            _input.contentType = _allowDecimal 
                ? TMP_InputField.ContentType.DecimalNumber 
                : TMP_InputField.ContentType.IntegerNumber;
        }

        private void OnEndEdit(string value)
        {
            if (value.Equals(DevConsole.GetVariableAsString(SettingName)))
            {
                return;
            }
            Queue($"{SettingName} {value}");
        }

        public override void LoadValue()
        {
            _input.text = DevConsole.GetVariableAsString(SettingName);
        }

    }
}

