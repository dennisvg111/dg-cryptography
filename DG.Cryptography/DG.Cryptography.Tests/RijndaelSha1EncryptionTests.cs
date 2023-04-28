using DG.Cryptography.Encryption;
using Xunit;

namespace DG.Cryptography.Tests
{
    public class RijndaelSha1EncryptionTests
    {
        [Theory]
        [InlineData("test", "key")]
        public void EncryptionReversible(string plainText, string key)
        {
            var encryption = RijndaelSha1Encryption.Default;

            var encrypted = encryption.Encrypt(plainText, key);
            var decrypted = encryption.Decrypt(encrypted, key);

            Assert.NotEqual(plainText, encrypted);
            Assert.Equal(plainText, decrypted);
        }
    }
}
