using Fragsurf.Actors;
using Fragsurf.Maps;
using Fragsurf.Movement;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [Inject(InjectRealm.Shared, typeof(Bunnyhop))]
    public class BunnyhopTracks : FSSharedScript
    {

        public BaseLeaderboardSystem LeaderboardSystem { get; } = new SteamworksLeaderboardSystem();

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
            hu.ClampVelocity(290, Game.GameMovement.JumpPower);   
            hu.Record(new BunnyhopTimeline(track));
        }

        private async void Track_OnFinish(FSMTrack track, Human hu)
        {
            var bhopTimeline = hu.Timeline as BunnyhopTimeline;
            bhopTimeline.RunIsLive = false;

            if (!Game.IsHost && hu.OwnerId == Game.ClientIndex)
            {
                var data = hu.Timeline.Serialize();
                var id = BaseLeaderboardSystem.GetLeaderboardId(Map.Current.Name, track, MoveStyle.FW);
                var runResult = await LeaderboardSystem.SubmitRunAsync(id, bhopTimeline.CurrentFrame, data);
                AnnounceRun(track, hu, runResult, bhopTimeline.CurrentFrame);
            }
        }

        private async void Track_OnStage(FSMTrack track, Human hu, int stage)
        {
            var bhopTimeline = hu.Timeline as BunnyhopTimeline;
            bhopTimeline.Stage = stage;

            if (!Game.IsHost && hu.OwnerId == Game.ClientIndex)
            {
                var data = hu.Timeline.Serialize();
                var id = BaseLeaderboardSystem.GetLeaderboardId(Map.Current.Name, track, MoveStyle.FW, stage);
                var runResult = await LeaderboardSystem.SubmitRunAsync(id, bhopTimeline.CurrentFrame, data);
                AnnounceRun(track, hu, runResult, bhopTimeline.CurrentFrame);
            }
        }

        private void AnnounceRun(FSMTrack track, Human hu, SubmitResponse result, BunnyhopTimelineFrame frame)
        {
            var player = Game.PlayerManager.FindPlayer(hu.OwnerId);

            if (!result.Success || player == null)
            {
                return;
            }

            var timeStr = Bunnyhop.FormatTime(result.TimeMilliseconds);

            if (result.Improved)
            {
                var msg = $"Finished {track.TrackName}/{track.TrackType} in <color=green>{timeStr}</color>s, {frame.Jumps} jumps @ rank <color=#34ebcc>#{result.NewRank}</color>!";
                Game.TextChat.MessageAll(msg);

                if (result.NewRank < result.OldRank)
                {
                    var improveStr = Bunnyhop.FormatTime(result.Improvement);
                    msg = $"Improvement of <color=green>{improveStr}</color>s";
                    Game.TextChat.MessageAll(msg);
                }

                if (result.NewRank == 1)
                {
                    var takeoverStr = Bunnyhop.FormatTime(result.Takeover);
                    Game.TextChat.MessageAll($"<color=#f04dff><b>**NEW WORLD RECORD!**</b></color>   <color=#ff4d4d>-{takeoverStr}</color>s");
                }
            }
            else
            {
                Game.TextChat.MessageAll($"Finished {track.TrackName}/{track.TrackType} in {timeStr}s");
            }
        }

        private void Track_OnCheckpoint(FSMTrack track, Human hu, int checkpoint)
        {
            (hu.Timeline as BunnyhopTimeline).Checkpoint = checkpoint + 1;
        }

        [ChatCommand("Open the Replay Tools modal", "replaytools")]
        public void OpenReplayTools()
        {
            if (Game.IsHost)
            {
                return;
            }
            UGuiManager.Instance.ToggleModal<Modal_ReplayTools>();
        }

        [ChatCommand("Open the Replay List modal", "replays")]
        public void OpenReplays()
        {
            if (Game.IsHost)
            {
                return;
            }
            UGuiManager.Instance.ToggleModal<Modal_ReplayList>();
        }

        [ChatCommand("Open the Ranks modal", "ranks", "top", "leaderboard", "ldb")]
        public void OpenRanks()
        {
            if (Game.IsHost)
            {
                return;
            }
            UGuiManager.Instance.ToggleModal<Modal_BunnyhopRanks>();
        }

    }
}

