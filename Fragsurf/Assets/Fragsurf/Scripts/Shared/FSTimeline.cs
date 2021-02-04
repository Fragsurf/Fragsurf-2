using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fragsurf.Shared 
{
    [Inject(InjectRealm.Shared)]
    public class FSTimeline : FSSharedScript
    {

        private List<FSTimelineTrack> _tracks = new List<FSTimelineTrack>();

        protected override void OnPlayerIntroduced(IPlayer player)
        {
        }

        protected override void OnPlayerDisconnected(IPlayer player)
        {
            //
        }

        protected override void OnPlayerRunCommand(IPlayer player)
        {
            foreach(var track in _tracks)
            {
                if(track.Human == player.Entity)
                {
                    track.RunCommand();
                }
            }
        }

        public FSTimelineTrack CreateTrack(Human hu)
        {
            var result = new FSTimelineTrack(this, hu);
            _tracks.Add(result);
            return result;
        }

        public void RemoveTrack(FSTimelineTrack track)
        {
            _tracks.Remove(track);
        }

    }

    public class FSTimelineTrack
    {

        public FSTimelineTrack(FSTimeline tl, Human hu)
        {
            Live = true;
            Timeline = tl;
            Human = hu;
            StartTick = hu.Game.CurrentTick;
        }

        public bool Live;
        public readonly Human Human;
        public readonly FSTimeline Timeline;
        public int StartTick { get; private set; }
        public List<FSTimelineFrame> Frames = new List<FSTimelineFrame>();
        public List<int> Checkpoints = new List<int>();

        public FSTimelineFrame CurrentFrame { get; private set; }

        public void Stop()
        {
            Live = false;
        }

        public void Reset()
        {
            Checkpoints.Clear();
            Frames.Clear();
            CurrentFrame = default;
            StartTick = Human.Game.CurrentTick;
            Live = true;
        }

        public FSTimelineFrame Checkpoint()
        {
            var idx = Mathf.Max(Frames.Count - 1, 0);
            Checkpoints.Add(idx);
            return Frames[idx];
        }

        public void RunCommand()
        {
            if (!Live)
            {
                return;
            }

            Frames.Add(CurrentFrame);

            var vel = Human.Velocity;
            vel.y = 0;

            CurrentFrame = new FSTimelineFrame()
            {
                Tick = CurrentFrame.Tick + 1,
                Angles = Human.Angles,
                Position = Human.Origin,
                Time = CurrentFrame.Time + Time.fixedDeltaTime,
                Strafes = 0,
                Jumps = 0,
                Velocity = (int)(vel.magnitude * .0254f)
            };
        }

    }

    public struct FSTimelineFrame
    {
        public int Tick;
        public Vector3 Position;
        public Vector3 Angles;
        public int Velocity;
        public float Time;
        public int Jumps;
        public int Strafes;
    }

}


