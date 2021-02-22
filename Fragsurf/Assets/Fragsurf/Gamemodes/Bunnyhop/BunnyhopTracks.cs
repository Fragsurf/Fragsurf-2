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

        private Dictionary<Human, FSMTrack.RunData> _activeRunData = new Dictionary<Human, FSMTrack.RunData>();

        protected override void _Initialize()
        {
            foreach(var track in GameObject.FindObjectsOfType<FSMTrack>())
            {
                track.OnStart.AddListener(Track_OnStart);
                track.OnFinish.AddListener(Track_OnFinish);
                track.OnStage.AddListener(Track_OnStage);
                track.OnCheckpoint.AddListener(Track_OnCheckpoint);

                if (!Game.IsHost)
                {
                    OutlineTrack(track);
                }
            }
        }

        public FSMTrack.RunData GetRunData(Human hu)
        {
            return _activeRunData.ContainsKey(hu) ? _activeRunData[hu] : null;
        }

        private void Track_OnStart(Human hu, FSMTrack.RunData runData)
        {
            // TODO: this should never happen so an event system is needed soon to tuck away this conditional
            // hooking directly into MonoBehaviour events isn't the way to go.
            if (hu.Game != Game) 
            {
                return;
            }
            _activeRunData[hu] = runData;
        }

        private void Track_OnFinish(Human hu, FSMTrack.RunData track)
        {
            if (hu.Game != Game)
            {
                return;
            }
            Debug.Log("Track finished");
        }

        private void Track_OnStage(Human hu, int stage, FSMTrack.RunData track)
        {
            if (hu.Game != Game)
            {
                return;
            }
            Debug.Log("Stage finished: " + stage);
        }

        private void Track_OnCheckpoint(Human hu, int checkpoint, FSMTrack.RunData track)
        {
            if (hu.Game != Game)
            {
                return;
            }
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

