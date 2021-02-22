using Fragsurf.Actors;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [Inject(InjectRealm.Shared, typeof(Bunnyhop))]
    public class BunnyhopTracks : FSSharedScript
    {

        public class RunState
        {
            public FSMTrack Track;
            public Timeline Timeline;
            public bool Live;
            public int Checkpoint = 1;
            public int Stage = 1;
        }

        private Dictionary<Human, RunState> _runStates = new Dictionary<Human, RunState>();

        public bool TryGetRunState(Human hu, out RunState runState)
        {
            return _runStates.TryGetValue(hu, out runState);   
        }

        protected override void _Initialize()
        {
            foreach(var track in GameObject.FindObjectsOfType<FSMTrack>())
            {
                track.OnStart.AddListener((x, y) => { if (x.Game == Game) Track_OnStart(track, x, y); });
                track.OnFinish.AddListener((x, y) => { if (x.Game == Game) Track_OnFinish(track, x, y); });
                track.OnStage.AddListener((x, y, z) => { if (x.Game == Game) Track_OnStage(track, x, y, z); });
                track.OnCheckpoint.AddListener((x, y, z) => { if (x.Game == Game) Track_OnCheckpoint(track, x, y, z); });

                if (!Game.IsHost)
                {
                    OutlineTrack(track);
                }
            }
        }

        private void Track_OnStart(FSMTrack track, Human hu, Timeline timeline)
        {
            if (!_runStates.ContainsKey(hu))
            {
                _runStates[hu] = new RunState();
            }
            _runStates[hu].Track = track;
            _runStates[hu].Timeline = timeline;
            _runStates[hu].Checkpoint = 1;
            _runStates[hu].Stage = 1;
            _runStates[hu].Live = true;
        }

        private void Track_OnFinish(FSMTrack track, Human hu, Timeline timeline)
        {
            if (hu.Game != Game)
            {
                return;
            }

            _runStates[hu].Live = false;

            if (!Game.IsHost)
            {
                Game.Get<Timelines>().Replay(timeline);
            }
            Debug.Log("Track finished");
        }

        private void Track_OnStage(FSMTrack track, Human hu, int stage, Timeline timeline)
        {
            if (hu.Game != Game)
            {
                return;
            }
            _runStates[hu].Stage = stage;
            Debug.Log("Stage finished: " + stage);
        }

        private void Track_OnCheckpoint(FSMTrack track, Human hu, int checkpoint, Timeline timeline)
        {
            if (hu.Game != Game)
            {
                return;
            }
            _runStates[hu].Checkpoint = checkpoint;
            Debug.Log("Checkpoint finished: " + checkpoint);
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

