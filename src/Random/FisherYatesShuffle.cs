using System.Collections.Generic;
using System.Linq;

namespace DG.Cryptography.Random
{
    /// <summary>
    /// Provides an implementation of the Fisher–Yates shuffle, used to swap elements of lists in place.
    /// </summary>
    public sealed class FisherYatesShuffle
    {
        private static readonly FisherYatesShuffle _defaultInstance = new FisherYatesShuffle(SecureRandomNumberProvider.Default);

        /// <summary>
        /// Returns a static instance of <see cref="FisherYatesShuffle"/>.
        /// </summary>
        public static FisherYatesShuffle Default => _defaultInstance;

        private readonly IRandomNumberProvider _rnp;

        /// <summary>
        /// Initializes a new instance of <see cref="FisherYatesShuffle"/> with the given implementation of <see cref="IRandomNumberProvider"/>.
        /// </summary>
        /// <param name="rng"></param>
        public FisherYatesShuffle(IRandomNumberProvider rng)
        {
            _rnp = rng;
        }

        /// <summary>
        /// Shuffles the given list using the current implementation of <see cref="IRandomNumberProvider"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        public void Shuffle<T>(IList<T> input)
        {
            int n = input.Count;
            while (n > 1)
            {
                int k = _rnp.Next(n--);
                T temp = input[n];
                input[n] = input[k];
                input[k] = temp;
            }
        }

        /// <summary>
        /// Shuffles the given string using the current implementation of <see cref="IRandomNumberProvider"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Shuffle(string input)
        {
            var stringArray = input.ToArray();
            Shuffle(stringArray);
            return new string(stringArray);
        }
    }
}
