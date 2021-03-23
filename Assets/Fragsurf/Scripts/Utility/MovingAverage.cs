using System.Collections.Generic;

namespace Fragsurf.Utility
{
    public class MovingAverage
    {

        private CircularBuffer<float> _samples;
        public float Average { get; private set; }
        public float TrimmedAverage { get; private set; }

        private List<float> _sortedSamples;

        public MovingAverage(int sampleSize)
        {
            _samples = new CircularBuffer<float>(sampleSize);
            _sortedSamples = new List<float>(sampleSize);
        }

        public void ComputeAverage(float newSample, bool computeTrimmedAverage = true)
        {
            _samples.PushBack(newSample);
            var accumulator = 0f;
            for (int i = 0; i < _samples.Size; i++)
            {
                accumulator += _samples[i];
            }
            Average = accumulator / _samples.Size;

            if (computeTrimmedAverage)
            {
                TrimmedAverage = ComputeTrimmedAverage(4);
            }
        }

        private float ComputeTrimmedAverage(int padding)
        {
            if (_sortedSamples.Count <= padding * 2)
            {
                return Average;
            }

            _sortedSamples.Clear();

            for (int i = 0; i < _samples.Size; i++)
            {
                _sortedSamples.Add(_samples[i]);
            }

            _sortedSamples.Sort();

            var accumulator = 0f;
            var length = 0;

            for (int i = padding; i < _sortedSamples.Count - padding; i++)
            {
                accumulator += _sortedSamples[i];
                length++;
            }

            return accumulator / length;
        }

        public void Clear()
        {
            _samples.Clear();
            _sortedSamples.Clear();
            Average = 0;
            TrimmedAverage = 0;
        }
    }
}


