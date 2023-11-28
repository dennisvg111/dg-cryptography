using DG.Cryptography.Encryption.IO;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace DG.Cryptography.Tests.Encryption.IO
{
    public class EncryptedStreamWrapperTests
    {
        [Theory]
        [InlineData(0, 16)]
        [InlineData(2, 16)]
        [InlineData(16, 32)]
        [InlineData(41, 48)]
        public void EncryptedStreamWrapper_Length_Correct(int streamLength, int expectedLength)
        {
            var text = new string('a', streamLength);
            var password = "password";

            var encryptionKey = StreamEncryptionKey.GenerateForPassword(password);
            var blockSize = encryptionKey.GetAesImplementation().IV.Length;

            using (var original = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                var encryptedStream = Encrypt(encryptionKey, original);
                var wrapper = new EncryptedStreamWrapper(encryptedStream, original.Length, blockSize);
                var calculatedLength = wrapper.Length;

                Assert.Equal(expectedLength, calculatedLength);

                long actualLength = 0;
                while (encryptedStream.ReadByte() != -1)
                {
                    actualLength++;
                }
                Assert.Equal(expectedLength, actualLength);
            }
        }

        private CryptoStream Encrypt(StreamEncryptionKey key, Stream stream)
        {
            using (var aes = key.GetAesImplementation())
            {
                var transform = aes.CreateEncryptor(aes.Key, aes.IV);
                var cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);

                return cryptoStream;
            }
        }
    }
}
