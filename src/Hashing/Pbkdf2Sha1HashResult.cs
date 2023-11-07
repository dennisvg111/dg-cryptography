using DG.Common;
using DG.Common.Exceptions;
using System;
using System.Collections.Generic;

namespace DG.Cryptography.Hashing
{
    internal class Pbkdf2Sha1HashResult
    {
        public const char Delimiter = ':';

        private readonly int _iterations;
        private readonly byte[] _salt;
        private readonly byte[] _hash;

        public int Iterations => _iterations;
        public int HashSize => _hash.Length;
        public byte[] Salt => _salt;

        public Pbkdf2Sha1HashResult(int iterations, byte[] salt, byte[] hash)
        {
            _iterations = iterations;
            _salt = salt;
            _hash = hash;
        }

        public override string ToString()
        {
            return _iterations.ToString() + Delimiter + SafeBase64.Encode(_salt) + Delimiter + SafeBase64.Encode(_hash);
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

            ThrowIf.Collection(split, nameof(hashedString)).CountOtherThan(3);

            if (!int.TryParse(split[0], out int iterations))
            {
                throw new FormatException("Could not parse iteration count from the given token string.");
            }
            ThrowIf.Number(iterations, nameof(hashedString)).IsZero();
            ThrowIf.Number(iterations, nameof(hashedString)).IsNegative();

            byte[] salt;
            try
            {
                salt = SafeBase64.Decode(split[1]);
            }
            catch (Exception ex)
            {
                throw new FormatException("Invalid salt.", ex);
            }

            byte[] hash;
            try
            {
                hash = SafeBase64.Decode(split[2]);
            }
            catch (Exception ex)
            {
                throw new FormatException("Invalid hash.", ex);
            }

            return new Pbkdf2Sha1HashResult(iterations, salt, hash);
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
