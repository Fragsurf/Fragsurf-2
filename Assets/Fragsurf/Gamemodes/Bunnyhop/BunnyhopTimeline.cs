using Fragsurf.Actors;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [MessagePackObject]
    public class BunnyhopTimeline : GenericEntityTimeline<BunnyhopTimelineFrame>
    {

        [IgnoreMember]
        public FSMTrack Track;
        [IgnoreMember]
        public int Checkpoint = 1;
        [IgnoreMember]
        public int Stage = 1;
        [IgnoreMember]
        public bool RunIsLive = true;
        [IgnoreMember]
        public bool InStartZone = false;
        [IgnoreMember]
        public int BadTick;
        [IgnoreMember]
        private CircularBuffer<bool> _realtimeSyncBuffer = new CircularBuffer<bool>(_realtimeSyncSize);
        [IgnoreMember]
        private const int _realtimeSyncSize = 100;

        [IgnoreMember]
        private float _previousYaw;

        private Dictionary<int, int> _segments = new Dictionary<int, int>();

        public BunnyhopTimeline() { }

        public float GetReplayPosition()
        {
            return (float)_frameIndex / Frames.Count;
        }

        public void SetSegment(int cp)
        {
            _segments[cp] = Mathf.Max(LastFrame.Tick, 1);
        }

        public bool GetSegment(int cp, out BunnyhopTimelineFrame finalFrame, out BunnyhopTimeline newTimeline)
        {
            finalFrame = default;
            newTimeline = null;

            if (!_segments.ContainsKey(cp))
            {
                return false;
            }

            var f1 = Frames.FindIndex(x => x.Tick == _segments[cp]);
            var f2 = _segments.ContainsKey(cp + 1) ? Frames.FindIndex(x => x.Tick == _segments[cp + 1]) : Frames.Count - 1;

            if(f1 == -1 
                || f2 == -1
                || f1 >= Frames.Count
                || f2 >= Frames.Count)
            {
                return false;
            }

            var startFrame = Frames[f1];
            var endFrame = Frames[f2];
            finalFrame = endFrame.Subtract(startFrame);

            var frameList = new List<BunnyhopTimelineFrame>(finalFrame.Tick);
            for(int i = f1; i < f2; i++) 
            {
                frameList.Add(Frames[i]);
            }

            newTimeline = new BunnyhopTimeline();
            newTimeline.Frames = frameList;

            return true;
        }

        public void SetReplayPosition(float v)
        {
            _frameIndex = (int)Mathf.Lerp(0, Frames.Count, v);
            if(_frameIndex > 0 && _frameIndex < Frames.Count)
            {
                ApplyFrame(Frames[_frameIndex]);
            }
        }

        protected override BunnyhopTimelineFrame GetFrame()
        {
            var frame = LastFrame;
            var hu = Entity as Human;
            var vel = hu.Velocity;
            vel.y = 0;
            frame.Tick++;
            frame.Position = Entity.Origin;
            frame.Angles = Entity.Angles;
            frame.Time += Time.fixedDeltaTime;
            frame.Velocity = (int)(vel.magnitude / SurfController.HammerScale);

            CalculateSync(hu, ref frame);
            CheckButtons(hu, ref frame);

            return frame;
        }

        private void CheckButtons(Human human, ref BunnyhopTimelineFrame frame)
        {
            if (human.MovementController is CSMovementController move)
            {
                frame.Buttons = (int)move.MoveData.Buttons;

                if (move.MoveData.JustJumped)
                {
                    frame.Jumps++;
                }

                var nb = move.MoveData.Buttons;
                var ob = move.MoveData.OldButtons;

                if ((nb.HasFlag(InputActions.MoveLeft) && !ob.HasFlag(InputActions.MoveLeft))
                    || (nb.HasFlag(InputActions.MoveRight) && !ob.HasFlag(InputActions.MoveRight)))
                {
                    frame.Strafes++;
                }
            }
        }

        private void CalculateSync(Human human, ref BunnyhopTimelineFrame frame)
        {
            if (!(human.MovementController is CSMovementController move))
            {
                return;
            }

            var nb = move.MoveData.Buttons;

            if (move.GroundObject == null)
            {
                var angleDiff = move.MoveData.ViewAngles.y - _previousYaw;
                if (angleDiff > 180)
                    angleDiff -= 360;
                else if (angleDiff < -180)
                    angleDiff += 360;

                // Add to good sync if client buttons match up
                if (angleDiff < 0)
                {
                    var rtsync = false;
                    frame.TotalSync++;
                    if (nb.HasFlag(InputActions.MoveLeft) && !nb.HasFlag(InputActions.MoveRight))
                    {
                        frame.GoodSync++;
                        rtsync = true;
                    }
                    _realtimeSyncBuffer.PushFront(rtsync);
                }
                else if (angleDiff > 0)
                {
                    var rtsync = false;
                    frame.TotalSync++;
                    if (nb.HasFlag(InputActions.MoveRight) && !nb.HasFlag(InputActions.MoveLeft))
                    {
                        frame.GoodSync++;
                        rtsync = true;
                    }
                    _realtimeSyncBuffer.PushFront(rtsync);
                }
            }

            frame.FinalSync = frame.TotalSync != 0
                ? (int)(((float)frame.GoodSync / frame.TotalSync) * 100f)
                : (byte)100;

            _previousYaw = human.Angles.y;
        }

        public int GetRealtimeSync(int count = 100)
        {
            count = Mathf.Clamp(count, 0, _realtimeSyncBuffer.Size);
            var ticksInSync = 0;
            for(int i = 0; i < count; i++)
            {
                if (_realtimeSyncBuffer[i])
                {
                    ticksInSync++;
                }
            }
            return (int)((float)ticksInSync / count * 100f);
        }

        Vector3 originLastFrame;
        protected override void ApplyFrame(BunnyhopTimelineFrame frame)
        {
            originLastFrame = Entity.Origin;
            Entity.Origin = frame.Position;
            Entity.Angles = frame.Angles;
            if(Entity is Human hu)
            {
                hu.Velocity = (Entity.Origin - originLastFrame).normalized * frame.Velocity * .0254f;
            }
        }

    }
}

