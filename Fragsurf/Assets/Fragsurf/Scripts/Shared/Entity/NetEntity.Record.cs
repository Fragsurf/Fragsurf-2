using MessagePack;
using System.Collections.Generic;

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
            TimelineMode = TimelineModes.Record;
        }

        public void Replay(EntityTimeline timeline)
        {
            Timeline = timeline;
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
                Timeline?.RecordTick(this);
            }

            if (TimelineMode == TimelineModes.Replay && _autoReplayTimeline)
            {
                Timeline?.ReplayTick(this);
            }
        }

    }

    public abstract class GenericEntityTimeline<T> : EntityTimeline
    {

        [Key(0)]
        public List<T> Frames = new List<T>();

        [IgnoreMember]
        public T CurrentFrame => _frameIndex <= Frames.Count && _frameIndex > 0 ? Frames[_frameIndex] : default;
        [IgnoreMember]
        public T LastFrame => Frames.Count > 0 ? Frames[Frames.Count - 1] : default;
        [IgnoreMember]
        protected int _frameIndex;

        public override void RecordTick(NetEntity ent) 
        {
            Frames.Add(GetFrame(ent));
            _frameIndex = Frames.Count - 1;
        }

        public override void ReplayTick(NetEntity ent)
        {
            if(Frames.Count == 0)
            {
                return;
            }

            if(_frameIndex >= Frames.Count)
            {
                _frameIndex = 0;
            }

            ApplyFrame(ent, Frames[_frameIndex]);

            _frameIndex++;
        }

        protected abstract T GetFrame(NetEntity ent);
        protected abstract void ApplyFrame(NetEntity ent, T frame);

    }

    public abstract class EntityTimeline
    {

        public abstract void RecordTick(NetEntity ent);
        public abstract void ReplayTick(NetEntity ent);

        public static T Deserialize<T>(byte[] data)
            where T : EntityTimeline
        {
            return MessagePackSerializer.Deserialize(typeof(T), data) as T;
        }

        public virtual byte[] Serialize()
        {
            return MessagePackSerializer.Serialize(GetType(), this);
        }

    }

}

