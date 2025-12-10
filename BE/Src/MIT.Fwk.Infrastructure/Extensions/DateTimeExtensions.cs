using System;
using System.Globalization;

namespace MIT.Fwk.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for DateTime manipulation and timezone conversion
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a UTC DateTime to a specified timezone with custom formatting
        /// </summary>
        /// <param name="utcDate">UTC DateTime to convert</param>
        /// <param name="offsetMinutes">Timezone offset in minutes (e.g., +120 for UTC+2, -300 for UTC-5)</param>
        /// <param name="format">Optional format string (e.g., "yyyy-MM-dd HH:mm:ss")</param>
        /// <param name="culture">Optional culture for formatting</param>
        /// <returns>Formatted date string in the target timezone</returns>
        public static string ConvertToTimeZone(this DateTime utcDate, int offsetMinutes = 0, string format = null, CultureInfo culture = null)
        {
            if (offsetMinutes == 0 && format == null && culture == null)
            {
                return utcDate.ToString(CultureInfo.InvariantCulture);
            }

            TimeZoneInfo targetTimeZone = TimeZoneInfo.Utc;

            if (offsetMinutes != 0)
            {
                TimeSpan offset = TimeSpan.FromMinutes(-offsetMinutes);
                string sign = offset >= TimeSpan.Zero ? "+" : "-";
                string offsetStr = $"{Math.Abs(offset.Hours):D2}:{Math.Abs(offset.Minutes):D2}";

                targetTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                    id: $"CustomUTC{sign}{offsetStr}",
                    baseUtcOffset: offset,
                    displayName: $"UTC{sign}{offsetStr}",
                    standardDisplayName: $"UTC{sign}{offsetStr}"
                );
            }

            DateTime convertedDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, targetTimeZone);

            if (format != null && culture != null)
            {
                return convertedDate.ToString(format, culture);
            }
            else if (format != null)
            {
                return convertedDate.ToString(format);
            }
            else if (culture != null)
            {
                return convertedDate.ToString(culture);
            }

            return convertedDate.ToString(CultureInfo.InvariantCulture);
        }
    }
}
