using Fragsurf.Shared.Entity;
using MessagePack;
using System;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [MessagePackObject]
    public struct BunnyhopTimelineFrame
    {
        [Key(0)]
        public int Tick;
        [Key(1)]
        public Vector3 Position;
        [Key(2)]
        public Vector3 Angles;
        [Key(3)]
        public int Velocity;
        [Key(4)]
        public float Time;
        [Key(5)]
        public int Jumps;
        [Key(6)]
        public int Strafes;
        [Key(7)]
        public int FinalSync;
        [Key(8)]
        public int Buttons;
        [IgnoreMember]
        public int GoodSync;
        [IgnoreMember]
        public int TotalSync;

        public BunnyhopTimelineFrame Subtract(BunnyhopTimelineFrame other)
        {
            var fr = this;
            fr.Tick = Tick - other.Tick;
            fr.Time = Time - other.Time;
            fr.Jumps = Jumps - other.Jumps;
            fr.Strafes = Strafes - other.Strafes;
            return fr;
        }

    }
}

