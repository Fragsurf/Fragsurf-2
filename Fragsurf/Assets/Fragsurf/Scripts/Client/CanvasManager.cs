using Fragsurf.Shared;
using Fragsurf.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fragsurf.Client
{
    [Inject(InjectRealm.Client)]
    public class CanvasManager : FSSharedScript
    {

        private Canvas _uiCanvas;
        private EventSystem _uiEventSystem;
        private const string _modalPathPrefix = "UI/Modals/";

        public static readonly string[] DefaultModals = new string[]
        {
            // todo: something better
            Modal_Crosshair.Identifier,
            Modal_EscapeMenu.Identifier,
            Modal_Settings.Identifier,
            Modal_ColorPicker.Identifier,
            Modal_Perf.Identifier,
            Modal_Console.Identifier,
            Modal_Dialog.Identifier
        };

        protected override void _Start()
        {
            var container = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("UICanvas"));
            _uiCanvas = container.GetComponentInChildren<Canvas>();
            _uiEventSystem = container.GetComponentInChildren<EventSystem>();

            foreach (var modal in DefaultModals)
            {
                SpawnModal(modal);
            }
        }

        private void SpawnModal(string modalPrefabName)
        {
            var resourcePath = _modalPathPrefix + modalPrefabName;
            var resource = Resources.Load<GameObject>(resourcePath);
            if (!resource)
            {
                Debug.LogError("Modal doesn't exist at path: " + resourcePath);
                return;
            }
            GameObject.Instantiate<GameObject>(resource, _uiCanvas.transform);
        }

        public static void EnableEventSystem()
        {
            var game = FSGameLoop.GetGameInstance(false);
            if (game)
            {
                var cm = game.Get<CanvasManager>();
                if (cm && cm._uiEventSystem)
                {
                    cm._uiEventSystem.enabled = true;
                }
            }
        }

        public static void DisableEventSystem()
        {
            var game = FSGameLoop.GetGameInstance(false);
            if (game)
            {
                var cm = game.Get<CanvasManager>();
                if (cm && cm._uiEventSystem)
                {
                    cm._uiEventSystem.enabled = false;
                }
            }
        }

    }
}

