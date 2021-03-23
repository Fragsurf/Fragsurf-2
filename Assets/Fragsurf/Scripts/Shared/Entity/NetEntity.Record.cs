using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public partial class NetEntity
    {

        protected bool _autoRecordTimeline = true;
        protected bool _autoReplayTimeline = true;

        public EntityTimeline Timeline;

        public void Record(EntityTimeline timeline)
        {
            Timeline = timeline;
            timeline.Mode = TimelineMode.Record;
            timeline.Entity = this;
        }

        public void Replay(EntityTimeline timeline)
        {
            Timeline = timeline;
            timeline.Mode = TimelineMode.Replay;
            timeline.Entity = this;
        }

        private void Tick_Timeline()
        {
            if(Timeline == null)
            {
                return;
            }

            if(_autoRecordTimeline && Timeline.Mode == TimelineMode.Record)
            {
                Timeline.RecordTick();
            }
            else if(_autoReplayTimeline && Timeline.Mode == TimelineMode.Replay)
            {
                Timeline.ReplayTick();
            }
        }

    }

    public enum TimelineMode
    {
        None,
        Record,
        Replay
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
        public TimelineMode Mode;
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

        public virtual async Task<byte[]> SerializeAsync()
        {
            using var ms = new MemoryStream();
            await MessagePackSerializer.SerializeAsync(GetType(), ms, this);
            return ms.ToArray();
        }

    }

}

