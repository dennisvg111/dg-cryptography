using DG.Common;
using DG.Cryptography.Encryption.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace DG.Cryptography.Tests.Encryption.IO
{
    public class StreamEncryptionTests
    {
        [Fact]
        public void Encrypt_Returns_Unrecognizable()
        {
            var text = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            var password = "password";

            var encryptionKey = StreamEncryptionKey.GenerateForPassword(password);
            var decryptionKey = StreamEncryptionKey.FromPublicKey(encryptionKey.PublicKey, password);

            using (var original = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                var encryptedStream = StreamEncryption.Encrypt(original, encryptionKey);
                Assert.NotNull(encryptedStream);

                using (var reader = new StreamReader(encryptedStream))
                {
                    var decryptedText = reader.ReadToEnd();
                    Assert.NotEqual(text, decryptedText);

                    int distinctCharacterCount = decryptedText.Distinct().Count();
                    Assert.True(distinctCharacterCount >= 10, "Encrypting the same bytes multiple times should return many distinct encrypted bytes.");
                }
            }
        }
        [Fact]
        public void Encrypt_Reversible()
        {
            var text = "Hello world!";
            var password = "password";

            var encryptionKey = StreamEncryptionKey.GenerateForPassword(password);
            var decryptionKey = StreamEncryptionKey.FromPublicKey(encryptionKey.PublicKey, password);

            using (var original = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                var encryptedStream = StreamEncryption.Encrypt(original, encryptionKey);
                Assert.NotNull(encryptedStream);


                using (var decrypted = StreamEncryption.Decrypt(encryptedStream, decryptionKey))
                using (var reader = new StreamReader(decrypted))
                {
                    var decryptedText = reader.ReadToEnd();
                    Assert.Equal(text, decryptedText);
                }
            }
        }

        private static readonly Process _currentProcess = Process.GetCurrentProcess();

        [Fact(Skip = "Large memory footprint test, only run in local environments")]
        public void Encrypt_MemoryUsage_LowWhileNotReading()
        {
            var key = StreamEncryptionKey.GenerateForPassword("MyPassword123");

            using (var file = DummyFile.WithSize(ByteSize.FromMB(500)))
            using (var stream = file.GetStream())
            {
                var encryptedStream = StreamEncryption.Encrypt(stream, key);
                Assert.NotNull(encryptedStream);

                var firstByte = encryptedStream.ReadByte();
                Assert.NotEqual(-1, firstByte);


                _currentProcess.Refresh();
                var usedMemory = ByteSize.FromBytes(_currentProcess.PrivateMemorySize64);
                Assert.True(usedMemory < ByteSize.FromMB(500), $"Expected less than 500MB, actual {usedMemory}");

                using (var ms = new MemoryStream())
                {
                    encryptedStream.CopyTo(ms);
                    _currentProcess.Refresh();
                    usedMemory = ByteSize.FromBytes(_currentProcess.PrivateMemorySize64);
                    Assert.True(usedMemory > ByteSize.FromMB(500), $"Expected more than 500MB, actual {usedMemory}");
                }
            }
        }

        public class DummyFile : IDisposable
        {
            private readonly ByteSize _size;
            private readonly string _name;
            private bool disposedValue;

            public override string ToString()
            {
                return _name + ": " + _size;
            }

            public DummyFile(long bytes)
            {
                _size = ByteSize.FromBytes(bytes);

                Directory.CreateDirectory(@"C:\tmp");
                _name = @"C:\tmp\" + Guid.NewGuid().ToString().ToLowerInvariant() + ".dummy";
                FileStream fs = new FileStream(_name, FileMode.CreateNew);
                fs.Seek(bytes - 1, SeekOrigin.Begin);
                fs.WriteByte(0);
                fs.Close();
            }

            public static DummyFile WithSize(ByteSize size)
            {
                return new DummyFile(size.ToByteCount());
            }

            public Stream GetStream()
            {
                return File.OpenRead(_name);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    File.Delete(_name);
                }
                disposedValue = true;
            }

            // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
            // ~DummyFile()
            // {
            //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            //     Dispose(disposing: false);
            // }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
