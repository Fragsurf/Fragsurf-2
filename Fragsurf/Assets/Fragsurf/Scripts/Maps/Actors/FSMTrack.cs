using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fragsurf.Actors
{
    public enum FSMTrackType
    {
        Staged  = 0,
        Linear  = 1,
        Bonus   = 2
    }

    [Serializable]
    public class FSMTrackLinear
    {
        public FSMTrigger StartTrigger;
        public FSMTrigger EndTrigger;
        public FSMTrigger[] Checkpoints;

        public bool IsValid()
        {
            if(!StartTrigger || !EndTrigger)
            {
                Debug.LogError("Track is invalid, missing a trigger!");
                return false;
            }
            return true;
        }
    }

    [Serializable]
    public class FSMTrackStage
    {
        public FSMTrackStageData[] Stages;

        public bool IsValid()
        {
            foreach (var stage in Stages)
            {
                if(!stage.StartTrigger || !stage.EndTrigger)
                {
                    Debug.LogError("Track is invalid, missing a trigger!");
                    return false;
                }
            }
            return true;
        }

        [Serializable]
        public class FSMTrackStageData
        {
            public string StageName;
            public FSMTrigger StartTrigger;
            public FSMTrigger EndTrigger;
        }
    }

    [Serializable]
    public class FSMTrackBonus
    {
        public FSMTrigger StartTrigger;
        public FSMTrigger EndTrigger;

        public bool IsValid()
        {
            if(!StartTrigger || !EndTrigger)
            {
                Debug.LogError("Track is invalid, missing a trigger!");
                return false;
            }
            return true;
        }
    }


    public class FSMTrack : FSMActor
    {

        public UnityEvent<Human, int, Timeline> OnStage = new UnityEvent<Human, int, Timeline>();
        public UnityEvent<Human, int, Timeline> OnCheckpoint = new UnityEvent<Human, int, Timeline>();
        public UnityEvent<Human, Timeline> OnFinish = new UnityEvent<Human, Timeline>();
        public UnityEvent<Human, Timeline> OnStart = new UnityEvent<Human, Timeline>();

        private class RunData
        {

            public RunData(Human human, FSMTrack track)
            {
                Track = track;
                Human = human;
            }

            public readonly FSMTrack Track;
            public readonly Human Human;
            public Timeline Timeline;
            public int Checkpoint = -1;
            public int Stage;

            public void Reset()
            {
                Checkpoint = -1;
                Stage = 0;
                Timeline.Reset();
            }

        }

        [Header("Track Options")]
        [SerializeField]
        private string _trackName;
        [SerializeField]
        private FSMTrackType _trackType;
        [SerializeField]
        private FSMTrackLinear _linearData;
        [SerializeField]
        private FSMTrackStage _stageData;
        [SerializeField]
        private FSMTrackBonus _bonusData;

        private List<RunData> _runDatas = new List<RunData>();
        private List<RunData> _stageRunDatas = new List<RunData>();

        public bool IsMainTrack => _trackType != FSMTrackType.Bonus;
        public string TrackName => _trackName;
        public FSMTrackType TrackType => _trackType;

        public FSMTrackLinear LinearData => _linearData;
        public FSMTrackStage StageData => _stageData;
        public FSMTrackBonus BonusData => _bonusData;

        private void OnDestroy()
        {
            _runDatas.Clear();
        }

        private void Start()
        {
            switch (TrackType)
            {
                case FSMTrackType.Linear:
                    if (!_linearData.IsValid())
                    {
                        Debug.LogError("Track is invalid: " + TrackName, gameObject);
                        return;
                    }
                    _linearData.StartTrigger.OnTriggerEnter.AddListener(EnterStartZone);
                    _linearData.StartTrigger.OnTriggerExit.AddListener(ExitStartZone);
                    _linearData.EndTrigger.OnTriggerEnter.AddListener(EnterEndZone);
                    for(int i = 0; i < _linearData.Checkpoints.Length; i++)
                    {
                        var cp = i;
                        _linearData.Checkpoints[i].OnTriggerEnter.AddListener((ent) =>
                        {
                            EnterLinearCheckpoint(cp, ent);
                        });
                    }
                    break;
                case FSMTrackType.Bonus:
                    if (!_bonusData.IsValid())
                    {
                        Debug.LogError("Track is invalid: " + TrackName, gameObject);
                        return;
                    }
                    _bonusData.StartTrigger.OnTriggerEnter.AddListener(EnterStartZone);
                    _bonusData.StartTrigger.OnTriggerEnter.AddListener(ExitStartZone);
                    _bonusData.EndTrigger.OnTriggerEnter.AddListener(EnterEndZone);
                    break;
                case FSMTrackType.Staged:
                    if (!_stageData.IsValid())
                    {
                        Debug.LogError("Track is invalid: " + TrackName, gameObject);
                        return;
                    }
                    for(int i = 0; i < _stageData.Stages.Length; i++)
                    {
                        var stage = i;
                        _stageData.Stages[i].StartTrigger.OnTriggerExit.AddListener((ent) =>
                        {
                            if (ent is Human hu)
                            {
                                Staged_ExitStart(stage, hu);
                            }
                        });
                        _stageData.Stages[i].EndTrigger.OnTriggerEnter.AddListener((ent) =>
                        {
                            if (ent is Human hu)
                            {
                                Staged_EnterEnd(stage, hu);
                            }
                        });
                    }
                    _stageData.Stages[0].StartTrigger.OnTriggerEnter.AddListener(EnterStartZone);
                    _stageData.Stages[0].StartTrigger.OnTriggerExit.AddListener(ExitStartZone);
                    _stageData.Stages[_stageData.Stages.Length - 1].EndTrigger.OnTriggerEnter.AddListener(EnterEndZone);
                    break;
            }


        }

        public override void Tick()
        {
            for (int i = _runDatas.Count - 1; i >= 0; i--)
            {
                if (_runDatas[i].Human == null
                    || !_runDatas[i].Human.IsValid())
                {
                    _runDatas.RemoveAt(i);
                }
            }

            for(int i = _stageRunDatas.Count - 1; i >= 0; i--)
            {
                if(_stageRunDatas[i].Human == null
                    || !_stageRunDatas[i].Human.IsValid())
                {
                    _stageRunDatas.RemoveAt(i);
                }
            }
        }

        private bool TryGetRunData(Human hu, out RunData runData)
        {
            runData = null;
            foreach(var rd in _runDatas)
            {
                if(rd.Human == hu)
                {
                    runData = rd;
                    return true;
                }
            }
            return false;
        }

        private bool TryGetStagedRunData(Human hu, int stage, out RunData runData)
        {
            runData = null;
            foreach (var rd in _stageRunDatas)
            {
                if (rd.Human == hu && rd.Stage == stage)
                {
                    runData = rd;
                    return true;
                }
            }
            return false;
        }

        private void EnterStartZone(NetEntity ent)
        {
            if (!(ent is Human hu))
            {
                return;
            }

            if (TryGetRunData(hu, out RunData runData))
            {
                runData.Reset();
                runData.Timeline.Recording = false;
            }
        }

        private void ExitStartZone(NetEntity ent)
        {
            if (!(ent is Human hu))
            {
                return;
            }

            if(!TryGetRunData(hu, out RunData runData))
            {
                var tl = ent.Game.Get<Timelines>();
                runData = new RunData(hu, this)
                {
                    Checkpoint = -1,
                    Timeline = tl.CreateTimeline(hu)
                };
                _runDatas.Add(runData);
            }

            runData.Reset();
            OnStart?.Invoke(hu, runData.Timeline);
        }

        private void EnterLinearCheckpoint(int idx, NetEntity ent)
        {
            if(!(ent is Human hu)
                || !TryGetRunData(hu, out RunData runData)
                || !runData.Timeline.Recording)
            {
                return;
            }
            runData.Checkpoint = idx;
            runData.Timeline.Checkpoint();
            OnCheckpoint?.Invoke(hu, idx, runData.Timeline);
        }

        private void EnterEndZone(NetEntity ent)
        {
            if (!(ent is Human hu)
                || !TryGetRunData(hu, out RunData runData)
                || !runData.Timeline.Recording)
            {
                return;
            }

            runData.Timeline.Recording = false;

            if(_trackType == FSMTrackType.Linear
                && _linearData.Checkpoints.Length > 0
                && runData.Checkpoint != _linearData.Checkpoints.Length - 1)
            {
                Debug.LogError("Missed a checkpoint...");
                return;
            }

            if(_trackType == FSMTrackType.Staged
                && _stageData.Stages.Length > 0
                && runData.Stage != _stageData.Stages.Length - 1)
            {
                Debug.LogError("Missed a stage...");
                return;
            }

            OnFinish?.Invoke(hu, runData.Timeline);
        }

        private void Staged_ExitStart(int stage, Human hu)
        {
            _stageRunDatas.RemoveAll(x => x.Human == hu && x.Stage != stage);

            if (!TryGetStagedRunData(hu, stage, out RunData rd))
            {
                var tl = hu.Game.Get<Timelines>();
                rd = new RunData(hu, this)
                {
                    Stage = stage,
                    Timeline = tl.CreateTimeline(hu)
                };
                _stageRunDatas.Add(rd);
            }
            rd.Reset();
        }

        private void Staged_EnterEnd(int stage, Human hu)
        {
            if (!TryGetStagedRunData(hu, stage, out RunData runData)
                || !runData.Timeline.Recording)
            {
                return;
            }

            if(TryGetRunData(hu, out RunData linearRunData))
            {
                linearRunData.Checkpoint = stage;
            }

            runData.Timeline.Recording = false;
            OnStage?.Invoke(hu, stage, runData.Timeline);
            _stageRunDatas.Remove(runData);
        }

    }

}

