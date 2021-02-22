using System;
using UnityEngine;

namespace Fragsurf.Shared
{
    public struct TimelineFrame
    {
        public int Tick;
        public Vector3 Position;
        public Vector3 Angles;
        public int Velocity;
        public float Time;
        public int Jumps;
        public int Strafes;
        public int GoodSync;
        public int GoodSyncVel;
        public int TotalSync;
        public int FinalSync;

        public string FormattedTime() => TimeSpan.FromSeconds(Time).ToString(@"mm\:ss\:fff");
    }
}
