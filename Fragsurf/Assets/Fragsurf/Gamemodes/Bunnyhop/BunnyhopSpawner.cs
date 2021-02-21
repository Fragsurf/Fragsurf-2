using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [Inject(InjectRealm.Shared, typeof(Bunnyhop))]
    public class BunnyhopSpawner : FSSharedScript
    {

        protected override void OnPlayerIntroduced(IPlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }

            SpawnPlayer(player);
        }

        [ChatCommand("Shows the world record on this map", "wr")]
        public void TEST6() { }

        [ChatCommand("Shows the top players on this map", "top")]
        public void TEST5() { }

        [ChatCommand("Shows your personal record", "pr")]
        public void TEST4() { }

        [ChatCommand("Turn on noclip mode", "noclip")]
        public void TEST3() { }

        [ChatCommand("Teleport to a bonus", "b", "bonus")]
        public void TEST2() { }

        [ChatCommand("Teleport to a stage", "s", "stage")]
        public void TEST() { }

        [ChatCommand("Teleport to the beginning", "r", "spawn", "restart")]
        public void SpawnPlayer(IPlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }

            if(!(player.Entity is Human hu))
            {
                if(player.Entity != null)
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

