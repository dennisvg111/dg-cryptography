using DG.Common.Exceptions;
using System;
using System.Collections.Generic;

namespace DG.Cryptography.Hashing
{
    internal class Pbkdf2Sha1HashResult
    {
        public const char Delimiter = ':';

        private readonly string _algorithm;
        private readonly int _iterations;
        private readonly int _hashSize;
        private readonly byte[] _salt;
        private readonly byte[] _hash;

        public int Iterations => _iterations;
        public int HashSize => _hashSize;
        public byte[] Salt => _salt;

        public Pbkdf2Sha1HashResult(string algorithm, int iterations, int hashSize, byte[] salt, byte[] hash)
        {
            _algorithm = algorithm;
            _iterations = iterations;
            _hashSize = hashSize;
            _salt = salt;
            _hash = hash;
        }

        public override string ToString()
        {
            return _algorithm + Delimiter + _iterations + Delimiter + _hashSize + Delimiter + Convert.ToBase64String(_salt) + Delimiter + Convert.ToBase64String(_hash);
        }

        public bool SlowEquals(Pbkdf2Sha1HashResult testHash)
        {
            IReadOnlyList<byte> a = _hash;
            IReadOnlyList<byte> b = testHash._hash;
            uint diff = (uint)a.Count ^ (uint)b.Count;
            for (int i = 0; i < a.Count && i < b.Count; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        public static Pbkdf2Sha1HashResult Parse(string hashedString)
        {
            string[] split = hashedString.Split(new char[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);

            ThrowIf.Collection(split, nameof(hashedString)).CountOtherThan(5);
            string algorithm = split[0];

            int iterations;
            if (!int.TryParse(split[1], out iterations))
            {
                throw new FormatException("Could not parse iteration count from the given token string.");
            }
            ThrowIf.Number(iterations, nameof(hashedString)).IsZero();
            ThrowIf.Number(iterations, nameof(hashedString)).IsNegative();

            byte[] salt;
            try
            {
                salt = Convert.FromBase64String(split[3]);
            }
            catch (Exception ex)
            {
                throw new FormatException("Invalid salt.", ex);
            }

            byte[] hash;
            try
            {
                hash = Convert.FromBase64String(split[4]);
            }
            catch (Exception ex)
            {
                throw new FormatException("Invalid hash.", ex);
            }

            int hashSize;
            if (!int.TryParse(split[2], out hashSize))
            {
                throw new FormatException("Could not parse hash size from the given token string.");
            }
            ThrowIf.Number(hashSize, nameof(hashedString)).IsNotBetweenInclusive(hash.Length, hash.Length, "Invalid hash size.");
            return new Pbkdf2Sha1HashResult(algorithm, iterations, hashSize, salt, hash);
        }

        public static bool TryParse(string s, out Pbkdf2Sha1HashResult hashedString)
        {
            try
            {
                hashedString = Parse(s);
                return true;
            }
            catch (Exception)
            {
                hashedString = null;
                return false;
            }
        }
    }
}
