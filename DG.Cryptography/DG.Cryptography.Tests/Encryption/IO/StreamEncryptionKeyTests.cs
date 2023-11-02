using DG.Cryptography.Encryption.IO;
using Xunit;

namespace DG.Cryptography.Tests.Encryption.IO
{
    public class StreamEncryptionKeyTests
    {
        [Fact]
        public void Generate_CreatesUnique()
        {
            string password = "HelloWorld1!";

            var oldKey = StreamEncryptionKey.GenerateForPassword(password);

            for (int i = 0; i < 100; i++)
            {
                var newKey = StreamEncryptionKey.GenerateForPassword(password);

                Assert.NotEqual(oldKey, newKey);
            }
        }

        [Theory]
        [InlineData("short")]
        [InlineData("")]
        [InlineData("very long password with special characters and spaces and multiple words in one single sentence to test if long keys will not generate differently !@.")]
        public void PublicKey_CanBeReused(string password)
        {
            var key1 = StreamEncryptionKey.GenerateForPassword(password);

            var publicKey = key1.PublicKey;

            var key2 = StreamEncryptionKey.FromPublicKey(publicKey, password);

            Assert.Equal(key1, key2);
        }
    }
}
