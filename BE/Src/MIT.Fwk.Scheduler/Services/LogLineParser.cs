using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using Newtonsoft.Json;
using System;

namespace MIT.Fwk.Scheduler.Services
{
    # nullable enable
    /// <summary>
    /// Implementation of ILogLineParser for parsing JSON log lines
    /// </summary>
    public class LogLineParser : ILogLineParser
    {
        private readonly ILogger<LogLineParser> _logger;

        public LogLineParser(ILogger<LogLineParser> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool TryParse(string line, out LogToMongo? logToMongo)
        {
            logToMongo = null;

            try
            {
                // Find JSON boundaries in the log line
                int startIndex = line.IndexOf('{');
                int endIndex = line.LastIndexOf('}');

                if (startIndex == -1 || endIndex == -1 || startIndex >= endIndex)
                {
                    _logger.LogWarning("Line does not contain valid JSON boundaries");
                    return false;
                }

                string jsonContent = line.Substring(startIndex, endIndex - startIndex + 1);
                logToMongo = JsonConvert.DeserializeObject<LogToMongo>(jsonContent);

                return logToMongo != null;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse JSON from log line");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error parsing log line");
                return false;
            }
        }
    }
}
