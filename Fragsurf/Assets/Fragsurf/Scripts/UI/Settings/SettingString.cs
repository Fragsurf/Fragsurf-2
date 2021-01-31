using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class SettingString : SettingElement
    {

        [SerializeField]
        private TMP_InputField _input;

        protected override void _Initialize()
        {
            var val = DevConsole.GetVariable<string>(SettingName);
            _input.text = val.ToString();
            _input.onEndEdit.AddListener(OnEndEdit);
            _input.contentType = TMP_InputField.ContentType.Alphanumeric;
        }

        private void OnEndEdit(string value)
        {
            DevConsole.SetVariable(SettingName, value);
        }

    }
}

