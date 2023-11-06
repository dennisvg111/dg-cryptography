using DG.Cryptography.Random;
using DG.Cryptography.Random.Testing;
using System.Linq;
using Xunit;

namespace DG.Cryptography.Tests.Random
{
    public class SecureRandomNumberProviderTests
    {
        [Fact]
        public void ChiSquared_Fails()
        {
            int iterations = 1000000;
            int bytesPerIteration = 50;
            double precision = 0.005;

            var outputs = new double[iterations][];
            byte[] buffer = new byte[bytesPerIteration];

            for (int i = 0; i < iterations; i++)
            {
                var random = new System.Random();
                random.NextBytes(buffer);
                outputs[i] = buffer.Select(b => (double)b).ToArray();
            }

            var chiSquared = new ChiSquared(outputs);
            var alpha = chiSquared.CalculateAlpha();
            Assert.InRange(alpha, 0, precision);
        }

        [Fact]
        public void ChiSquared_LessThan()
        {
            int iterations = 1000000;
            int bytesPerIteration = 50;
            double precision = 0.005;

            var outputs = new double[iterations][];

            for (int i = 0; i < iterations; i++)
            {
                using (var rnp = new SecureRandomNumberProvider())
                {
                    outputs[i] = rnp.NextBytes(bytesPerIteration).Select(b => (double)b).ToArray();
                }
            }

            var chiSquared = new ChiSquared(outputs);
            var alpha = chiSquared.CalculateAlpha();
            Assert.InRange(alpha, 0, precision);
        }

        [Fact]
        public void ChiSquared_Frequencies()
        {
            int bytes = 1200;

            double[] frequencies = new double[256];
            using (var rnp = new SecureRandomNumberProvider())
            {
                var generated = rnp.NextBytes(bytes);

                foreach (var b in generated)
                {
                    frequencies[b]++;
                }
            }

            var expectedFrequency = Enumerable.Repeat(4.6875, 256).ToArray();

            var chiSquared = new ChiSquared(frequencies, expectedFrequency);
            var alpha = chiSquared.CalculateAlpha();
            Assert.InRange(alpha, 0, 0.001);
        }
    }
}
