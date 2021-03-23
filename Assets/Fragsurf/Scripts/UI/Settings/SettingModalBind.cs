using System;
using Fragsurf.Client;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Fragsurf.UI
{
    public class SettingModalBind : SettingElement
    {

        [SerializeField]
        private Toggle _holdToggle;
        [SerializeField]
        private Button _button;
        [SerializeField]
        private TMP_Text _buttonText;

        private string _modalName;
        private bool _settingBind;
        private string _currentKey;

        private static List<KeyCode> _keyCodes = new List<KeyCode>();
        private static List<SettingModalBind> _settingBinds = new List<SettingModalBind>();

        protected override void _Initialize()
        {
            if (_keyCodes.Count == 0)
            {
                foreach (KeyCode kc in Enum.GetValues(typeof(KeyCode)))
                {
                    _keyCodes.Add(kc);
                }
            }

            // modal/Name
            var path = SettingName.Split('/');
            _modalName = path[1];
            Setting.SetLabel(SettingName);
            _button.onClick.AddListener(OnClick);
            _holdToggle.isOn = false;
            _holdToggle.onValueChanged.AddListener((v) =>
            {
                if(!string.IsNullOrWhiteSpace(_currentKey))
                {
                    Enqueue(_currentKey);
                } 
            });
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
                            Enqueue(kc.ToString());
                            _buttonText.text = kc.ToString().ToLower();
                            break;
                        }
                    }
                }
                else if (Input.mouseScrollDelta.y != 0)
                {
                    DeactivateAll();
                    var dir = Input.mouseScrollDelta.y > 0
                        ? "mwheelup"
                        : "mwheeldown";
                    Enqueue(dir);
                    _buttonText.text = dir;
                }
            }

            _button.interactable = !_settingBind;
        }

        private void Enqueue(string key)
        {
            _currentKey = key;

            if (_holdToggle.isOn)
            {
                Queue($"hardbind {key} \"+modal {_modalName}\"");
            }
            else
            {
                Queue($"hardbind {key} \"modal.toggle {_modalName}\"");
            }
        }

        private void OnClick()
        {
            foreach (var sb in _settingBinds)
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
            var binds = UserSettings.Binds.FindBindDatas($"modal.toggle {_modalName}");
            var hold = false;

            if(binds.Count == 0)
            {
                binds = UserSettings.Binds.FindBindDatas($"+modal {_modalName}");
                if(binds.Count > 0)
                {
                    hold = true;
                }
            }

            if (binds.Count > 0)
            {
                var keyName = binds[0].KeyName.ToString().ToLower();
                _currentKey = keyName;
                _holdToggle.isOn = hold;
                _buttonText.text = _currentKey;
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

