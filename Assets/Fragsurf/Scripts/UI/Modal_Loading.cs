using Fragsurf.Maps;
using Fragsurf.Shared;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_Loading : UGuiModal
    {

        [SerializeField]
        private TMP_Text _loadingHint;

        private void Update()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if(cl == null || cl.GameLoader == null)
            {
                Close();
                return;
            }

            var loading = cl.GameLoader.Loading;
            if(loading && !IsOpen)
            {
                Open();
            }
            else if(!loading && IsOpen)
            {
                Close();
            }

            if(loading && _loadingHint)
            {
                _loadingHint.text = cl.GameLoader.LoadingHint;
            }
        }

    }
}

