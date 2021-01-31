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

        private static Modal_SettingsPageEntry _visiblePage;

        public override void LoadData(SettingsCategoryData data)
        {
            _categoryLabel.text = data.CategoryName;
            _pageButton.onClick.AddListener(() => ShowPage(data.Page));
        }

        private void ShowPage(Modal_SettingsPageEntry page)
        {
            if (_visiblePage)
            {
                _visiblePage.gameObject.SetActive(false);
                _visiblePage = null;
            }
            _visiblePage = page;
            page.gameObject.SetActive(true);
        }

    }
}

