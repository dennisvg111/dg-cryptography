using DG.Cryptography.Random;
using System.Security.Cryptography;

namespace DG.Cryptography.Hashing
{
    /// <summary>
    /// Provides a hashing algorithm using the PBKDF2 key derivation algorithm, and using HMAC with SHA-1 as hashing function.
    /// </summary>
    public class Pbkdf2Sha1Hash
    {
        private const string _algorithm = "sha1";

        /// <summary>
        /// 24
        /// </summary>
        public const int DefaultSaltBytes = 24;

        /// <summary>
        /// 64000
        /// </summary>
        public const int DefaultIterations = 64000;

        /// <summary>
        /// 18
        /// </summary>
        public const int DefaultHashBytes = 18;

        private static readonly Pbkdf2Sha1Hash _defaultInstance = new Pbkdf2Sha1Hash(DefaultSaltBytes, DefaultIterations, DefaultHashBytes);

        /// <summary>
        /// Returns an instance of <see cref="Pbkdf2Sha1Hash"/>, with <inheritdoc cref="DefaultSaltBytes"/> salt bytes, <inheritdoc cref="DefaultIterations"/> iterations, and <inheritdoc cref="DefaultHashBytes"/> bytes in the returned hashes.
        /// </summary>
        public static Pbkdf2Sha1Hash Default => _defaultInstance;

        private readonly int _saltBytes;
        private readonly int _iterations;
        private readonly int _hashBytes;

        /// <summary>
        /// Initializes a new instance of <see cref="Pbkdf2Sha1Hash"/>, with the given amount of salt bytes, amount of iterations, and amount of bytes in the returned hashes.
        /// </summary>
        /// <param name="saltBytes"></param>
        /// <param name="iterations"></param>
        /// <param name="hashBytes"></param>
        public Pbkdf2Sha1Hash(int saltBytes, int iterations, int hashBytes)
        {
            _saltBytes = saltBytes;
            _iterations = iterations;
            _hashBytes = hashBytes;
        }

        /// <summary>
        /// Creates a hashed version of the given <paramref name="plainText"/>.
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string Hash(string plainText)
        {
            byte[] salt = SecureRandomNumberProvider.Default.NextBytes(_saltBytes);

            var result = Pbkdf2(plainText, salt, _iterations, _hashBytes);
            return result.ToString();
        }

        /// <summary>
        /// Verifies if the given <paramref name="plainText"/> results in the given <paramref name="hashedString"/>.
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="hashedString"></param>
        /// <returns></returns>
        public static bool VerifyHash(string plainText, string hashedString)
        {
            var originalHash = Pbkdf2Sha1HashResult.Parse(hashedString);
            var testHash = Pbkdf2(plainText, originalHash.Salt, originalHash.Iterations, originalHash.HashSize);
            return originalHash.SlowEquals(testHash);
        }

        private static Pbkdf2Sha1HashResult Pbkdf2(string password, byte[] salt, int iterations, int outputBytes)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt))
            {
                pbkdf2.IterationCount = iterations;
                var hash = pbkdf2.GetBytes(outputBytes);
                return new Pbkdf2Sha1HashResult(_algorithm, iterations, outputBytes, salt, hash);
            }
        }
    }
}
