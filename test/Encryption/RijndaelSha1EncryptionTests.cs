using DG.Cryptography.Encryption;
using System;
using Xunit;

namespace DG.Cryptography.Tests.Encryption
{
    public class RijndaelSha1EncryptionTests
    {
        [Theory]
        [InlineData("test-text", "key")]
        public void EncryptionReversible(string plainText, string key)
        {
            var encryption = RijndaelSha1Encryption.Default;

            var encrypted = encryption.Encrypt(plainText, key);
            var decrypted = encryption.Decrypt(encrypted, key);

            Assert.NotEqual(plainText, encrypted);
            Assert.Equal(plainText, decrypted);
        }

        [Theory]
        [InlineData("plaintext-for-empty-key", "")]
        [InlineData("", "key-for-empty")]
        [InlineData("plaintext-for-empty-key", null)]
        [InlineData(null, "key-for-empty")]
        public void EncryptShouldThrow(string plainText, string key)
        {
            var encryption = RijndaelSha1Encryption.Default;

            Assert.ThrowsAny<Exception>(() => encryption.Encrypt(plainText, key));
        }

        [Theory]
        [InlineData("ciphertext-for-empty-key", "")]
        [InlineData("", "key-for-empty")]
        [InlineData("ciphertext-for-empty-key", null)]
        [InlineData(null, "key-for-empty")]
        public void DecryptShouldThrow(string plainText, string key)
        {
            var encryption = RijndaelSha1Encryption.Default;

            Assert.ThrowsAny<Exception>(() => encryption.Decrypt(plainText, key));
        }
    }
}
