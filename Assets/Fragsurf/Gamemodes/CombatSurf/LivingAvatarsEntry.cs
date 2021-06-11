using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class LivingAvatarsEntry : EntryElement<LivingAvatarsEntry.Data>
    {

        public class Data
        {
            public BasePlayer Player;
        }

        [SerializeField]
        private GameObject _selfHighlight;
        [SerializeField]
        private GameObject _deadOverlay;
        [SerializeField]
        private SteamAvatar _steamAvatar;

        public BasePlayer Player;

        public override void LoadData(Data data)
        {
            Player = data.Player;
            _steamAvatar.SteamId = data.Player.SteamId;
            _steamAvatar.Fetch();

            var cl = FSGameLoop.GetGameInstance(false);
            _selfHighlight.gameObject.SetActive(cl && data.Player.ClientIndex == cl.ClientIndex);
            _deadOverlay.gameObject.SetActive(false);
        }

        private void Update()
        {
            if(Player != null && Player.Entity is Human hu)
            {
                _deadOverlay.gameObject.SetActive(hu.Dead);
            }
            else if(_deadOverlay.gameObject.activeSelf)
            {
                _deadOverlay.gameObject.SetActive(false);
            }
        }

    }
}

