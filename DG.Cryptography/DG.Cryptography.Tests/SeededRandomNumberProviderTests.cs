using DG.Cryptography.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DG.Cryptography.Tests
{
    public class SeededRandomNumberProviderTests
    {
        public static IEnumerable<object[]> PreCalculatedSeeds => new object[][]
        {
            new object[]
            {
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new uint[] { 3416999137, 768482030, 3720233933, 3886160853, 1340862447, 506769984, 1953408128, 3933604003, 341311979, 2426506646 }
            },
            new object[]
            {
                new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
                new uint[] { 959993113, 252991332, 3266572781, 3784527337, 4098539890, 3664332934, 3530882703, 3965004308, 3792243078, 3333848394 }
            },
            new object[]
            {
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 },
                new uint[] { 1827710822, 1319319851, 2322412866, 3507821229, 2628725565, 932810844, 2087619693, 4073667679, 968536127, 1108431516 }
            },
            new object[]
            {
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[] { 4142777262, 438206814, 3520989036, 2348188222, 1167453218, 1174341576, 767010085, 2646430130, 951773479, 2953172789 }
            },
        };

        [Theory]
        [MemberData(nameof(PreCalculatedSeeds))]
        public void SameSeedGivesSameValues(byte[] seed, uint[] expectedValues)
        {

            using (SeededRandomNumberProvider srnp = new SeededRandomNumberProvider(seed))
            {
                var values = Enumerable.Range(0, 10).Select((i) => srnp.NextUint()).ToArray();

                var s1 = string.Join(", ", values.Select(i => i.ToString()));

                Assert.Equal(expectedValues, values);
            }
        }

        [Fact]
        public void Constructor_SeedNullThrowsArgumentNull()
        {
            byte[] seed = null;

            Action constructor = () => new SeededRandomNumberProvider(seed);

            Assert.Throws<ArgumentNullException>(constructor);
        }

        [Theory]
        [MemberData(nameof(GenerateSmallSeeds))]
        public void SeedLessThanEightThrowsException(byte[] seed)
        {
            Action constructor = () => new SeededRandomNumberProvider(seed);

            Assert.ThrowsAny<ArgumentException>(constructor);
        }

        public static IEnumerable<object[]> GenerateSmallSeeds()
        {
            List<object[]> seeds = new List<object[]>();
            for (int i = 0; i < 8; i++)
            {
                byte[] seed = new byte[i];
                seeds.Add(new object[] { seed });
            }
            return seeds;
        }
    }
}
