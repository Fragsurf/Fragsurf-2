using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.UI 
{
    public class ClanSettings : MonoBehaviour
    {

        private ClanSettingsToggleEntry _clanSettingsToggleTemplate;

        private void Start()
        {
            _clanSettingsToggleTemplate = gameObject.GetComponentInChildren<ClanSettingsToggleEntry>();
            _clanSettingsToggleTemplate.gameObject.SetActive(false);

            RefreshClanList();
        }

        private void RefreshClanList()
        {
            if(!SteamClient.IsValid)
            {
                return;
            }
            _clanSettingsToggleTemplate.Clear();
            foreach (var clan in SteamFriends.GetClans())
            {
                _clanSettingsToggleTemplate.Append(new ClanSettingsToggleEntryData()
                {
                    Clan = clan
                });
            }
        }

    }
}


