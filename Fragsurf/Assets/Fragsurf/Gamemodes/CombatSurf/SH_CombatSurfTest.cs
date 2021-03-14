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
            if (Game.IsHost && dmgInfo.ResultedInDeath)
            {
                var props = Game.Get<PlayerProps>();
                var killer = Game.EntityManager.FindEntity<Human>(dmgInfo.AttackerEntityId);
                if(killer != null)
                {
                    props.IncrementProp(killer.OwnerId, "Kills", 1);
                }
                props.IncrementProp(hu.OwnerId, "Deaths", -1);
            }
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

        [ChatCommand("Give an item [AK47/Knife/AWP/Axe/Bat/etc]", "give")]
        public void Give(IPlayer player, string item)
        {
            if (!Game.IsHost || !(player.Entity is Human hu))
            {
                return;
            }
            hu.Give(item);
        }

        [ChatCommand("Spawns a bot", "bot")]
        public void SpawnBot(IPlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }

            var bot = new Human(Game);
            Game.EntityManager.AddEntity(bot);
            bot.BotController = new BotController(bot);
            bot.Spawn(1);
            bot.Give("Knife");
            bot.Give("AK47");
        }

    }
}

