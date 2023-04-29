using DG.Common.Exceptions;
using System;
using System.Security.Cryptography;

namespace DG.Cryptography.Random
{
    /// <summary>
    /// An implementation of <see cref="IRandomNumberProvider"/> based on a cryptographic secure random number generator, that returns the same values for the same seed.
    /// </summary>
    public sealed class SeededRandomNumberProvider : BaseRandomNumberProvider, IDisposable
    {
        private const int _iterations = 1000;

        private readonly Rfc2898DeriveBytes _deriveBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeededRandomNumberProvider"/> class using the given seed. Note that the seed needs to be at least 8 bytes long. 
        /// </summary>
        /// <param name="seed"></param>
        public SeededRandomNumberProvider(byte[] seed)
        {
            ThrowIf.Parameter.IsNullOrEmpty(seed, nameof(seed), "Seed cannot be empty.");
            ThrowIf.Collection(seed, nameof(seed)).CountLessThan(8, "Seed needs to be at least 8 bytes long.");

            _deriveBytes = new Rfc2898DeriveBytes(string.Empty, seed, _iterations);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _deriveBytes.Dispose();
        }

        /// <inheritdoc/>
        public override byte[] NextBytes(int count)
        {
            return _deriveBytes.GetBytes(count);
        }
    }
}
