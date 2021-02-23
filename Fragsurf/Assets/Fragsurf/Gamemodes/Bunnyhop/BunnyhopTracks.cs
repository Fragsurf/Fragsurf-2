using Fragsurf.Actors;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [Inject(InjectRealm.Shared, typeof(Bunnyhop))]
    public class BunnyhopTracks : FSSharedScript
    {

        protected override void _Initialize()
        {
            foreach(var track in GameObject.FindObjectsOfType<FSMTrack>())
            {
                track.OnStart.AddListener((x) => { if (x.Game == Game) Track_OnStart(track, x); });
                track.OnFinish.AddListener((x) => { if (x.Game == Game) Track_OnFinish(track, x); });
                track.OnStage.AddListener((x, y) => { if (x.Game == Game) Track_OnStage(track, x, y); });
                track.OnCheckpoint.AddListener((x, y) => { if (x.Game == Game) Track_OnCheckpoint(track, x, y); });

                if (!Game.IsHost)
                {
                    OutlineTrack(track);
                }
            }
        }

        private void Track_OnStart(FSMTrack track, Human hu)
        {
            hu.Record(new BunnyhopTimeline(track));
        }

        private void Track_OnFinish(FSMTrack track, Human hu)
        {
            if (hu.Game != Game)
            {
                return;
            }

            (hu.Timeline as BunnyhopTimeline).RunIsLive = false;

            var data = hu.Timeline.Serialize();
            var recreated = EntityTimeline.Deserialize<BunnyhopTimeline>(data);

            //var compressed = Compress(data);
            //Debug.Log("original: " + data.Length + ", compressed: " + compressed.Length);
            

            if (Game.IsHost)
            {
                var dummy = new Human(Game);
                Game.EntityManager.AddEntity(dummy);
                dummy.Replay(recreated);
            }
        }

        private void Track_OnStage(FSMTrack track, Human hu, int stage)
        {
            if (hu.Game != Game)
            {
                return;
            }

            (hu.Timeline as BunnyhopTimeline).Stage = stage;
        }

        private void Track_OnCheckpoint(FSMTrack track, Human hu, int checkpoint)
        {
            if (hu.Game != Game)
            {
                return;
            }

            (hu.Timeline as BunnyhopTimeline).Checkpoint = checkpoint + 1;
        }

        private void OutlineTrack(FSMTrack track)
        {
            switch (track.TrackType)
            {
                case FSMTrackType.Linear:
                    OutlineTrigger(track.LinearData.StartTrigger, Color.green);
                    OutlineTrigger(track.LinearData.EndTrigger, Color.red);
                    foreach(var cp in track.LinearData.Checkpoints)
                    {
                        OutlineTrigger(cp, Color.yellow);
                    }
                    break;
                case FSMTrackType.Staged:
                    foreach (var stage in track.StageData.Stages)
                    {
                        OutlineTrigger(stage.StartTrigger, Color.green);
                        OutlineTrigger(stage.EndTrigger, Color.red);
                    }
                    break;
                case FSMTrackType.Bonus:
                    OutlineTrigger(track.BonusData.StartTrigger, Color.green);
                    OutlineTrigger(track.BonusData.EndTrigger, Color.red);
                    break;
            }
        }

        private void OutlineTrigger(FSMTrigger trigger, Color color)
        {
            foreach(var mf in trigger.GetComponentsInChildren<MeshFilter>())
            {
                LineHelper.GenerateOutline(mf, color, 2f);
            }
        }

    }
}

