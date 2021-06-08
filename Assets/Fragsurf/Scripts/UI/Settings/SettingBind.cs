using System;
using Fragsurf.Client;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class SettingBind : SettingElement
    {

        [SerializeField]
        private Button _button;
        [SerializeField]
        private TMP_Text _buttonText;

        private string _action;
        private bool _settingBind;

        private static List<KeyCode> _keyCodes = new List<KeyCode>();
        private static List<SettingBind> _settingBinds = new List<SettingBind>();

        protected override void _Initialize()
        {
            if(_keyCodes.Count == 0)
            {
                foreach (KeyCode kc in Enum.GetValues(typeof(KeyCode)))
                {
                    _keyCodes.Add(kc);
                }
            }

            var path = SettingName.Split('/');
            var header = path[1];
            _action = path[2].Replace('|', '/');
            Setting.SetLabel($"{header}/{_action}");
            _button.onClick.AddListener(OnClick);
        }

        private void OnEnable()
        {
            _settingBinds.Add(this);
            LoadValue();
        }

        private void OnDisable()
        {
            _settingBind = false;
            _settingBinds.Remove(this);
            EnableInput();
        }

        private void Update()
        {
            if (_settingBind)
            {
                if (Input.anyKeyDown)
                {
                    DeactivateAll();
                    foreach (var kc in _keyCodes)
                    {
                        if (Input.GetKeyDown(kc))
                        {
                            if (kc == KeyCode.Escape)
                            {
                                break;
                            }
                            Queue($"hardbind {kc} \"{_action}\"");
                            _buttonText.text = kc.ToString().ToLower();
                            break;
                        }
                    }
                }
                else if(Input.mouseScrollDelta.y != 0)
                {
                    DeactivateAll();
                    var dir = Input.mouseScrollDelta.y > 0
                        ? "mwheelup"
                        : "mwheeldown";
                    Queue($"hardbind {dir} \"{_action}\"");
                    _buttonText.text = dir;
                }
            }

            _button.interactable = !_settingBind;
        }

        private void OnClick()
        {
            foreach(var sb in _settingBinds)
            {
                if (sb._settingBind)
                {
                    return;
                }
            }
            _settingBind = true;
            DisableInput();
        }

        public override void LoadValue(string val)
        {

        }

        public override void LoadValue()
        {
            var binds = UserSettings.Binds.FindBindDatas(_action);
            if (binds != null && binds.Count > 0)
            {
                _buttonText.text = binds[0].KeyName.ToString().ToLower();
                Setting.SetDescription(string.Empty);
            }
            else
            {
                _buttonText.text = "<color=#FF0000>-</color>";
            }
        }

        private void DeactivateAll()
        {
            EnableInput();
            foreach (var b in _settingBinds)
            {
                b._settingBind = false;
            }
        }

        private void DisableInput()
        {
            UGuiManager.DisableEventSystem();
            ConsoleBinds.Blocked = true;
            ClientInput.Blockers.Add(this);

            if (UGuiManager.Instance)
            {
                UGuiManager.Instance.EscapeEnabled = false;
            }
        }

        private void EnableInput()
        {
            UGuiManager.EnableEventSystem();
            ConsoleBinds.Blocked = false;
            ClientInput.Blockers.Remove(this);

            if (UGuiManager.Instance)
            {
                UGuiManager.Instance.EscapeEnabled = true;
            }
        }

    }
}

