using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    [Inject(InjectRealm.Shared, typeof(CombatSurf))]
    public class SH_CombatSurfTest : FSSharedScript
    {

        protected override void OnPlayerIntroduced(IPlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }

            SpawnPlayer(player);
        }

        protected override void OnHumanKilled(Human hu)
        {
        }

        protected override void OnHumanDamaged(Human hu, DamageInfo dmgInfo)
        {
            Debug.Log(Game.IsHost + ":" + dmgInfo.Amount);
        }

        private void SpawnPlayer(IPlayer player)
        {
            if (!Game.IsHost)
            {
                Game.Get<SpectateController>().Spectate(Human.Local);
                return;
            }

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

            hu.Spawn();
        }

    }
}

