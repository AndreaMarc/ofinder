using System;

namespace MIT.Fwk.Licensing
{
    /// <summary>
    /// Exception thrown when license validation or activation fails
    /// </summary>
    public class LicenseException : Exception
    {
        public LicenseErrorCode ErrorCode { get; }

        public LicenseException(LicenseErrorCode errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public LicenseException(LicenseErrorCode errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// Error codes for license validation failures
    /// </summary>
    public enum LicenseErrorCode
    {
        /// <summary>No license file found</summary>
        NoLicense = 57880,

        /// <summary>Invalid license file format (decryption failed)</summary>
        InvalidFormat = 56892,

        /// <summary>License content is empty</summary>
        EmptyContent = 34582,

        /// <summary>Invalid license structure (wrong number of fields)</summary>
        InvalidStructure = 35789,

        /// <summary>License serial number mismatch (wrong machine)</summary>
        SerialMismatch = 34678,

        /// <summary>License has expired</summary>
        Expired = 0,

        /// <summary>System date has been tampered with</summary>
        DateTampered = 89785,

        /// <summary>License file path has been altered</summary>
        PathAltered = 23689,

        /// <summary>Invalid activation key</summary>
        InvalidActivationKey = 0
    }
}
