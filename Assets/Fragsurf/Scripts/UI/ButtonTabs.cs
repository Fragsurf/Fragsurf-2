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

                if(tab.Content.TryGetComponent(out UGuiModal modal))
                {
                    modal.OnOpened.AddListener(() =>
                    {
                        OpenTab(tab, false);
                    });
                    modal.OnClosed.AddListener(() =>
                    {
                        CloseTab(tab, false);
                    });
                }

                CloseTab(tab);
            }
            OpenTab(_tabs[0]);
        }

        public void OpenTab(int index)
        {
            if(_tabs == null || _tabs.Length <= index || index < 0)
            {
                return;
            }
            OpenTab(_tabs[index]);
        }
        
        private void OpenTab(ButtonTab tab, bool openModal = true)
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

            if(openModal && tab.Content.TryGetComponent(out UGuiModal modal))
            {
                modal.Open();
            }

            _enabledTab = tab;
        }

        private void CloseTab(ButtonTab tab, bool closeModal = true)
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

            if (closeModal && tab.Content.TryGetComponent(out UGuiModal modal))
            {
                if (modal is Modal_Console console)
                {
                    console.CloseParentModalOnClose = false;
                }
                modal.Close();
            }
        }

    }
}

