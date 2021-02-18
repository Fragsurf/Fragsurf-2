using Fragsurf.Shared;
using Fragsurf.Utility;

namespace Fragsurf.Client
{
    public class EntityInterpState
    {

        private CircularBuffer<EntityFrame> _frames = new CircularBuffer<EntityFrame>(20);

        public CircularBuffer<EntityFrame> Frames => _frames;
        public EntityFrame this[int i] => _frames[i];
        public int FrameCount => _frames.Size;

        public void AddFrame(EntityFrame frame)
        {
            _frames.PushFront(frame);
        }

    }
}