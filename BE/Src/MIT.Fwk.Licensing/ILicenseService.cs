using System;

namespace MIT.Fwk.Licensing
{
    /// <summary>
    /// Service for managing software licensing validation and activation
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// Validates the current license file
        /// </summary>
        /// <returns>True if license is valid, throws exception otherwise</returns>
        /// <exception cref="LicenseException">Thrown when license is invalid, expired, or missing</exception>
        bool IsValid();

        /// <summary>
        /// Activates a new license with the provided key
        /// </summary>
        /// <param name="key">License activation key</param>
        /// <param name="months">Number of months for license validity (default: 12)</param>
        /// <exception cref="LicenseException">Thrown when activation key is invalid</exception>
        void ActivateLicense(long key, int months = 12);

        /// <summary>
        /// Generates a license key for the current machine
        /// </summary>
        /// <returns>License key as long integer</returns>
        long GenerateKey();
    }
}
