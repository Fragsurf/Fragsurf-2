using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class LivingAvatars : MonoBehaviour
    {

        public int TeamNumber;

        private LivingAvatarsEntry _template;

        private void Start()
        {
            _template = gameObject.GetComponentInChildren<LivingAvatarsEntry>();
            _template.gameObject.SetActive(false);

            SpectateController.ScoreboardUpdateNotification += BuildAvatars;

            BuildAvatars();
        }

        private void OnDestroy()
        {
            SpectateController.ScoreboardUpdateNotification -= BuildAvatars;
        }

        private void Update()
        {
            foreach(LivingAvatarsEntry el in _template.ChildrenElements)
            {
                var active = el.Player != null
                    && el.Player.Entity is Human hu
                    && !hu.Dead;
                el.gameObject.SetActive(active);
            }
        }

        public void BuildAvatars()
        {
            _template.Clear();

            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl)
            {
                return;
            }

            foreach(var player in cl.PlayerManager.Players)
            {
                if(player.Team != TeamNumber)
                {
                    continue;
                }
                _template.Append(new LivingAvatarsEntry.Data()
                {
                    Player = player
                });
            }
        }

    }
}

