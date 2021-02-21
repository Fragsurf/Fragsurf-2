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

