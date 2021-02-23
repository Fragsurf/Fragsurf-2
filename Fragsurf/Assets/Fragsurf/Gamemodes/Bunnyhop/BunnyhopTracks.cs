using Fragsurf.Actors;
using Fragsurf.Maps;
using Fragsurf.Movement;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [Inject(InjectRealm.Shared, typeof(Bunnyhop))]
    public class BunnyhopTracks : FSSharedScript
    {

        private BaseLeaderboardSystem _leaderboardSystem = new SteamworksLeaderboardSystem();

        protected override void _Initialize()
        {
            foreach(var track in GameObject.FindObjectsOfType<FSMTrack>())
            {
                track.OnStart.AddListener((x) => { if (x.Game == Game) Track_OnStart(track, x); });
                track.OnFinish.AddListener((x) => { if (x.Game == Game) Track_OnFinish(track, x); });
                track.OnStage.AddListener((x, y) => { if (x.Game == Game) Track_OnStage(track, x, y); });
                track.OnCheckpoint.AddListener((x, y) => { if (x.Game == Game) Track_OnCheckpoint(track, x, y); });
            }
        }

        private void Track_OnStart(FSMTrack track, Human hu)
        {
            hu.Record(new BunnyhopTimeline(track));
        }

        private async void Track_OnFinish(FSMTrack track, Human hu)
        {
            var bhopTimeline = hu.Timeline as BunnyhopTimeline;
            bhopTimeline.RunIsLive = false;

            if (!Game.IsHost && hu.OwnerId == Game.ClientIndex)
            {
                var data = hu.Timeline.Serialize();
                var id = GetLeaderboardId(track);
                await _leaderboardSystem.SubmitRunAsync(id, bhopTimeline.CurrentFrame, data);
            }
        }

        private async void Track_OnStage(FSMTrack track, Human hu, int stage)
        {
            var bhopTimeline = hu.Timeline as BunnyhopTimeline;
            bhopTimeline.Stage = stage;

            if (!Game.IsHost && hu.OwnerId == Game.ClientIndex)
            {
                var data = hu.Timeline.Serialize();
                var id = GetLeaderboardId(track, stage);
                await _leaderboardSystem.SubmitRunAsync(id, bhopTimeline.CurrentFrame, data);
            }
        }

        private void Track_OnCheckpoint(FSMTrack track, Human hu, int checkpoint)
        {
            (hu.Timeline as BunnyhopTimeline).Checkpoint = checkpoint + 1;
        }

        private LeaderboardIdentifier GetLeaderboardId(FSMTrack track, int number = 0)
        {
            return new LeaderboardIdentifier()
            {
                Map = Map.Current.Name,
                Number = number,
                Style = MoveStyle.FW,
                TrackName = track.TrackName,
                TrackType = track.TrackType
            };
        }

    }
}

