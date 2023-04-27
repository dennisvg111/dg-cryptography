using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DG.Cryptography.Encryption
{
    /// <summary>
    /// Provides an encryption algorithm using the Rijndael algorithm, and SHA1 based RFC 2898 password based cryptography.
    /// </summary>
    public class RijndaelSha1Encryption
    {
        private int _keyBytes;
        private int _iterations;

        public int KeyBytes => _keyBytes;
        public int Iterations => _iterations;

        public RijndaelSha1Encryption(int keyBytes, int iterations)
        {
            _keyBytes = keyBytes;
            _iterations = iterations;
        }

        public static RijndaelSha1Encryption Default => new RijndaelSha1Encryption(32, 1000);

        public string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentException();
            }
            byte[] saltStringBytes = GenerateBitsOfRandomEntropy(_keyBytes * 8);
            byte[] ivStringBytes = GenerateBitsOfRandomEntropy(_keyBytes * 8);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(key, saltStringBytes, _iterations))
            {
                byte[] keyBytes = password.GetBytes(_keyBytes);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = _keyBytes * 8;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                byte[] cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return null;
            }
            // format: salt IV ciphertext
            byte[] cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            byte[] saltStringBytes = cipherTextBytesWithSaltAndIv.Take(_keyBytes).ToArray();
            byte[] ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(_keyBytes).Take(_keyBytes).ToArray();
            byte[] cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(_keyBytes * 2).Take(cipherTextBytesWithSaltAndIv.Length - (_keyBytes * 2)).ToArray();

            using (Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(key, saltStringBytes, _iterations))
            {
                byte[] keyBytes = password.GetBytes(_keyBytes);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = _keyBytes * 8;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] GenerateBitsOfRandomEntropy(int bits)
        {
            byte[] randomBytes = new byte[bits / 8];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
