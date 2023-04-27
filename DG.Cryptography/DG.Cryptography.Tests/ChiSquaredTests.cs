using DG.Cryptography.Random.Testing;
using Xunit;

namespace DG.Cryptography.Tests
{
    public class ChiSquaredTests
    {
        private static readonly double[,] _example = new double[,]
        {
            {90, 60, 104, 95},
            {30, 50, 51, 20},
            {30, 40, 45, 35}
        };

        [Fact]
        public void CalculateExpectedValue()
        {
            var chiSquared = new ChiSquared(_example);

            var expected = chiSquared.CalculateExpectedValue(0, 0);

            Assert.Equal(80.54, expected, 0.005);
        }

        [Fact]
        public void CalculateX2()
        {
            var chiSquared = new ChiSquared(_example);

            var x2 = chiSquared.CalculateX2();

            Assert.Equal(24.57, x2, 0.005);
        }

        [Fact]
        public void CalculateDegreesOfFreedom()
        {
            var chiSquared = new ChiSquared(_example);

            var degreesOfFreedom = chiSquared.CalculateDegreesOfFreedom();

            Assert.Equal(6, degreesOfFreedom);
        }

        [Fact]
        public void CalculateAlpha()
        {
            var chiSquared = new ChiSquared(_example);

            var alpha = chiSquared.CalculateAlpha();

            Assert.Equal(0.0004, alpha, 0.00005);
        }
    }
}
