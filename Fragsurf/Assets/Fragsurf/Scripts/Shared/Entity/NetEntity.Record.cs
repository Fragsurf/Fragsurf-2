using MessagePack;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public partial class NetEntity
    {

        public enum TimelineModes
        {
            None,
            Record,
            Replay
        }

        public TimelineModes TimelineMode { get; private set; }
        public EntityTimeline Timeline { get; private set; }

        public void Record<T>(T timeline)
            where T : EntityTimeline
        {
            Timeline = timeline;
            Timeline.Entity = this;
            TimelineMode = TimelineModes.Record;
        }

        public void Replay(EntityTimeline timeline)
        {
            Timeline = timeline;
            Timeline.Entity = this;
            TimelineMode = TimelineModes.Replay;
        }

        public void StopReplay()
        {
            if(TimelineMode == TimelineModes.Replay)
            {
                Timeline = null;
            }
        }

        public void StopRecording()
        {
            if(TimelineMode == TimelineModes.Record)
            {
                Timeline = null;
            }
        }

        private void Tick_Timeline()
        {
            if (TimelineMode == TimelineModes.Record && _autoRecordTimeline)
            {
                Timeline?.RecordTick();
            }

            if (TimelineMode == TimelineModes.Replay && _autoReplayTimeline)
            {
                Timeline?.ReplayTick();
            }
        }

    }

    public abstract class GenericEntityTimeline<T> : EntityTimeline
    {

        [Key(0)]
        public List<T> Frames = new List<T>();

        [IgnoreMember]
        public T CurrentFrame => _frameIndex < Frames.Count && _frameIndex > 0 ? Frames[_frameIndex] : default;
        [IgnoreMember]
        public T LastFrame => Frames.Count > 0 ? Frames[Frames.Count - 1] : default;
        [IgnoreMember]
        protected int _frameIndex;

        public override void RecordTick() 
        {
            if (Paused)
            {
                return;
            }

            Frames.Add(GetFrame());
            _frameIndex = Frames.Count - 1;
        }

        public override void ReplayTick()
        {
            if(Frames.Count == 0 || Paused)
            {
                return;
            }

            if(_frameIndex >= Frames.Count)
            {
                _frameIndex = 0;
            }

            ApplyFrame(Frames[_frameIndex]);

            _frameIndex++;
        }

        protected abstract T GetFrame();
        protected abstract void ApplyFrame(T frame);

    }

    public abstract class EntityTimeline
    {

        [IgnoreMember]
        public bool Paused;
        [IgnoreMember]
        public NetEntity Entity;

        public abstract void RecordTick();
        public abstract void ReplayTick();

        public static T Deserialize<T>(byte[] data)
            where T : EntityTimeline
        {
            try
            {
                return MessagePackSerializer.Deserialize(typeof(T), data) as T;
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        public virtual byte[] Serialize()
        {
            return MessagePackSerializer.Serialize(GetType(), this);
        }

    }

}

