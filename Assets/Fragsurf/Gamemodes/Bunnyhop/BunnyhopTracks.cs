using System.Collections.Generic;
using System.IO;
using Fragsurf.Actors;
using Fragsurf.Maps;
using Fragsurf.Movement;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.UI;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [Inject(InjectRealm.Shared, typeof(Bunnyhop))]
    public class BunnyhopTracks : FSSharedScript
    {

        [ConVar("timer.messagecolor", "Color of timer messages", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public Color MessageColor { get; set; } = Color.white;
        [ConVar("timer.speedcolor", "Color of speed variables in messages", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public Color SpeedColor { get; set; } = Color.yellow;
        [ConVar("timer.timecolor", "Color of time variables in messages", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public Color TimeColor { get; set; } = Color.green;
        [ConVar("timer.misccolor", "Color of misc variables in messages (jumps, strafes)", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public Color MiscColor { get; set; } = new Color32(222, 159, 24, 255);
        [ConVar("timer.namecolor", "Color of name variables in messages", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public Color NameColor { get; set; } = Color.cyan;
        [ConVar("timer.name", "Name of timer messages", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public string TimerName { get; set; } = "[Timer]";

        public BaseLeaderboardSystem LeaderboardSystem { get; } = new SteamworksLeaderboardSystem();

        protected override void _Initialize()
        {
            base._Initialize();

            if (Game.IsHost || FSGameLoop.GetGameInstance(true) == null)
            {
                new BTimesSpeedrunMapDataProvider().CreateTracks(Map.Current.Name);
            }

            foreach (var track in GameObject.FindObjectsOfType<FSMTrack>())
            {
                // the x.Game == Game conditional is important because we're tying into MonoBehaviours which exist in shared space.
                track.OnStart.AddListener((x) => { if (x.Game == Game) Track_OnStart(track, x); });
                track.OnFinish.AddListener((x) => { if (x.Game == Game) Track_OnFinish(track, x); });
                track.OnStage.AddListener((x, y) => { if (x.Game == Game) Track_OnStage(track, x, y); });
                track.OnCheckpoint.AddListener((x, y) => { if (x.Game == Game) Track_OnCheckpoint(track, x, y); });
                track.OnStartStage.AddListener((x, y) => { if (x.Game == Game) Track_OnStartStage(track, x, y); });
                track.OnEnterStart.AddListener((x) => { if (x.Game == Game) Track_OnEnterStart(track, x); });
            }
        }

        private void Track_OnEnterStart(FSMTrack track, Human hu)
        {
            var bhopTimeline = new BunnyhopTimeline() { Track = track };
            hu.Timeline = bhopTimeline;
            bhopTimeline.InStartZone = true;
        }

        private void Track_OnStart(FSMTrack track, Human hu)
        {
            var bhopTimeline = hu.Timeline as BunnyhopTimeline;
            if(bhopTimeline == null)
            {
                Debug.LogError("Timeline is null, didn't enter start?");
                return;
            }

            if(hu.MovementController is CSMovementController csm
                && csm.MoveType == MoveType.Noclip)
            {
                return;
            }

            bhopTimeline.InStartZone = false;

            if(hu.HammerVelocity() > 290)
            {
                hu.ClampVelocity(280, Game.GameMovement.JumpPower);
            }
            hu.Record(bhopTimeline);

            bhopTimeline.RecordTick();

            if (!Game.IsHost && hu.OwnerId == Game.ClientIndex && track.TrackType != FSMTrackType.Staged)
            {
                var frame = bhopTimeline.LastFrame;
                var name = track.TrackName;
                if(track.TrackType == FSMTrackType.Bonus)
                {
                    name = "Bonus";
                }
                else if (track.IsMainTrack)
                {
                    name = "Main";
                }
                var msg = $"<color={MessageColor.HashRGBA()}><color={NameColor.HashRGBA()}>{name}</color> started, <color={SpeedColor.HashRGBA()}>{frame.Velocity}u/s</color></color>";
                Game.TextChat.PrintChat(TimerName, msg);
            }
        }

        private void Track_OnStartStage(FSMTrack track, Human hu, int stage)
        {
            if (!(hu.Timeline is BunnyhopTimeline bhopTimeline))
            {
                return;
            }

            if (hu.MovementController is CSMovementController csm
                && csm.MoveType == MoveType.Noclip)
            {
                return;
            }

            if (hu.HammerVelocity() > 290)
            {
                hu.ClampVelocity(280, Game.GameMovement.JumpPower);
            }
            bhopTimeline.SetSegment(stage);

            if (!Game.IsHost && hu.OwnerId == Game.ClientIndex)
            {
                var frame = bhopTimeline.LastFrame;
                var msg = $"<color={MessageColor.HashRGBA()}><color={MiscColor.HashRGBA()}>Stage {stage}</color> started, <color={SpeedColor.HashRGBA()}>{frame.Velocity} u/s</color></color>";
                Game.TextChat.PrintChat(TimerName, msg);
            }
        }

        private async void Track_OnFinish(FSMTrack track, Human hu)
        {
            if(!(hu.Timeline is BunnyhopTimeline bhopTimeline)
                || !bhopTimeline.RunIsLive)
            {
                return;
            }

            bhopTimeline.RunIsLive = false;

            if (!Game.IsHost && hu.OwnerId == Game.ClientIndex)
            {
                var tlCopy = new BunnyhopTimeline()
                {
                    Frames = new List<BunnyhopTimelineFrame>(bhopTimeline.Frames),
                    Entity = hu,
                    Checkpoint = bhopTimeline.Checkpoint,
                    Stage = bhopTimeline.Stage,
                    Track = bhopTimeline.Track
                };
                var id = BaseLeaderboardSystem.GetLeaderboardId(Map.Current.Name, track, MoveStyle.FW);
                var runResult = await LeaderboardSystem.SubmitRunAsync(id, bhopTimeline.LastFrame, tlCopy);
                AnnounceRun(track, hu, runResult, tlCopy.LastFrame);
            }
        }

        private async void Track_OnStage(FSMTrack track, Human hu, int stage)
        {
            if (!(hu.Timeline is BunnyhopTimeline bhopTimeline)
                || !bhopTimeline.RunIsLive)
            {
                return;
            }

            bhopTimeline.Stage = stage;

            if(!Game.IsHost
                && hu.OwnerId == Game.ClientIndex
                && bhopTimeline.GetSegment(stage, out BunnyhopTimelineFrame frame, out BunnyhopTimeline newTimeline))
            {
                var id = BaseLeaderboardSystem.GetLeaderboardId(Map.Current.Name, track, MoveStyle.FW, stage);
                var runResult = await LeaderboardSystem.SubmitRunAsync(id, frame, newTimeline);
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
                var msg = $"<color={MessageColor.HashRGBA()}> Finished <color={NameColor.HashRGBA()}>{trackName}</color> in <color={TimeColor.HashRGBA()}>{timeStr}s</color>, <color={MiscColor.HashRGBA()}>{frame.Jumps} jumps</color> @ rank <color={MiscColor.HashRGBA()}>#{result.NewRank}</color>!</color>";
                Game.TextChat.MessageAll(msg);

                if (result.NewRank < result.OldRank)
                {
                    var improveStr = Bunnyhop.FormatTime(result.Improvement);
                    msg = $"<color={MessageColor.HashRGBA()}>Improvement of <color={TimeColor.HashRGBA()}>{improveStr}s</color></color>";
                    Game.TextChat.MessageAll(msg);
                }
            }
            else
            {
                Game.TextChat.MessageAll($"<color={MessageColor.HashRGBA()}>Finished <color={NameColor.HashRGBA()}>{trackName}</color> in <color={TimeColor.HashRGBA()}>{timeStr}s</color></color>");
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

