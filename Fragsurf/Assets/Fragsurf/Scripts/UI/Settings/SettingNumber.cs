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
            var val = DevConsole.GetVariableAsString(SettingName);
            _input.text = val.ToString();
            _input.onEndEdit.AddListener(OnEndEdit);
            _input.contentType = _allowDecimal 
                ? TMP_InputField.ContentType.DecimalNumber 
                : TMP_InputField.ContentType.IntegerNumber;
        }

        private void Update()
        {
            if(SettingName == "screen.refreshrate")
            {
                // anything to do with resolution is fucking jank so let's just make life easy.
                _input.text = DevConsole.GetVariableAsString(SettingName);
            }
        }

        private void OnEndEdit(string value)
        {
            if (!_allowDecimal
                && int.TryParse(value, out int val))
            {
                DevConsole.SetVariable(SettingName, val);
            }
            else if(float.TryParse(value, out float fval))
            {
                DevConsole.SetVariable(SettingName, fval);
            }
            _input.text = DevConsole.GetVariableAsString(SettingName);
        }

    }
}

