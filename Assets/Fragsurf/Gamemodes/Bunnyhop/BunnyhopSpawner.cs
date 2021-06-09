using Fragsurf.Actors;
using Fragsurf.Client;
using Fragsurf.Maps;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Linq;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [Inject(InjectRealm.Shared, typeof(Bunnyhop))]
    public class BunnyhopSpawner : FSSharedScript
    {

        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }

            SpawnAtStart(player);
        }

        [ChatCommand("Teleport to a start zone", "r", "restart")]
        public void SpawnAtStart(BasePlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }

            SpawnPlayer(player);

            if(Map.Current == null)
            {
                return;
            }

            var track = Map.Current.Actors.FirstOrDefault(x => x is FSMTrack) as FSMTrack;
            if(track == null)
            {
                return;
            }

            FSMTrigger startTrig = null;

            switch(track.TrackType)
            {
                case FSMTrackType.Linear:
                    startTrig = track.LinearData.StartTrigger;
                    break;
                case FSMTrackType.Staged:
                    if(track.StageData.Stages == null || track.StageData.Stages.Length == 0)
                    {
                        break;
                    }
                    startTrig = track.StageData.Stages[0].StartTrigger;
                    break;
                case FSMTrackType.Bonus:
                    startTrig = track.BonusData.StartTrigger;
                    break;
            }

            if(startTrig == null)
            {
                return;
            }

            player.Entity.Origin = startTrig.transform.position;
        }

        [ChatCommand("Spawn your human", "spawn")]
        public void SpawnPlayer(BasePlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }

            Game.PlayerManager.SetPlayerTeam(player, 1);

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

        [ChatCommand("Give an item [AK47/Knife/AWP/Axe/Bat/etc]", "give")]
        public void Give(BasePlayer player, string item)
        {
            if (!Game.IsHost || !(player.Entity is Human hu))
            {
                return;
            }
            hu.Give(item);
        }

        [ChatCommand("Enter noclip mode", "noclip", "nc")]
        public void NoclipCmd(BasePlayer player)
        {
            if(!(player.Entity is Human hu)
                || !(hu.MovementController is CSMovementController csm))
            {
                return;
            }

            csm.MoveType = csm.MoveType == Movement.MoveType.Noclip
                ? Movement.MoveType.Walk
                : Movement.MoveType.Noclip;

            if(hu.Timeline is BunnyhopTimeline tl)
            {
                tl.RunIsLive = false;
            }
        }

    }
}

