using UnityEngine;

namespace Fragsurf.Shared
{
    public class TimelineReplay
    {

        public readonly Timeline Track;

        private GameObject _target;
        private int _frameIndex;

        public TimelineFrame Frame => Track.Frames[FrameIndex];
        public int FrameIndex
        {
            get => _frameIndex;
            set
            {
                _frameIndex = value;
                if (_frameIndex >= Track.Frames.Count)
                {
                    _frameIndex = 0;
                }
            }
        }

        public TimelineReplay(GameObject target, Timeline track)
        {
            Track = track;
            _target = target;
        }

        public void Tick()
        {
            _target.transform.position = Frame.Position;
            _target.transform.eulerAngles = Frame.Angles;
            FrameIndex++;
        }

    }
}
