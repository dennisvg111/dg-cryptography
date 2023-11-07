using DG.Cryptography.Hashing;
using DG.Cryptography.Random;
using Xunit;

namespace DG.Cryptography.Tests.Hashing
{
    public class Pbkdf2Sha1HashTests
    {
        [Fact]
        public void Hash_CanBeVerified()
        {
            var hashing = new Pbkdf2Sha1Hash(64, 64000, 28);
            var plainText = TokenGenerator.Default.Generate(25);

            var hashedString = hashing.Hash(plainText);

            Assert.True(Pbkdf2Sha1Hash.VerifyHash(plainText, hashedString));
        }

        [Fact]
        public void VerifyHash_ReturnsTrue()
        {
            var hashedString = "64000:ZCOxw2QzkmuPqlY7oHLT1Jz1afDKw9FixQBtOx2bfGhPVB2ypIHG_hW3PGJx4TD4P7QUAE6cydAF4olup05YjA:AbZT8bivO0O526dq9sOi0kU5zNO8vsjdyqMgkw";
            var plainText = "Hello world!";

            Assert.True(Pbkdf2Sha1Hash.VerifyHash(plainText, hashedString));
        }

        [Fact]
        public void VerifyHash_ReturnsFalse()
        {
            var hashedString = "64000:ZCOxw2QzkmuPqlY7oHLT1Jz1afDKw9FixQBtOx2bfGhPVB2ypIHG_hW3PGJx4TD4P7QUAE6cydAF4olup05YjA:AbZT8bivO0O526dq9sOi0kU5zNO8vsjdyqMgkw";
            var plainText = "Hello world";

            Assert.False(Pbkdf2Sha1Hash.VerifyHash(plainText, hashedString));
        }
    }
}
