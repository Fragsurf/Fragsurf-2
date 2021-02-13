using System;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI 
{
    public class ButtonTabs : MonoBehaviour
    {

        [Serializable]
        public class ButtonTab
        {
            public Button Button;
            public GameObject Content;
            [HideInInspector]
            public Color OriginalColor;
        }

        [SerializeField]
        private Color _enabledColor;
        [SerializeField]
        private ButtonTab[] _tabs;

        private ButtonTab _enabledTab;

        private void Start()
        {
            if(_tabs == null || _tabs.Length == 0)
            {
                return;
            }
            foreach(var tab in _tabs)
            {
                var colors = tab.Button.colors;
                colors.disabledColor = Color.white;
                tab.Button.colors = colors;
                if (tab.Button.image)
                {
                    tab.OriginalColor = tab.Button.image.color;
                }
                tab.Button.onClick.AddListener(() => OpenTab(tab));
                CloseTab(tab);
            }
            OpenTab(_tabs[0]);
        }
        
        private void OpenTab(ButtonTab tab)
        {
            if(_enabledTab == tab)
            {
                return;
            }
            if (_enabledTab != null)
            {
                CloseTab(_enabledTab);
            }
            tab.Content.SetActive(true);
            tab.Button.interactable = false;

            if (tab.Button.image)
            {
                tab.Button.image.color = _enabledColor;
            }

            _enabledTab = tab;
        }

        private void CloseTab(ButtonTab tab)
        {
            if(_enabledTab == tab)
            {
                _enabledTab = null;
            }
            tab.Content.SetActive(false);
            tab.Button.interactable = true;

            if (tab.Button.image)
            {
                tab.Button.image.color = tab.OriginalColor;
            }
        }

    }
}

