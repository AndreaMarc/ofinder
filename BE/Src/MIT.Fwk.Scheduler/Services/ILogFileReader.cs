using System.Collections.Generic;

namespace MIT.Fwk.Scheduler.Services
{
    /// <summary>
    /// Service for reading log files from disk
    /// </summary>
    public interface ILogFileReader
    {
        /// <summary>
        /// Gets list of log files ready to be processed
        /// </summary>
        /// <returns>List of file paths</returns>
        IEnumerable<string> GetFilesToProcess();

        /// <summary>
        /// Reads all lines from a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Enumerable of lines from the file</returns>
        IEnumerable<string> ReadLines(string filePath);

        /// <summary>
        /// Deletes a processed log file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        bool DeleteFile(string filePath);

        /// <summary>
        /// Writes error lines to an error file
        /// </summary>
        /// <param name="originalFilePath">Original log file path</param>
        /// <param name="errorLines">Lines that failed processing</param>
        void WriteErrorFile(string originalFilePath, IEnumerable<string> errorLines);

        /// <summary>
        /// Checks if a file is ready to be processed (older than today or ends with a digit)
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if file is ready for processing</returns>
        bool IsFileReadyForProcessing(string filePath);
    }
}
