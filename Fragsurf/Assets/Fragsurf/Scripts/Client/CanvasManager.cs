using Fragsurf.Shared;
using Fragsurf.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fragsurf.Client
{
    [Inject(InjectRealm.Client)]
    public class CanvasManager : FSSharedScript
    {

        //private Canvas _uiCanvas;
        //private const string _modalPathPrefix = "UI/Modals/";

        public static readonly string[] DefaultModals = new string[]
        {
            // todo: something better
            Modal_Crosshair.Identifier,
            "MainMenu",
            Modal_ColorPicker.Identifier,
            Modal_Dialog.Identifier,
            Modal_Perf.Identifier,
            "Browser",
            "Chatbox"
        };

        protected override void _Start()
        {
            //var container = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("UICanvas"));
            //GameObject.DontDestroyOnLoad(container);
            //_uiCanvas = container.GetComponentInChildren<Canvas>();
            //_uiEventSystem = container.GetComponentInChildren<EventSystem>();

            //foreach (var modal in DefaultModals)
            //{
            //    SpawnModal(modal);
            //}
        }

        protected override void OnGameLoaded()
        {
            UGuiManager.Instance.CloseModal<Modal_MainMenu>();
        }

        //private void SpawnModal(string modalPrefabName)
        //{
            //var resourcePath = _modalPathPrefix + modalPrefabName;
            //var resource = Resources.Load<GameObject>(resourcePath);
            //if (!resource)
            //{
            //    Debug.LogError("Modal doesn't exist at path: " + resourcePath);
            //    return;
            //}
            //GameObject.Instantiate<GameObject>(resource, _uiCanvas.transform);
        //}

    }
}

