using Fragsurf.Client;
using Fragsurf.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_EscapeMenu : UGuiModal
    {

        public const string Identifier = "EscapeMenu";

        [Header("Buttons")]

        [SerializeField]
        private Button _returnToGame;
        [SerializeField]
        private Button _settings;
        [SerializeField]
        private Button _serverBrowser;
        [SerializeField]
        private Button _disconnect;
        [SerializeField]
        private Button _quit;

        private void Start()
        {
            _returnToGame.onClick.AddListener(Close);
            _settings.onClick.AddListener(() =>
            {
                UGuiManager.Instance.OpenModal<Modal_Settings>();
            });
            _serverBrowser.onClick.AddListener(() =>
            {
                UGuiManager.Instance.OpenModal("ServerBrowser");
            });
            _disconnect.onClick.AddListener(() =>
            {
                var game = FSGameLoop.GetGameInstance(false);
                if (game)
                {
                    var csm = game.Network as ClientSocketManager;
                    csm.Disconnect();
                }
            });
            _quit.onClick.AddListener(() =>
            {
                var game = FSGameLoop.GetGameInstance(false);
                if (game)
                {
                    game.Quit();
                }
            });
        }

    }
}

