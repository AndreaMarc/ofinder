#nullable enable

using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Services;
using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;

namespace MIT.Fwk.Licensing
{
    /// <summary>
    /// Service for managing software licensing validation and activation
    /// </summary>
    public class LicenseService : ILicenseService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly LicenseOptions _options;

        public LicenseService(IEncryptionService encryptionService, IOptions<LicenseOptions> options)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public bool IsValid()
        {
            string path = GetLicenseFilePath();

            if (!File.Exists(path))
            {
                throw new LicenseException(LicenseErrorCode.NoLicense, $"No License: {LicenseErrorCode.NoLicense}");
            }

            string license = File.ReadAllText(path);
            string chkStr = DecryptLicense(license);

            ValidateLicenseContent(chkStr, path);

            // Update last read timestamp
            UpdateLicenseTimestamp(chkStr, path);

            return true;
        }

        /// <inheritdoc/>
        public void ActivateLicense(long key, int months = 12)
        {
            if (!IsKeyValid(key))
            {
                Console.WriteLine("License activation failed: invalid key.");
                throw new LicenseException(LicenseErrorCode.InvalidActivationKey, "License activation failed: invalid key.");
            }

            string path = GetLicenseFilePath();
            string chkStr = $"{GetSerial()};{DateTime.Now.AddMonths(months).ToUniversalTime().Ticks};{DateTime.Now.ToUniversalTime().Ticks};{path}";
            string encryptionKey = GetEncryptionKey();
            string lic = _encryptionService.EncryptString(chkStr, encryptionKey);

            File.WriteAllText(path, lic);
            Console.WriteLine("License activation successful.");
        }

        /// <inheritdoc/>
        public long GenerateKey()
        {
            long x = GetSerial();
            return Convert.ToInt64(x * x + 53 / (double)x + 113 * (x / (double)4));
        }

        #region Private Helper Methods

        private string GetLicenseFilePath()
        {
            return _options.LicenseFilePath
                ?? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "license.lic");
        }

        private string DecryptLicense(string license)
        {
            try
            {
                // Try primary key
                return _encryptionService.DecryptString(license, GetEncryptionKey());
            }
            catch
            {
                try
                {
                    // Try trial key for backward compatibility
                    return _encryptionService.DecryptString(license, GetTrialEncryptionKey());
                }
                catch
                {
                    throw new LicenseException(LicenseErrorCode.InvalidFormat, $"Invalid License: {LicenseErrorCode.InvalidFormat}");
                }
            }
        }

        private void ValidateLicenseContent(string chkStr, string path)
        {
            if (string.IsNullOrEmpty(chkStr))
            {
                throw new LicenseException(LicenseErrorCode.EmptyContent, $"Invalid License: {LicenseErrorCode.EmptyContent}");
            }

            string[] parts = chkStr.Split(';');

            if (parts.Length != 4)
            {
                throw new LicenseException(LicenseErrorCode.InvalidStructure, $"Invalid License: {LicenseErrorCode.InvalidStructure}");
            }

            // Validate serial number
            if (parts[0] != GetSerial().ToString())
            {
                throw new LicenseException(LicenseErrorCode.SerialMismatch, $"Invalid License: {LicenseErrorCode.SerialMismatch}");
            }

            // Validate expiration date
            DateTime expirationDate = new(long.Parse(parts[1]));
            if (expirationDate.CompareTo(DateTime.Now.ToUniversalTime()) < 0)
            {
                throw new LicenseException(LicenseErrorCode.Expired, $"License Expired: {expirationDate:dd/MM/yyyy}");
            }

            // Validate last read timestamp (detect date tampering)
            DateTime lastRead = new(long.Parse(parts[2]));
            if (lastRead.CompareTo(DateTime.Now.ToUniversalTime()) > 0)
            {
                throw new LicenseException(LicenseErrorCode.DateTampered, $"Invalid License: {LicenseErrorCode.DateTampered}");
            }

            // Validate file path
            if (!parts[3].Equals(path))
            {
                throw new LicenseException(LicenseErrorCode.PathAltered, $"Invalid License: {LicenseErrorCode.PathAltered}");
            }
        }

        private void UpdateLicenseTimestamp(string chkStr, string path)
        {
            string[] parts = chkStr.Split(';');
            string updatedChkStr = $"{parts[0]};{parts[1]};{DateTime.Now.ToUniversalTime().Ticks};{path}";
            string encryptionKey = GetEncryptionKey();
            string lic = _encryptionService.EncryptString(updatedChkStr, encryptionKey);
            File.WriteAllText(path, lic);
        }

        private bool IsKeyValid(long key)
        {
            long x = GetSerial();
            long k = Convert.ToInt64(x * x + 53 / (double)x + 113 * (x / (double)4));
            return k == key;
        }

        private string GetEncryptionKey()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream? stream = assembly.GetManifestResourceStream(_options.PrimaryKeyResource);

            if (stream == null)
            {
                throw new InvalidOperationException($"Cannot find embedded resource: {_options.PrimaryKeyResource}");
            }

            using StreamReader reader = new(stream);
            string str = reader.ReadToEnd().TrimEnd('\r', '\n').Trim().PadRight(24, '0');

            // Fallback to alternate resource if default value
            if (str.Equals("0000-0000-0000-0000"))
            {
                stream = assembly.GetManifestResourceStream(_options.FallbackKeyResource);
                if (stream != null)
                {
                    using StreamReader fallbackReader = new(stream);
                    str = fallbackReader.ReadToEnd().TrimEnd('\r', '\n').Trim().PadRight(24, '0');
                }
            }

            return str;
        }

        private string GetTrialEncryptionKey()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream? stream = assembly.GetManifestResourceStream(_options.TrialKeyResource);

            if (stream == null)
            {
                throw new InvalidOperationException($"Cannot find embedded resource: {_options.TrialKeyResource}");
            }

            using StreamReader reader = new(stream);
            return reader.ReadToEnd().TrimEnd('\r', '\n').Trim();
        }

        private long GetSerial()
        {
            long hardwareSerial = CalculateHardwareSerial();
            long encryptionKeySerial = CalculateEncryptionKeySerial();
            return hardwareSerial + encryptionKeySerial;
        }

        private long CalculateEncryptionKeySerial()
        {
            long v = 0;
            string s = GetEncryptionKey();
            int index = 1;

            foreach (char c in s)
            {
                if (char.IsDigit(c))
                {
                    v += v + c * (index * 2);
                }
                else if (char.IsLetter(c))
                {
                    v += v + c * (index * 3);
                }
                index++;
            }

            return v;
        }

        private long CalculateHardwareSerial()
        {
            string identifier = GetHardwareIdentifier();
            long w = 0;
            int index = 1;

            foreach (char c in identifier)
            {
                if (char.IsDigit(c))
                {
                    w += w + long.Parse(c.ToString()) * (index * 2);
                }
                else if (char.IsLetter(c))
                {
                    // Convert hex character to numeric value (A=10, B=11, ..., F=15, other=16)
                    int value = c >= 'A' && c <= 'F'
                        ? Convert.ToInt32(c.ToString(), 16)
                        : c >= 'a' && c <= 'f'
                            ? Convert.ToInt32(c.ToString(), 16)
                            : 16;

                    w += w + value * (index * 2);
                }

                index++;
            }

            return w;
        }

        private string GetHardwareIdentifier()
        {
            PhysicalAddress? mac = GetMacAddress();

            if (mac != null)
            {
                byte[] bytes = mac.GetAddressBytes();
                string hexString = string.Concat(bytes.Select(b => b.ToString("X2")));

                // Add additional calculation based on MAC address bytes
                long additionalValue = 0;
                foreach (byte y in bytes)
                {
                    additionalValue += Convert.ToInt64(y * y + 51 / (double)y + 107 * (y / (double)5));
                }

                return hexString;
            }
            else
            {
                // Fallback to hostname
                IPGlobalProperties pcProps = IPGlobalProperties.GetIPGlobalProperties();
                return pcProps.HostName;
            }
        }

        private PhysicalAddress? GetMacAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            if (nics == null || nics.Length < 1)
            {
                return null;
            }

            NetworkInterface? adapter = nics.FirstOrDefault(a =>
                a.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                a.OperationalStatus == OperationalStatus.Up);

            return adapter?.GetPhysicalAddress();
        }

        #endregion
    }
}
