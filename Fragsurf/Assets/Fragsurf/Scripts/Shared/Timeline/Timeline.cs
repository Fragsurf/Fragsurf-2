using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
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

            var vel = Human.Velocity;
            vel.y = 0;

            CurrentFrame = new TimelineFrame()
            {
                Tick = CurrentFrame.Tick + 1,
                Angles = Human.Angles,
                Position = Human.Origin,
                Time = CurrentFrame.Time + Time.fixedDeltaTime,
                Strafes = 0,
                Jumps = 0,
                Velocity = (int)(vel.magnitude / SurfController.HammerScale)
            };
        }

    }
}
