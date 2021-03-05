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
            _input.onEndEdit.AddListener(OnEndEdit);
            _input.contentType = TMP_InputField.ContentType.Standard;
        }

        private void OnEndEdit(string value)
        {
            if (value.Equals(DevConsole.GetVariableAsString(SettingName)))
            {
                return;
            }
            Queue($"{SettingName} {value}");
        }

        public override void LoadValue(string val)
        {
            _input.text = val;
        }

    }
}

