using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.Tricksurf
{
    [Inject(InjectRealm.Shared, typeof(Tricksurf))]
    public class TricksurfSpawner : FSSharedScript
    {

        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }
            SpawnHuman(player);
        }

        [ChatCommand("Respawn", "r", "restart", "spawn")]
        public void Respawn(BasePlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }
            SpawnHuman(player);
        }

        private void SpawnHuman(BasePlayer player)
        {
            Game.PlayerManager.SetPlayerTeam(player, 1);

            if (!(player.Entity is Human hu))
            {
                if (player.Entity != null)
                {
                    player.Entity.Delete();
                }
                hu = new Human(Game);
                Game.EntityManager.AddEntity(hu);
                hu.OwnerId = player.ClientIndex;
            }

            Game.Get<SH_Tricksurf>().InvalidateTrack(player);

            hu.Spawn();
        }

    }
}

