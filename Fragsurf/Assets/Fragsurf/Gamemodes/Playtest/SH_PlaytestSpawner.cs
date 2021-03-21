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

        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            if (!Game.IsServer)
            {
                return;
            }

            SpawnPlayer(player);
            Give(player, "AK47");
            Give(player, "M1911");
            Give(player, "Knife");
        }

        [ChatCommand("Give an item [AK47/Knife/AWP/Axe/Bat/etc]", "give")]
        public void Give(BasePlayer player, string item)
        {
            if (!Game.IsServer || !(player.Entity is Human hu))
            {
                return;
            }
            hu.Give(item);
        }

        [ChatCommand("Change team", "team")]
        public void Team(BasePlayer player, int team)
        {
            if (!Game.IsServer)
            {
                return;
            }
            Game.PlayerManager.SetPlayerTeam(player, (byte)team);
        }

        [ChatCommand("Spawns a bot", "bot")]
        public void SpawnBot(BasePlayer player)
        {
            if (!Game.IsServer)
            {
                return;
            }

            Game.PlayerManager.CreateFakePlayer("Bot");
        }

        [ChatCommand("Teleport to the beginning", "r", "spawn", "restart")]
        public void SpawnPlayer(BasePlayer player)
        {
            if (!Game.IsServer)
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

            if (player.IsFake)
            {
                hu.BotController = new BotController(hu);
            }

            hu.Spawn();
        }

        protected override void OnHumanSpawned(Human hu)
        {
            Debug.Log(Game.IsServer + "human spawned");
        }

        protected override void OnHumanKilled(Human hu)
        {
            Debug.Log(Game.IsServer + "human killed");
        }

        protected override void OnHumanDamaged(Human hu, DamageInfo dmgInfo)
        {
            if (Game.IsServer)
            {
                var pp = Game.Get<PlayerProps>();
                var dmg = pp.GetProp(dmgInfo.AttackerEntityId, "Damage");
                pp.SetProp(dmgInfo.AttackerEntityId, "Damage", dmg + dmgInfo.Amount);
            }
        }

    }
}

