using Fragsurf.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Client
{
    [Inject(InjectRealm.Client)]
    public class CanvasManager : FSSharedScript
    {

        private Canvas _uiCanvas;
        private const string _modalPathPrefix = "UI/Modals/";

        private List<string> _defaultModals = new List<string>()
        {
            "Crosshair",
            "EscapeMenu",
            "Console"
        };

        protected override void _Start()
        {
            _uiCanvas = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("UICanvas"))
                .GetComponentInChildren<Canvas>();

            foreach(var modal in _defaultModals)
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

    }
}

