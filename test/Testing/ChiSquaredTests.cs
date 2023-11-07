using DG.Cryptography.Testing;
using Xunit;

namespace DG.Cryptography.Tests.Testing
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
        public void Result_Correct()
        {
            var chiSquared = new ChiSquared(_example);

            var x2 = chiSquared.Result;

            Assert.Equal(24.57, x2, 0.005);
        }

        [Fact]
        public void DegreesOfFreedom_Correct()
        {
            var chiSquared = new ChiSquared(_example);

            var degreesOfFreedom = chiSquared.DegreesOfFreedom;

            Assert.Equal(6, degreesOfFreedom);
        }

        [Fact]
        public void CalculatePValue_Correct()
        {
            var chiSquared = new ChiSquared(_example);

            var pValue = chiSquared.CalculatePValue();

            Assert.Equal(0.0004, pValue, 0.00005);
        }

        private static readonly double[] _diceExample = new double[] { 5, 8, 9, 8, 10, 20 };
        [Fact]
        public void DiceFrequency_Correct()
        {
            var chiSquared = new ChiSquared(_diceExample);
            var chi = chiSquared.Result;
            var pValue = chiSquared.CalculatePValue();

            Assert.Equal(13.4D, chi);
            Assert.InRange(pValue, 0.01, 0.025);
        }
    }
}
