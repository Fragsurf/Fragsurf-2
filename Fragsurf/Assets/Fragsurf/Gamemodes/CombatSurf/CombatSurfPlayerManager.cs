using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;

namespace Fragsurf.Gamemodes.CombatSurf
{
    [Inject(InjectRealm.Server, typeof(CombatSurf))]
    public class CombatSurfPlayerManager : FSSharedScript
    {

        protected override void OnPlayerIntroduced(IPlayer player)
        {
            CreateHuman(player);

            var t1players = 0;
            var t2players = 0;

            foreach(var pl in Game.PlayerManager.Players)
            {
                if(pl.Team == 1)
                {
                    t1players++;
                }
                else if(pl.Team == 2)
                {
                    t2players++;
                }
            }

            if(t1players == 0 || t1players <= t2players)
            {
                Game.PlayerManager.SetPlayerTeam(player, 1);
            }
            else
            {
                Game.PlayerManager.SetPlayerTeam(player, 2);
            }
        }

        protected override void OnPlayerChangedTeam(IPlayer player)
        {
            var teamName = player.Team == 0
                ? "<color=#c0c2c0>Spectators</color>"
                : "<color=green>Team " + player.Team + "</color>";

            Game.TextChat.MessageAll($"<color=yellow>{player.DisplayName}</color> has joined {teamName}");

            if(!(player.Entity is Human hu))
            {
                CreateHuman(player);
                hu = player.Entity as Human;
            }

            if(player.Team <= 0)
            {
                hu.Dead = true;
                hu.OutOfGame = true;
                return;
            }

            hu.OutOfGame = false;

            var rm = Game.Get<RoundManager>();
            if(rm == null
                || rm.MatchState != MatchStates.Live)
            {
                hu.Spawn(player.Team);
                return;
            }

            if(rm.RoundState == RoundStates.Freeze)
            {
                hu.Spawn(player.Team);
            }
            else
            {
                hu.Dead = true;
            }
        }

        private void CreateHuman(IPlayer player)
        {
            if (!(player.Entity is Human hu))
            {
                if (player.Entity != null)
                {
                    player.Entity.Delete();
                }
                hu = new Human(Game);
                player.Entity = hu;
                Game.EntityManager.AddEntity(hu);
                hu.OwnerId = player.ClientIndex;
            }
        }

        [ChatCommand("Give an item [AK47/Knife/AWP/Axe/Bat/etc]", "give")]
        public void Give(IPlayer player, string item)
        {
            if (!(player.Entity is Human hu))
            {
                return;
            }
            hu.Give(item);
        }

        [ChatCommand("Spawns a bot", "bot")]
        public void SpawnBot(IPlayer player)
        {
            var bot = new Human(Game);
            Game.EntityManager.AddEntity(bot);
            bot.BotController = new BotController(bot);
            bot.Spawn(1);
            bot.Give("Knife");
            bot.Give("AK47");
        }

    }
}

