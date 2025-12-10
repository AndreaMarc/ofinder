using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MIT.Fwk.Scheduler.Services
{
    /// <summary>
    /// Implementation of ILogFileReader for reading log files from disk
    /// </summary>
    public class LogFileReader : ILogFileReader
    {
        private readonly MongoLogOptions _options;
        private readonly ILogger<LogFileReader> _logger;
        private readonly HashSet<string> _processingFiles = new();

        public LogFileReader(IOptions<MongoLogOptions> options, ILogger<LogFileReader> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<string> GetFilesToProcess()
        {
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.LogDirectory);

            if (!Directory.Exists(logDirectory))
            {
                _logger.LogWarning("Log directory does not exist: {LogDirectory}", logDirectory);
                return Enumerable.Empty<string>();
            }

            List<string> files = Directory.GetFiles(logDirectory, _options.FilePattern)
                .Where(f => !_processingFiles.Contains(f))
                .OrderBy(f => f)
                .Take(_options.MaxFilesPerBatch)
                .ToList();

            // Mark files as being processed
            foreach (string file in files)
            {
                _processingFiles.Add(file);
            }

            _logger.LogInformation("Found {Count} files to process", files.Count);
            return files;
        }

        public IEnumerable<string> ReadLines(string filePath)
        {
            return File.ReadLines(filePath);
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
                _processingFiles.Remove(filePath);
                _logger.LogInformation("File deleted: {FilePath}", Path.GetFileName(filePath));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {FilePath}", Path.GetFileName(filePath));
                return false;
            }
        }

        public void WriteErrorFile(string originalFilePath, IEnumerable<string> errorLines)
        {
            string fileName = Path.GetFileName(originalFilePath);
            string directory = Path.GetDirectoryName(originalFilePath) ?? string.Empty;
            string errorFilePath = Path.Combine(directory, fileName.Replace("LOGMONGO", _options.ErrorFilePrefix));

            File.WriteAllLines(errorFilePath, errorLines);
            _logger.LogWarning("Error file created: {ErrorFilePath} ({Count} lines)", Path.GetFileName(errorFilePath), errorLines.Count());
        }

        public bool IsFileReadyForProcessing(string filePath)
        {
            string fileName = Path.GetFileName(filePath).ToLowerInvariant();

            // Check if filename ends with a digit (rolled over log file)
            if (char.IsDigit(fileName.Last()))
            {
                return true;
            }

            // Check if file is older than today (extract date from filename YYYY-MM-DD)
            try
            {
                string dateStr = fileName.Substring(0, 10);
                DateTime fileDate = DateTime.ParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                return fileDate.AddDays(1) <= DateTime.Now;
            }
            catch
            {
                // If date parsing fails, consider file ready for processing
                _logger.LogWarning("Could not parse date from filename: {FileName}", fileName);
                return true;
            }
        }
    }
}
