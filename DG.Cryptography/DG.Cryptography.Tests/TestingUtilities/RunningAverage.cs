using System;

namespace DG.Cryptography.Tests.TestingUtilities
{
    public class RunningAverage
    {
        private long _count = 0;
        private double _average = 0;
        private double _min = double.MaxValue;
        private double _max = double.MinValue;

        public double Average => _average;
        public double Min => _min;
        public double Max => _max;

        public void AddSample(double value)
        {
            _min = Math.Min(value, _min);
            _max = Math.Max(value, _max);
            var newAverage = _count / (double)(_count + 1) * _average + value / (_count + 1);
            _average = newAverage;
            _count++;
        }
    }
}
