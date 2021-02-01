using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class SettingsCategoryData
    {
        public string CategoryName;
        public Modal_SettingsPageEntry Page;
    }

    public class Modal_SettingsCategoryEntry : EntryElement<SettingsCategoryData>
    {

        [SerializeField]
        private TMP_Text _categoryLabel;
        [SerializeField]
        private Button _pageButton;

        private Modal_SettingsPageEntry _page;

        private static Modal_SettingsCategoryEntry _selectedCategory;

        public override void LoadData(SettingsCategoryData data)
        {
            _categoryLabel.text = data.CategoryName;
            _page = data.Page;
            _pageButton.onClick.AddListener(ShowPage);
        }

        private void ShowPage()
        {
            if (_selectedCategory)
            {
                _selectedCategory._pageButton.interactable = true;
                _selectedCategory._page.gameObject.SetActive(false);
                _selectedCategory = null;
            }
            _selectedCategory = this;
            _selectedCategory._pageButton.interactable = false;
            _page.gameObject.SetActive(true);
        }

    }
}

