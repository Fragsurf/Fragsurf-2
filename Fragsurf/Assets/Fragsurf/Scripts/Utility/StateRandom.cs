using System;

namespace Fragsurf.Utility
{
    public class StateRandom : Random
    {
        public int NumberOfInvokes { get; private set; }

        public StateRandom(int Seed, int forward = 0) : base(Seed)
        {
            for (int i = 0; i < forward; ++i)
                Next(0);
        }

        public float Range(float minimum, float maximum)
        {
            NumberOfInvokes++;
            return (float)(base.NextDouble() * (maximum - minimum) + minimum);
        }

        public override double NextDouble()
        {
            NumberOfInvokes++;
            return base.NextDouble();
        }

        public override void NextBytes(byte[] buffer)
        {
            NumberOfInvokes++;
            base.NextBytes(buffer);
        }

        public override int Next()
        {
            NumberOfInvokes++;
            return base.Next();
        }

        public override int Next(int minValue, int maxValue)
        {
            NumberOfInvokes++;
            return base.Next(minValue, maxValue);
        }

        public override int Next(int maxValue)
        {
            NumberOfInvokes++;
            return base.Next(maxValue);
        }
    }
}

