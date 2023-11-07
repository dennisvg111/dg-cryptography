using System;
using System.Security.Cryptography;

namespace DG.Cryptography.Random
{
    /// <summary>
    /// An implementation of <see cref="IRandomNumberProvider"/> using <see cref="RNGCryptoServiceProvider"/> to provide sequences of random bytes.
    /// </summary>
    public sealed class SecureRandomNumberProvider : IRandomNumberProvider, IDisposable
    {
        private readonly static SecureRandomNumberProvider _defaultInstance = new SecureRandomNumberProvider(true);

        /// <summary>
        /// Returns a static instance of <see cref="SecureRandomNumberProvider"/>.
        /// </summary>
        public static SecureRandomNumberProvider Default => _defaultInstance;

        private readonly bool _isDefault;
        private RNGCryptoServiceProvider _rng;

        private SecureRandomNumberProvider(bool isDefault)
        {
            _rng = new RNGCryptoServiceProvider();
            _isDefault = isDefault;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureRandomNumberProvider"/>.
        /// </summary>
        public SecureRandomNumberProvider() : this(false) { }

        /// <inheritdoc/>
        public void GetNext(byte[] buffer)
        {
            _rng.GetBytes(buffer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_isDefault || _rng == null)
            {
                return;
            }
            _rng.Dispose();
            _rng = null;
        }
    }
}
