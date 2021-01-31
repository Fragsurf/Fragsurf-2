using System;
using System.Collections.Generic;

namespace Fragsurf.UI
{
    public class Modal_Settings : UGuiModal
    {

        public const string Identifier = "Settings";

        private Modal_SettingsPageEntry _pageTemplate;
        private Modal_SettingsCategoryEntry _categoryTemplate;

        private void Start()
        {
            _pageTemplate = GetComponentInChildren<Modal_SettingsPageEntry>(true);
            _categoryTemplate = GetComponentInChildren<Modal_SettingsCategoryEntry>(true);
            _pageTemplate.gameObject.SetActive(false);
            _categoryTemplate.gameObject.SetActive(false);

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

    }
}

