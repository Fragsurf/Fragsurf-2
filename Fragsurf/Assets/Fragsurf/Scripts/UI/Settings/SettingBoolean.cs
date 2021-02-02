using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class SettingBoolean : SettingElement
    {

        [SerializeField]
        private Toggle _toggle;

        protected override void _Initialize()
        {
            _toggle.onValueChanged.AddListener(OnChanged);
        }

        private void OnChanged(bool newValue)
        {
            if(newValue == DevConsole.GetVariable<bool>(SettingName))
            {
                return;
            }
            Queue($"{SettingName} {newValue}");
        }

        public override void LoadValue(string val)
        {
            bool.TryParse(val, out bool bval);
            _toggle.isOn = bval;
        }

        public override void LoadValue()
        {
            LoadValue(DevConsole.GetVariable<bool>(SettingName).ToString());
        }

    }
}

