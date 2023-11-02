using System;
using System.Security.Cryptography;

namespace DG.Cryptography.Random
{
    /// <summary>
    /// An implementation of <see cref="IRandomNumberProvider"/> using <see cref="RNGCryptoServiceProvider"/> to provide sequences of random bytes.
    /// </summary>
    public sealed class SecureRandomNumberProvider : BaseRandomNumberProvider, IDisposable
    {
        private readonly static SecureRandomNumberProvider _defaultInstance = new SecureRandomNumberProvider();

        /// <summary>
        /// Returns a static instance of <see cref="SecureRandomNumberProvider"/>.
        /// </summary>
        public static SecureRandomNumberProvider Default => _defaultInstance;

        private RNGCryptoServiceProvider _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureRandomNumberProvider"/>.
        /// </summary>
        public SecureRandomNumberProvider()
        {
            _rng = new RNGCryptoServiceProvider();
        }

        /// <inheritdoc/>
        public override byte[] NextBytes(int count)
        {
            byte[] buffer = new byte[count];
            _rng.GetBytes(buffer);
            return buffer;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_rng != null)
            {
                _rng.Dispose();
            }
            _rng = null;
        }
    }
}
