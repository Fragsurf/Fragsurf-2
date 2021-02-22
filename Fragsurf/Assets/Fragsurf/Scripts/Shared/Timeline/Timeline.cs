using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class Timeline
    {

        public Timeline(Human hu)
        {
            Recording = true;
            Human = hu;
            StartTick = hu.Game.CurrentTick;
        }

        public readonly Human Human;
        public List<TimelineFrame> Frames = new List<TimelineFrame>();
        public List<int> Checkpoints = new List<int>();

        public bool Recording { get; set; }
        public int StartTick { get; private set; }
        public TimelineFrame CurrentFrame { get; private set; }

        private float _previousYaw;

        public void Reset()
        {
            _previousYaw = 0f;
            Checkpoints.Clear();
            Frames.Clear();
            CurrentFrame = default;
            StartTick = Human.Game.CurrentTick;
            Recording = true;
        }

        public TimelineFrame Checkpoint()
        {
            var idx = Mathf.Max(Frames.Count - 1, 0);
            Checkpoints.Add(idx);
            return Frames[idx];
        }

        public void RunCommand()
        {
            if (!Recording)
            {
                return;
            }

            Frames.Add(CurrentFrame);

            var vel = Human.Velocity;
            vel.y = 0;
            var newFrame = CurrentFrame;
            newFrame.Tick++;
            newFrame.Position = Human.Origin;
            newFrame.Angles = Human.Angles;
            newFrame.Time = CurrentFrame.Time + Time.fixedDeltaTime;
            newFrame.Velocity = (int)(vel.magnitude / SurfController.HammerScale);

            CalculateSync(ref newFrame);
            CheckJumpsAndStrafes(ref newFrame);

            CurrentFrame = newFrame;
        }

        private void CheckJumpsAndStrafes(ref TimelineFrame frame)
        {
            if (Human.MovementController is DefaultMovementController move)
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

        private void CalculateSync(ref TimelineFrame frame)
        {
            if(!(Human.MovementController is DefaultMovementController move))
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
                : 100;

            _previousYaw = Human.Angles.y;
        }

    }
}
