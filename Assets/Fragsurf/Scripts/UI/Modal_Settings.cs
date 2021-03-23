using Fragsurf.Client;
using Fragsurf.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_Settings : UGuiModal
    {

        [SerializeField]
        private Button _saveButton;
        [SerializeField]
        private Button _cancelButton;

        private Modal_SettingsPageEntry _pageTemplate;
        private Modal_SettingsCategoryEntry _categoryTemplate;
        private Modal_SettingsChangeEntry _changeEntry;

        private Dictionary<string, Modal_SettingsChangeEntry> _pendingChanges = new Dictionary<string, Modal_SettingsChangeEntry>(StringComparer.OrdinalIgnoreCase);

        protected override void Awake()
        {
            base.Awake();

            _pageTemplate = GetComponentInChildren<Modal_SettingsPageEntry>(true);
            _categoryTemplate = GetComponentInChildren<Modal_SettingsCategoryEntry>(true);
            _changeEntry = GetComponentInChildren<Modal_SettingsChangeEntry>(true);
            _pageTemplate.gameObject.SetActive(false);
            _categoryTemplate.gameObject.SetActive(false);
            _changeEntry.gameObject.SetActive(false);

            _saveButton.onClick.AddListener(SaveChanges);
            _cancelButton.onClick.AddListener(ClearChanges);
        }

        private void Start()
        {
            CreatePages();
        }

        protected override void OnOpen()
        {
            ClearChanges();
        }

        protected override void OnClose()
        {
            ClearChanges();
        }

        private void CreatePages()
        {
            var userSettings = DevConsole.GetVariablesWithFlags(ConVarFlags.UserSetting, ConVarFlags.UserSettingHidden);
            var userSettingCategories = new List<string>();

            foreach (var cmd in userSettings)
            {
                var categoryName = cmd.Substring(0, cmd.IndexOf('.'));
                if (userSettingCategories.Contains(categoryName))
                {
                    continue;
                }
                userSettingCategories.Add(categoryName);
            }

            foreach (var category in userSettingCategories)
            {
                var categorySettings = userSettings.FindAll(x => x.StartsWith(category, StringComparison.OrdinalIgnoreCase));
                CreatePage(category, categorySettings);
            }

            var bindSettings = new List<string>()
            {
                "bind/reset",
                "bind/controls/+left",
                "bind/controls/+right"
            };
            foreach (var action in Enum.GetNames(typeof(InputActions)))
            {
                bindSettings.Add("bind/controls/+input " + action);
            }

            CreatePage("controls", bindSettings);

            var bindModals = new List<string>();

            foreach(var modal in UGuiManager.Instance.GetModals())
            {
                bindModals.Add($"modal/{modal.Name}");
            }

            CreatePage("modals", bindModals);
        }

        private Dictionary<string, Tuple<Modal_SettingsPageEntry, Modal_SettingsCategoryEntry>> _pages
            = new Dictionary<string, Tuple<Modal_SettingsPageEntry, Modal_SettingsCategoryEntry>>(StringComparer.OrdinalIgnoreCase);

        public void RemovePage(string pageName)
        {
            if (_pages.ContainsKey(pageName))
            {
                _pageTemplate.Remove(_pages[pageName].Item1);
                _categoryTemplate.Remove(_pages[pageName].Item2);
                _pages.Remove(pageName);
            }
        }

        public void CreatePage(string pageName, List<string> settingNames)
        {
            RemovePage(pageName);

            var pageData = new SettingsPageData()
            {
                SettingNames = settingNames
            };
            var page = _pageTemplate.Append(pageData) as Modal_SettingsPageEntry;
            page.gameObject.SetActive(false);

            var categoryData = new SettingsCategoryData()
            {
                CategoryName = pageName,
                Page = page
            };

            var cat = _categoryTemplate.Append(categoryData) as Modal_SettingsCategoryEntry;

            _pages[pageName] = new Tuple<Modal_SettingsPageEntry, Modal_SettingsCategoryEntry>(page, cat);

            GetInputFields();
        }

        private void SaveChanges()
        {
            foreach (var kvp in _pendingChanges)
            {
                DevConsole.ExecuteLine(kvp.Value.Data.Command);
            }
            ClearChanges();
            //UserSettings.Instance.Save();
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

            foreach (var settingElement in GetComponentsInChildren<SettingElement>(true))
            {
                if (string.IsNullOrEmpty(settingElement.SettingName)
                    || !settingElement.Initialized)
                {
                    continue;
                }
                settingElement.LoadValue();
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

