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
            DevConsole.SetVariable(SettingName, newValue);
        }

        public override void LoadValue()
        {
            _toggle.isOn = DevConsole.GetVariable<bool>(SettingName);
        }

    }
}

