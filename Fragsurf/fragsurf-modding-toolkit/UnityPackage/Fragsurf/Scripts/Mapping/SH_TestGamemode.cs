using Fragsurf.FSM.Actors;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using UnityEngine;

namespace Fragsurf.Mapping
{
    [Inject(InjectRealm.Shared, typeof(TestGamemode))]
    public class SH_TestGamemode : FSSharedScript
    {

        protected override void OnPlayerIntroduced(IPlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }
            var spawnPos = Vector3.zero;
            var spawnAngles = Vector3.zero;
            var spawnPoint = FindObjectOfType<FSMSpawnPoint>();
            if(spawnPoint != null)
            {
                spawnPos = spawnPoint.transform.position;
                spawnAngles = spawnPoint.transform.eulerAngles;
            }
            var ent = new Human(Game);
            ent.Origin = spawnPos;
            ent.Angles = spawnAngles;
            ent.OwnerId = player.ClientIndex;
            player.Entity = ent;
            Game.EntityManager.AddEntity(ent);
        }

    }
}

