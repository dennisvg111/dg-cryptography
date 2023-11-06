using DG.Cryptography.Random;
using DG.Cryptography.Tests.TestingUtilities;
using Xunit;

namespace DG.Cryptography.Tests.Random
{
    public class IRandomNumberProviderExtensionTests
    {
        private const int _maxIterations = 10_000_000;
        private readonly IRandomNumberProvider _randomNumberProvider = SecureRandomNumberProvider.Default;

        [Fact]
        public void NextUInt_EvenDistribution()
        {
            var runningAverage = new RunningAverage();

            for (int i = 0; i < _maxIterations; i++)
            {
                var integer = IRandomNumberProviderExtensions.NextUint(_randomNumberProvider);
                runningAverage.AddSample(integer);
            }

            var expected = uint.MaxValue / 2D;

            //expected: 2,147,483,647.5
            var tolerance = 1_000_000;

            Assert.InRange(runningAverage.Average, expected - tolerance, expected + tolerance);
        }

        [Fact]
        public void NextInt_EvenDistribution()
        {
            var runningAverage = new RunningAverage();

            for (int i = 0; i < _maxIterations; i++)
            {
                var integer = IRandomNumberProviderExtensions.Next(_randomNumberProvider, 100);
                runningAverage.AddSample(integer);
            }

            Assert.Equal(0, runningAverage.Min);
            Assert.Equal(99, runningAverage.Max);
            Assert.Equal(49.5, runningAverage.Average, 0.1);
        }

        [Fact]
        public void NextDouble_EvenDistribution()
        {
            var runningAverage = new RunningAverage();

            for (int i = 0; i < _maxIterations; i++)
            {
                var d = IRandomNumberProviderExtensions.NextDouble(_randomNumberProvider);
                runningAverage.AddSample(d);
            }

            //doubles should never be outside of 0-1 range.
            Assert.True(runningAverage.Min >= 0);
            Assert.True(runningAverage.Max < 1);

            //even distribution
            Assert.Equal(0, runningAverage.Min, 0.0001);
            Assert.Equal(1, runningAverage.Max, 0.0001);
            Assert.Equal(0.5, runningAverage.Average, 0.001);
        }
    }
}
