using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MIT.Fwk.Core.Services
{
    /// <summary>
    /// Implementation of IEncryptionService.
    /// Provides modern encryption and hashing using SHA-256 (replaces legacy SHA-1).
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        public string EncryptString(string text, string keyString)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrEmpty(keyString))
                throw new ArgumentNullException(nameof(keyString));

            byte[] key = Encoding.UTF8.GetBytes(keyString);

            using Aes aesAlg = Aes.Create();
            try
            {
                using ICryptoTransform encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV);
                using MemoryStream msEncrypt = new();
                using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new(csEncrypt))
                {
                    swEncrypt.Write(text);
                }

                byte[] iv = aesAlg.IV;
                byte[] encryptedContent = msEncrypt.ToArray();
                byte[] result = new byte[iv.Length + encryptedContent.Length];

                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

                return Convert.ToBase64String(result);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Encryption failed", ex);
            }
        }

        public string DecryptString(string cipherText, string keyString)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));
            if (string.IsNullOrEmpty(keyString))
                throw new ArgumentNullException(nameof(keyString));

            byte[] fullCipher = Convert.FromBase64String(cipherText);
            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);

            byte[] key = Encoding.UTF8.GetBytes(keyString);

            using Aes aesAlg = Aes.Create();
            using ICryptoTransform decryptor = aesAlg.CreateDecryptor(key, iv);
            using MemoryStream msDecrypt = new(cipher);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);

            return srDecrypt.ReadToEnd();
        }

        public string GenerateSha256Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            byte[] data = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public string GenerateMd5Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            byte[] data = MD5.HashData(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public byte[] Sign(string text, string certPath)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrEmpty(certPath))
                throw new ArgumentNullException(nameof(certPath));

            // Load certificate with private key (.NET 8 compatible)
            X509Certificate2 cert = new X509Certificate2(certPath);
            RSA rsa = cert.GetRSAPrivateKey()
                ?? throw new CryptographicException("Certificate does not contain an RSA private key");

            // Hash the data with SHA-256 (replaces legacy SHA-1)
            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA256.HashData(data);

            // Sign the hash
            RSACryptoServiceProvider csp = rsa as RSACryptoServiceProvider
                ?? throw new CryptographicException("RSA provider is not RSACryptoServiceProvider");

            return csp.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public bool Verify(string text, byte[] signature, string certPath)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));
            if (signature == null || signature.Length == 0)
                throw new ArgumentNullException(nameof(signature));
            if (string.IsNullOrEmpty(certPath))
                throw new ArgumentNullException(nameof(certPath));

            // Load certificate (public key only) (.NET 8 compatible)
            X509Certificate2 cert = new X509Certificate2(certPath);
            RSA rsa = cert.GetRSAPublicKey()
                ?? throw new CryptographicException("Certificate does not contain an RSA public key");

            // Hash the data with SHA-256 (replaces legacy SHA-1)
            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA256.HashData(data);

            // Verify the signature
            return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
