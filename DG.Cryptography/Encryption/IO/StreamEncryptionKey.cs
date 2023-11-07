using DG.Common;
using DG.Cryptography.Random;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace DG.Cryptography.Encryption.IO
{
    /// <summary>
    /// This class represents an encryption key that can be used with <see cref="StreamEncryption"/> to encrypt a stream.
    /// </summary>
    public class StreamEncryptionKey
    {
        private const int _saltBytes = 32;
        private const int _iterations = 1042; // Recommendation is >= 1000.

        private static readonly SecureRandomNumberProvider _randomBytesProvider = SecureRandomNumberProvider.Default;
        private static readonly int _defaultKeyBits;
        private static readonly int _defaultBlockBits;

        static StreamEncryptionKey()
        {
            AesManaged aes = new AesManaged();
            _defaultKeyBits = aes.LegalKeySizes[0].MaxSize;
            _defaultBlockBits = aes.LegalBlockSizes[0].MaxSize;
        }

        private readonly byte[] _salt;
        private readonly byte[] _iv;
        private readonly byte[] _key;



        /// <summary>
        /// Initializes a new instance of <see cref="StreamEncryptionKey"/>, with the given salt, IV, and key.
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="iv"></param>
        /// <param name="key"></param>
        public StreamEncryptionKey(byte[] salt, byte[] iv, byte[] key)
        {
            _salt = salt;
            _iv = iv;
            _key = key;
        }

        /// <summary>
        /// Returns the public key part of this encryption key. This can be used to recreate the encryption key using <see cref="FromPublicKey(string, string)"/>.
        /// </summary>
        /// <returns></returns>
        public string PublicKey
        {
            get
            {
                var saltString = SafeBase64.Encode(_salt);
                var ivString = SafeBase64.Encode(_iv);

                return _key.Length.ToString().PadLeft(4, '0')
                    + saltString.Length.ToString().PadLeft(4, '0')
                    + saltString
                    + ivString;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="StreamEncryptionKey"/> from the given public key part and password.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static StreamEncryptionKey FromPublicKey(string publicKey, string password)
        {
            var formatException = new FormatException($"\"{publicKey}\" is not a valid public key for {nameof(StreamEncryptionKey)}.");

            if (publicKey.Length < 8)
            {
                throw formatException;
            }
            var keyBytesPart = publicKey.Substring(0, 4).TrimStart('0');
            var saltStringLengthPart = publicKey.Substring(4, 4).TrimStart('0');

            if (!int.TryParse(keyBytesPart, out int keyBytesCount) || !int.TryParse(saltStringLengthPart, out int saltStringLength))
            {
                throw formatException;
            }

            var saltString = publicKey.Substring(8, saltStringLength);
            var ivString = publicKey.Substring(8 + saltStringLength);

            var salt = SafeBase64.Decode(saltString);
            var iv = SafeBase64.Decode(ivString);

            return FromSaltIvAndPassword(salt, iv, password, keyBytesCount);
        }

        /// <summary>
        /// Generates a new instance of <see cref="StreamEncryptionKey"/>, with a random salt and IV.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static StreamEncryptionKey GenerateForPassword(string password)
        {
            byte[] salt = _randomBytesProvider.NextBytes(_saltBytes);
            byte[] iv = _randomBytesProvider.NextBytes(_defaultBlockBits / 8);

            return FromSaltIvAndPassword(salt, iv, password, _defaultKeyBits / 8);
        }

        private static StreamEncryptionKey FromSaltIvAndPassword(byte[] salt, byte[] iv, string password, int keyByteCount)
        {
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, _iterations);
            var keyBytes = key.GetBytes(keyByteCount);

            return new StreamEncryptionKey(salt, iv, keyBytes);
        }

        /// <summary>
        /// Returns an implementation of the Advanced Encryption Standard algorithm.
        /// </summary>
        /// <returns></returns>
        public AesManaged GetAesImplementation()
        {
            return new AesManaged
            {
                BlockSize = _iv.Length * 8,
                KeySize = _key.Length * 8,
                IV = _iv,
                Key = _key,
                Mode = CipherMode.CBC
            };
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as StreamEncryptionKey;
            if (other == null)
            {
                return false;
            }

            return Enumerable.SequenceEqual(_salt, other._salt)
                && Enumerable.SequenceEqual(_iv, other._iv)
                && Enumerable.SequenceEqual(_key, other._key);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.OfEach(_salt)
                .AndEach(_iv)
                .AndEach(_key);
        }
    }
}
