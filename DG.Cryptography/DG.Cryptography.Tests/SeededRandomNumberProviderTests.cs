using DG.Cryptography.Random;
using System.Linq;
using Xunit;

namespace DG.Cryptography.Tests
{
    public class SeededRandomNumberProviderTests
    {
        [Fact]
        public void SameSeedGivesSameValues()
        {
            var seed1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var seed2 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var seed3 = new byte[] { 10, 2, 3, 5, 4, 6, 7, 8, 9, 1 };

            using (SeededRandomNumberProvider p1 = new SeededRandomNumberProvider(seed1))
            using (SeededRandomNumberProvider p2 = new SeededRandomNumberProvider(seed2))
            using (SeededRandomNumberProvider p3 = new SeededRandomNumberProvider(seed3))
            {
                var u1 = Enumerable.Range(0, 120).Select((i) => p1.NextUint()).ToArray();
                var u2 = Enumerable.Range(0, 120).Select((i) => p2.NextUint()).ToArray();
                var u3 = Enumerable.Range(0, 120).Select((i) => p3.NextUint()).ToArray();

                Assert.Equal(u2, u1);
                Assert.NotEqual(u3, u1);
            }
        }
    }
}
