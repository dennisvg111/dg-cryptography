using System;
using System.Collections.Generic;

namespace DG.Cryptography.Random
{
    /// <summary>
    /// Provides extension methods for implementations of <see cref="IRandomNumberProvider"/>.
    /// </summary>
    public static class IRandomNumberProviderExtensions
    {
        private static readonly object _lock = new object();

        /// <summary>
        /// Generates an array of bytes with the given amount of randomly generated values
        /// </summary>
        /// <param name="randomNumberProvider"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] NextBytes(this IRandomNumberProvider randomNumberProvider, int count)
        {
            var buffer = new byte[count];
            randomNumberProvider.GetNext(buffer);
            return buffer;
        }

        /// <summary>
        /// Generates a 32-bit <see cref="uint"/>.
        /// </summary>
        /// <param name="randomNumberProvider"></param>
        /// <returns>An unsigned integer between 0 and <see cref="uint.MaxValue"/></returns>
        public static uint NextUint(this IRandomNumberProvider randomNumberProvider)
        {
            var bytes = randomNumberProvider.NextBytes(sizeof(uint));
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Generates a random <see cref="double"/> between 0.0 and 1.0.
        /// </summary>
        /// <param name="randomNumberProvider"></param>
        /// <returns></returns>
        public static double NextDouble(this IRandomNumberProvider randomNumberProvider)
        {
            var bytes = randomNumberProvider.NextBytes(sizeof(ulong));

            //bit-shift 11 and 53 based on double's mantissa bits
            var ul = BitConverter.ToUInt64(bytes, 0) >> 11;
            return ul / (double)(1UL << 53);
        }

        /// <summary>
        /// Generates an integer between 0 inclusive and <paramref name="maxValueExclusive"/> exclusive.
        /// </summary>
        /// <param name="randomNumberProvider"></param>
        /// <param name="maxValueExclusive"></param>
        /// <returns>A positive integer that is less than <paramref name="maxValueExclusive"/></returns>
        public static int Next(this IRandomNumberProvider randomNumberProvider, int maxValueExclusive)
        {
            lock (_lock)
            {
                uint maxValue = uint.MaxValue - (uint.MaxValue % (uint)maxValueExclusive);
                do
                {
                    var num = randomNumberProvider.NextUint();

                    //this check is needed so lower numbers don't have a larger chance to be chosen.
                    //for example, if uint.MaxValue would be 40 and maxValueExlusive would be 19 the modulo operation would cause 38, 39, and 40 to also be mapped to 0 through 2.
                    //this would mean the first 3 numbers have a 3/41 chance to be generated (41 because 0 is also a valid option), while the rest would have a 2/41 chance, so 1.5 times the chance.
                    //in this example maxValue would be mapped to 40 - (40 % 19) = 40 - 2 = 38, so values 38 and above are ignored.
                    if (num < maxValue)
                    {
                        return (int)(num % (uint)maxValueExclusive);
                    }
                } while (true);
            }
        }

        /// <summary>
        /// Returns a value at a random position in the given <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="randomNumberProvider"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T NextPickFrom<T>(this IRandomNumberProvider randomNumberProvider, IReadOnlyList<T> list)
        {
            return list[randomNumberProvider.Next(list.Count)];
        }
    }
}
