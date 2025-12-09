using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Scheduler.Services
{
    # nullable enable
    /// <summary>
    /// Facade implementation of ILogFileProcessor that coordinates log processing workflow
    /// </summary>
    public class LogFileProcessor : ILogFileProcessor
    {
        private readonly ILogFileReader _fileReader;
        private readonly ILogLineParser _lineParser;
        private readonly ILogTransformer _transformer;
        private readonly IMongoLogRepository _repository;
        private readonly ILogger<LogFileProcessor> _logger;

        public LogFileProcessor(
            ILogFileReader fileReader,
            ILogLineParser lineParser,
            ILogTransformer transformer,
            IMongoLogRepository repository,
            ILogger<LogFileProcessor> logger)
        {
            _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            _lineParser = lineParser ?? throw new ArgumentNullException(nameof(lineParser));
            _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessLogsAsync()
        {
            try
            {
                IEnumerable<string> files = _fileReader.GetFilesToProcess();
                List<string> processedFiles = new();

                foreach (string file in files)
                {
                    try
                    {
                        await ProcessFileAsync(file);
                        processedFiles.Add(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process file: {FileName}", Path.GetFileName(file));
                    }
                }

                if (processedFiles.Count > 0)
                {
                    _logger.LogInformation("Successfully processed {Count} files: {Files}",
                        processedFiles.Count,
                        string.Join(", ", processedFiles.Select(Path.GetFileName)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in log processing workflow");
                throw;
            }
        }

        private async Task ProcessFileAsync(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            // Check if file is ready for processing
            if (!_fileReader.IsFileReadyForProcessing(filePath))
            {
                _logger.LogDebug("File not ready for processing: {FileName}", fileName);
                return;
            }

            _logger.LogInformation("Processing file: {FileName}", fileName);

            List<BsonDocument> documentsToInsert = new();
            List<string> errorLines = new();

            // Read and parse file line by line
            foreach (string line in _fileReader.ReadLines(filePath))
            {
                if (_lineParser.TryParse(line, out LogToMongo? logToMongo) && logToMongo != null)
                {
                    try
                    {
                        BsonDocument document = _transformer.Transform(logToMongo);
                        documentsToInsert.Add(document);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to transform log line to BsonDocument");
                        errorLines.Add(line);
                    }
                }
                else
                {
                    errorLines.Add(line);
                }
            }

            // Insert documents into MongoDB
            if (documentsToInsert.Count > 0)
            {
                await _repository.InsertManyAsync(documentsToInsert);
            }

            // Delete processed file
            if (_fileReader.DeleteFile(filePath))
            {
                _logger.LogInformation("File processed and deleted: {FileName} ({DocumentCount} documents, {ErrorCount} errors)",
                    fileName, documentsToInsert.Count, errorLines.Count);
            }

            // Write error file if there were parsing errors
            if (errorLines.Count > 0)
            {
                _fileReader.WriteErrorFile(filePath, errorLines);
            }
        }
    }
}
