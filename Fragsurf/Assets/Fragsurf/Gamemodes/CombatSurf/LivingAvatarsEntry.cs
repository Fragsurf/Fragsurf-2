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
        private SteamAvatar _steamAvatar;

        public BasePlayer Player;

        public override void LoadData(Data data)
        {
            Player = data.Player;
            _steamAvatar.SteamId = data.Player.SteamId;
            _steamAvatar.Fetch();
        }

    }
}

