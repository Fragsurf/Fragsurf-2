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
            GetLoading(out bool loading, out string hint);

            if (loading && !IsOpen)
            {
                Open();
            }
            else if (!loading && IsOpen)
            {
                Close();
            }

            if (loading && _loadingHint)
            {
                _loadingHint.text = hint;
            }
        }

        private void GetLoading(out bool loading, out string hint)
        {
            loading = false;
            hint = string.Empty;

            var sv = FSGameLoop.GetGameInstance(true);
            if(sv)
            {
                switch(sv.GameLoader.State)
                {
                    case GameLoaderState.New:
                    case GameLoaderState.Creating:
                    case GameLoaderState.ChangingMap:
                        loading = true;
                        hint = "Creating server";
                        return;
                }
            }

            var cl = FSGameLoop.GetGameInstance(false);
            if (cl == null || cl.GameLoader == null)
            {
                loading = false;
                return;
            }

            loading = cl.GameLoader.Loading;
            hint = cl.GameLoader.LoadingHint;
        }

    }
}

