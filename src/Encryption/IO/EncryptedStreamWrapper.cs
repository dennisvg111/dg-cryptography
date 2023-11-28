using System.IO;
using System.Security.Cryptography;

namespace DG.Cryptography.Encryption.IO
{
    /// <summary>
    /// Provides a wrapper for <see cref="CryptoStream"/> that overrides <see cref="Length"/>.
    /// </summary>
    public class EncryptedStreamWrapper : Stream
    {
        private readonly CryptoStream _stream;
        private readonly long _length;

        /// <summary>
        /// Initializes a new instance of <see cref="EncryptedStreamWrapper"/> with the given <see cref="CryptoStream"/>, <paramref name="originalStreamLength"/>, and <paramref name="ivLength"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="originalStreamLength"></param>
        /// <param name="ivLength"></param>
        public EncryptedStreamWrapper(CryptoStream stream, long originalStreamLength, int ivLength)
        {
            _stream = stream;

            _length = originalStreamLength + ivLength - (originalStreamLength % ivLength);
        }

        /// <inheritdoc/>
        public override bool CanRead => _stream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => _stream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => _stream.CanWrite;

        /// <inheritdoc/>
        public override long Length => _length;

        /// <inheritdoc/>
        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        /// <inheritdoc/>
        public override void Flush()
        {
            _stream.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
    }
}
