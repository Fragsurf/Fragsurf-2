using Fragsurf.Client;
using Fragsurf.Movement;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_Settings : UGuiModal
    {

        public const string Identifier = "Settings";

        [SerializeField]
        private Button _saveButton;
        [SerializeField]
        private Button _cancelButton;

        private Modal_SettingsPageEntry _pageTemplate;
        private Modal_SettingsCategoryEntry _categoryTemplate;
        private Modal_SettingsChangeEntry _changeEntry;

        private Dictionary<string, Modal_SettingsChangeEntry> _pendingChanges = new Dictionary<string, Modal_SettingsChangeEntry>(StringComparer.OrdinalIgnoreCase);

        private void Start()
        {
            _pageTemplate = GetComponentInChildren<Modal_SettingsPageEntry>(true);
            _categoryTemplate = GetComponentInChildren<Modal_SettingsCategoryEntry>(true);
            _changeEntry = GetComponentInChildren<Modal_SettingsChangeEntry>(true);
            _pageTemplate.gameObject.SetActive(false);
            _categoryTemplate.gameObject.SetActive(false);
            _changeEntry.gameObject.SetActive(false);

            var allSettings = DevConsole.GetVariablesWithFlags(ConVarFlags.UserSetting);
            var categories = new List<string>();

            foreach(var cmd in allSettings)
            {
                var categoryName = cmd.Substring(0, cmd.IndexOf('.'));
                if (categories.Contains(categoryName))
                {
                    continue;
                }
                categories.Add(categoryName);
            }

            foreach(var category in categories)
            {
                var categorySettings = allSettings.FindAll(x => x.StartsWith(category, StringComparison.OrdinalIgnoreCase));
                CreatePage(category, categorySettings);
            }

            CreateBindsPage();

            _saveButton.onClick.AddListener(SaveChanges);
            _cancelButton.onClick.AddListener(ClearChanges);
        }

        protected override void OnClose()
        {
            base.OnClose();

            ClearChanges();
        }

        private void CreatePage(string category, List<string> settingNames)
        {
            var pageData = new SettingsPageData()
            {
                SettingNames = settingNames
            };
            var page = _pageTemplate.Append(pageData) as Modal_SettingsPageEntry;
            page.gameObject.SetActive(false);

            var categoryData = new SettingsCategoryData()
            {
                CategoryName = category,
                Page = page
            };

            _categoryTemplate.Append(categoryData);
        }

        private void CreateBindsPage()
        {
            var settingNames = new List<string>()
            {
                "bind/reset"
            };
            foreach(var action in Enum.GetNames(typeof(InputActions)))
            {
                settingNames.Add("bind/controls/+input " + action);
            }
            foreach(var defaultModal in CanvasManager.DefaultModals)
            {
                settingNames.Add("bind/ui/modal.toggle " + defaultModal);
            }
            CreatePage("binds", settingNames);
        }

        private void SaveChanges()
        {
            foreach (var kvp in _pendingChanges)
            {
                DevConsole.ExecuteLine(kvp.Value.Data.Command);
            }
            ClearChanges();

            foreach(var settingElement in GetComponentsInChildren<SettingElement>(true))
            {
                if (string.IsNullOrEmpty(settingElement.SettingName)
                    || !settingElement.Initialized)
                {
                    continue;
                }
                settingElement.LoadValue();
            }
        }

        private void ClearChanges()
        {
            _pendingChanges.Clear();

            if (_pageTemplate)
            {
                foreach (var child in _pageTemplate.Children)
                {
                    var page = child.GetComponent<Modal_SettingsPageEntry>();
                    foreach (var setting in page.SettingsEntries)
                    {
                        setting.SetPendingChanges(false);
                    }
                }
            }

            if (_changeEntry)
            {
                _changeEntry.Clear();
            }
        }

        public void QueueCommand(string settingName, string value)
        {
            if (_pendingChanges.ContainsKey(settingName)
                && _pendingChanges[settingName])
            {
                _changeEntry.Remove(_pendingChanges[settingName]);
                _pendingChanges.Remove(settingName);
            }
            var changeData = new SettingsChangeEntryData()
            {
                Command = value,
                SettingName = settingName
            };
            var element = _changeEntry.Append(changeData);
            _pendingChanges[settingName] = element as Modal_SettingsChangeEntry;

            foreach (var child in _pageTemplate.Children)
            {
                var page = child.GetComponent<Modal_SettingsPageEntry>();
                foreach (var setting in page.SettingsEntries)
                {
                    if(string.Equals(setting.SettingName, settingName))
                    {
                        setting.SetPendingChanges(true);
                    }
                }
            }
        }

    }
}

