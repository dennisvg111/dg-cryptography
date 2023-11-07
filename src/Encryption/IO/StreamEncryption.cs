using System.IO;
using System.Security.Cryptography;

namespace DG.Cryptography.Encryption.IO
{
    /// <summary>
    /// Provides functionality for encrypting and decrypting streams using a <see cref="StreamEncryptionKey"/>.
    /// </summary>
    public static class StreamEncryption
    {
        /// <summary>
        /// Creates a new stream that contains an encrypted version of the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Stream Encrypt(Stream stream, StreamEncryptionKey key)
        {
            using (var aes = key.GetAesImplementation())
            {
                var transform = aes.CreateEncryptor(aes.Key, aes.IV);
                var cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);

                return cryptoStream;
            }
        }

        /// <summary>
        /// Creates a new stream that contains a decrypted version of the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Stream Decrypt(Stream stream, StreamEncryptionKey key)
        {
            using (var aes = key.GetAesImplementation())
            {
                var transform = aes.CreateDecryptor(aes.Key, aes.IV);
                var cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);

                return cryptoStream;
            }
        }
    }
}
