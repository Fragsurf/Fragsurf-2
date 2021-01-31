using TMPro;
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
            _toggle.isOn = DevConsole.GetVariable<bool>(SettingName);
            _toggle.onValueChanged.AddListener(OnChanged);
        }

        private void OnChanged(bool newValue)
        {
            DevConsole.SetVariable(SettingName, newValue);
        }

    }
}

