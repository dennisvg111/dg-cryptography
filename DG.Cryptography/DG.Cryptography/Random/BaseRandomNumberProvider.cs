using System;
using System.Collections.Generic;

namespace DG.Cryptography.Random
{
    /// <summary>
    /// This class provides a way to implement <see cref="IRandomNumberProvider"/> by only implementing a method to generate byte arrays.
    /// </summary>
    public abstract class BaseRandomNumberProvider : IRandomNumberProvider
    {
        private readonly object _lock = new object();

        /// <summary>
        /// When overridden in a derived class, generates an array of bytes with the given amount of randomly generated values
        /// </summary>
        /// <param name="count"></param>
        public abstract byte[] NextBytes(int count);

        /// <summary>
        /// Generates an unsigned integer.
        /// </summary>
        /// <returns>An unsigned integer between 0 and <see cref="uint.MaxValue"/></returns>
        public uint NextUint()
        {
            var bytes = NextBytes(sizeof(uint));
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Generates an integer between 0 inclusive and <paramref name="maxValueExclusive"/> exclusive.
        /// </summary>
        /// <param name="maxValueExclusive"></param>
        /// <returns></returns>
        public int Next(int maxValueExclusive)
        {
            uint maxValue = uint.MaxValue - (uint.MaxValue % (uint)maxValueExclusive);
            do
            {
                var num = NextUint();

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

        /// <summary>
        /// Returns a value at a random position in the given <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public T PickFrom<T>(IReadOnlyList<T> list)
        {
            return list[Next(list.Count)];
        }
    }
}
