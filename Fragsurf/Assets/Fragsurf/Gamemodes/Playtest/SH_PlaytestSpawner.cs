using Fragsurf.Actors;
using Fragsurf.Maps;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Gamemodes.Playtest
{
    [Inject(InjectRealm.Shared, typeof(PlaytestGamemode))]
    public class SH_PlaytestSpawner : FSSharedScript
    {

        private bool _showTriggers;

        [ConVar("mapmode.testint", "This is a test variable :()", ConVarFlags.Replicator | ConVarFlags.Gamemode)]
        public int TestOkay { get; set; }
        [ConVar("mapmode.testcolor", "This is a test color ():", ConVarFlags.Gamemode)]
        public Color TestColor { get; set; } = Color.red;
        [ConVar("mapmode.showtriggers", "", ConVarFlags.Gamemode)]
        public bool ShowTriggers
        {
            get => _showTriggers;
            set
            {
                _showTriggers = value;
                foreach(var fsmTrigger in FindObjectsOfType<FSMTrigger>())
                {
                    fsmTrigger.EnableRenderers(value);
                }
            }
        }

        protected override void OnPlayerIntroduced(IPlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }

            SpawnPlayer(player);
            Give(player, "Knife");
            Give(player, "AK47");
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

        [ChatCommand("Teleport to the beginning", "r", "spawn", "restart")]
        public void SpawnPlayer(IPlayer player)
        {
            if (!Game.IsHost)
            {
                Game.Get<SpectateController>().Spectate(Human.Local);
                return;
            }

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

        protected override void OnHumanSpawned(Human hu)
        {
            Debug.Log(Game.IsHost + "human spawned");
        }

        protected override void OnHumanKilled(Human hu)
        {
            Debug.Log(Game.IsHost + "human killed");
        }

        protected override void OnHumanDamaged(Human hu, DamageInfo dmgInfo)
        {
            if (Game.IsHost)
            {
                var pp = Game.Get<PlayerProps>();
                var dmg = pp.GetProp(dmgInfo.AttackerEntityId, "Damage");
                pp.SetProp(dmgInfo.AttackerEntityId, "Damage", dmg + dmgInfo.Amount);
            }
        }

    }
}

