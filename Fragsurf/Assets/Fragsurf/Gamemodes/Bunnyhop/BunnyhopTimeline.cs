using Fragsurf.Actors;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
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
            CheckJumpsAndStrafes(hu, ref frame);

            return frame;
        }

        private void CheckJumpsAndStrafes(Human human, ref BunnyhopTimelineFrame frame)
        {
            if (human.MovementController is DefaultMovementController move)
            {
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
            if (!(human.MovementController is DefaultMovementController move))
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
                    frame.TotalSync++;
                    if (nb.HasFlag(InputActions.MoveLeft) && !nb.HasFlag(InputActions.MoveRight))
                    {
                        frame.GoodSync++;
                    }
                    if (move.MoveData.Velocity.z < 0)
                    {
                        frame.GoodSyncVel++;
                    }
                }
                else if (angleDiff > 0)
                {
                    frame.TotalSync++;
                    if (nb.HasFlag(InputActions.MoveRight) && !nb.HasFlag(InputActions.MoveLeft))
                    {
                        frame.GoodSync++;
                    }
                    if (move.MoveData.Velocity.z > 0)
                    {
                        frame.GoodSyncVel++;
                    }
                }
            }

            frame.FinalSync = frame.TotalSync != 0
                ? (int)(((float)frame.GoodSync / frame.TotalSync) * 100f)
                : (byte)100;

            _previousYaw = human.Angles.y;
        }

        protected override void ApplyFrame(BunnyhopTimelineFrame frame)
        {
            Entity.Origin = frame.Position;
            Entity.Angles = frame.Angles;
        }

    }
}

