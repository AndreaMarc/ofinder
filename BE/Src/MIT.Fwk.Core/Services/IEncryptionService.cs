namespace MIT.Fwk.Core.Services
{
    /// <summary>
    /// Encryption and hashing service abstraction.
    /// Provides AES encryption, SHA-256 hashing, and digital signatures.
    /// Replaces static EncryptionHelper with dependency injection pattern.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a string using AES encryption with the specified key.
        /// </summary>
        /// <param name="text">Plain text to encrypt</param>
        /// <param name="keyString">32-character encryption key (256-bit)</param>
        /// <returns>Base64-encoded encrypted string (IV + ciphertext)</returns>
        string EncryptString(string text, string keyString);

        /// <summary>
        /// Decrypts an AES-encrypted string.
        /// </summary>
        /// <param name="cipherText">Base64-encoded encrypted string</param>
        /// <param name="keyString">32-character encryption key (256-bit)</param>
        /// <returns>Decrypted plain text</returns>
        string DecryptString(string cipherText, string keyString);

        /// <summary>
        /// Generates SHA-256 hash of input string.
        /// Use for secure password hashing or data integrity verification.
        /// </summary>
        /// <param name="input">Input string to hash</param>
        /// <returns>SHA-256 hash as hex string</returns>
        string GenerateSha256Hash(string input);

        /// <summary>
        /// Generates MD5 hash of input string.
        /// Use only for non-security purposes (checksums, cache keys).
        /// MD5 is cryptographically broken - do not use for passwords or security.
        /// </summary>
        /// <param name="input">Input string to hash</param>
        /// <returns>MD5 hash as hex string</returns>
        string GenerateMd5Hash(string input);

        /// <summary>
        /// Signs data using RSA certificate private key with SHA-256.
        /// </summary>
        /// <param name="text">Text to sign</param>
        /// <param name="certPath">Path to certificate file (.pfx or .cer)</param>
        /// <returns>Digital signature bytes</returns>
        byte[] Sign(string text, string certPath);

        /// <summary>
        /// Verifies RSA signature using certificate public key with SHA-256.
        /// </summary>
        /// <param name="text">Original text</param>
        /// <param name="signature">Signature bytes to verify</param>
        /// <param name="certPath">Path to certificate file (.cer)</param>
        /// <returns>True if signature is valid, false otherwise</returns>
        bool Verify(string text, byte[] signature, string certPath);
    }
}
