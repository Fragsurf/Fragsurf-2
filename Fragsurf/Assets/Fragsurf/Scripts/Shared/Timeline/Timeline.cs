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

        public void Reset()
        {
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

            var jumps = CurrentFrame.Jumps;
            var strafes = CurrentFrame.Strafes;

            if(Human.MovementController is DefaultMovementController move)
            {
                if (move.MoveData.JustJumped)
                {
                    jumps++;
                }

                var nb = move.MoveData.Buttons;
                var ob = move.MoveData.OldButtons;

                if ((nb.HasFlag(InputActions.MoveLeft) && !ob.HasFlag(InputActions.MoveLeft))
                    || (nb.HasFlag(InputActions.MoveRight) && !ob.HasFlag(InputActions.MoveRight)))
                {
                    strafes++;
                }
            }

            var vel = Human.Velocity;
            vel.y = 0;

            CurrentFrame = new TimelineFrame()
            {
                Tick = CurrentFrame.Tick + 1,
                Angles = Human.Angles,
                Position = Human.Origin,
                Time = CurrentFrame.Time + Time.fixedDeltaTime,
                Strafes = strafes,
                Jumps = jumps,
                Velocity = (int)(vel.magnitude / SurfController.HammerScale)
            };
        }

    }
}
