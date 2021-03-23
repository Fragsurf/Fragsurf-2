using Fragsurf.Client;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class SettingResetBinds : SettingElement
    {

        [SerializeField]
        private Button _button;

        protected override void _Initialize()
        {
            Setting.SetLabel("Reset");

            _button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            UserSettings.Instance.ExecuteDefaultBinds();

            foreach(var sb in GameObject.FindObjectsOfType<SettingBind>(true))
            {
                sb.LoadValue();
            }
        }

        public override void LoadValue()
        {
            
        }

        public override void LoadValue(string val)
        {

        }

    }
}

