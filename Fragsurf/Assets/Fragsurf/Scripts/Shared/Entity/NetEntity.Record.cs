using System.Collections.Generic;

namespace Fragsurf.Shared.Entity
{
    public partial class NetEntity
    {

        public EntityTimeline Timeline { get; private set; }
        public bool IsRecording { get; private set; }

        public void Record<T>(T timeline)
            where T : EntityTimeline
        {
            Timeline = timeline;
        }

        public void StopRecording()
        {
            IsRecording = false;
        }

    }

    public abstract class GenericEntityTimeline<T> : EntityTimeline
    {

        public List<T> Frames { get; private set; } = new List<T>();

        public override void Tick(NetEntity ent) 
        {
            Frames.Add(GetFrame(ent));
        }

        public override void Deserialize(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public override byte[] Serialize()
        {
            throw new System.NotImplementedException();
        }

        protected abstract T GetFrame(NetEntity ent);

    }

    public abstract class EntityTimeline
    {
        public abstract void Tick(NetEntity ent);
        public abstract void Deserialize(byte[] data);
        public abstract byte[] Serialize();
    }

}

