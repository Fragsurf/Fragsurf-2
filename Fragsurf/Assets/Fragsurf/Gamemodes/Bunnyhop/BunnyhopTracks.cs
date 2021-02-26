using Fragsurf.Actors;
using Fragsurf.Maps;
using Fragsurf.Movement;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;
using System.Collections.Generic;
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
                track.OnStartStage.AddListener((x, y) => { if (x.Game == Game) Track_OnStartStage(track, x, y); });
            }
        }

        private void Track_OnStart(FSMTrack track, Human hu)
        {
            hu.ClampVelocity(290, Game.GameMovement.JumpPower);
            hu.Record(new BunnyhopTimeline());
        }

        private void Track_OnStartStage(FSMTrack track, Human hu, int stage)
        {
            if (!(hu.Timeline is BunnyhopTimeline bhopTimeline))
            {
                return;
            }

            hu.ClampVelocity(290, Game.GameMovement.JumpPower);
            bhopTimeline.SetSegment(stage);
        }

        private async void Track_OnFinish(FSMTrack track, Human hu)
        {
            if(!(hu.Timeline is BunnyhopTimeline bhopTimeline))
            {
                return;
            }

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
            if (!(hu.Timeline is BunnyhopTimeline bhopTimeline))
            {
                return;
            }

            bhopTimeline.Stage = stage;

            if(!Game.IsHost
                && hu.OwnerId == Game.ClientIndex
                && bhopTimeline.GetSegment(stage, out BunnyhopTimelineFrame frame, out byte[] data))
            {
                var id = BaseLeaderboardSystem.GetLeaderboardId(Map.Current.Name, track, MoveStyle.FW, stage);
                var runResult = await LeaderboardSystem.SubmitRunAsync(id, frame, data);
                AnnounceRun(track, hu, runResult, frame, stage);
            }
        }

        private void AnnounceRun(FSMTrack track, Human hu, SubmitResponse result, BunnyhopTimelineFrame frame, int number = 0)
        {
            var player = Game.PlayerManager.FindPlayer(hu.OwnerId);

            if (!result.Success || player == null)
            {
                return;
            }

            var timeStr = Bunnyhop.FormatTime(result.TimeMilliseconds);
            string trackName;

            if(track.TrackType == FSMTrackType.Staged && number > 0)
            {
                trackName = $"[Stage {number}]";
            }
            else if(track.TrackType == FSMTrackType.Bonus)
            {
                trackName = $"[Bonus {number}]";
            }
            else
            {
                trackName = $"[{track.TrackName}/{track.TrackType}]";
            }

            if (result.Improved)
            {
                var msg = $"Finished {trackName} in <color=green>{timeStr}</color>s, {frame.Jumps} jumps @ rank <color=#34ebcc>#{result.NewRank}</color>!";
                Game.TextChat.MessageAll(msg);

                if (result.NewRank < result.OldRank)
                {
                    var improveStr = Bunnyhop.FormatTime(result.Improvement);
                    msg = $"Improvement of <color=green>{improveStr}</color>s";
                    Game.TextChat.MessageAll(msg);
                }
            }
            else
            {
                Game.TextChat.MessageAll($"Finished {trackName} in {timeStr}s");
            }

            if (result.Improved && result.NewRank == 1)
            {
                var takeoverStr = Bunnyhop.FormatTime(result.Takeover);
                Game.TextChat.MessageAll($"<color=#f04dff><b>**NEW WORLD RECORD!**</b></color>   <color=#ff4d4d>-{takeoverStr}</color>s");
            }
        }

        private void Track_OnCheckpoint(FSMTrack track, Human hu, int checkpoint)
        {
            if (!(hu.Timeline is BunnyhopTimeline bhopTimeline))
            {
                return;
            }

            bhopTimeline.Checkpoint = checkpoint + 1;
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

