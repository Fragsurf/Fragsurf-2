using Fragsurf.Shared;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_MainMenu : UGuiModal
    {

        private void Update()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl || cl.GameLoader.State == GameLoaderState.New)
            {
                if (!IsOpen)
                {
                    Open();
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    var tabs = GetComponentInChildren<ButtonTabs>();
                    if (tabs)
                    {
                        tabs.OpenTab(0);
                    }
                }
            }
        }

        public void OpenModal(string name)
        {
            UGuiManager.Instance.OpenModal(name);
        }

        public void DisconnectFromGame()
        {
            var dialog = UGuiManager.Instance.Find<Modal_Dialog>();
            if (!dialog)
            {
                DevConsole.ExecuteLine("net.disconnect");
                return;
            }
            dialog.Confirmation("Disconnect from server", "Are you sure you want to leave this game?", () =>
            {
                DevConsole.ExecuteLine("net.disconnect");
            });
        }

        public void OpenUrl(string url)
        {
            var dialog = UGuiManager.Instance.Find<Modal_Dialog>();
            if (!dialog)
            {
                Application.OpenURL(url);
                return;
            }
            dialog.Confirmation("Go to website?", "This will open your browser to the following address: " + url, () =>
            {
                Application.OpenURL(url);
            });
        }

        public void QuitGame()
        {
            var dialog = UGuiManager.Instance.Find<Modal_Dialog>();
            if (!dialog)
            {
                FSGameLoop.Quit();
                return;
            }
            dialog.Confirmation("Exit Fragsurf", "Are you sure you want to quit the game?", () =>
            {
                FSGameLoop.Quit();
            });
        }

    }
}
