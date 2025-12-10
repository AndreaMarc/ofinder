using MIT.Fwk.Infrastructure.Entities;

namespace MIT.Fwk.Scheduler.Services
{
    # nullable enable
    /// <summary>
    /// Service for parsing JSON log lines
    /// </summary>
    public interface ILogLineParser
    {
        /// <summary>
        /// Parses a JSON log line into LogToMongo object
        /// </summary>
        /// <param name="line">Raw log line</param>
        /// <param name="logToMongo">Parsed LogToMongo object if successful</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        bool TryParse(string line, out LogToMongo? logToMongo);
    }
}
