namespace MIT.Fwk.Scheduler
{
    /// <summary>
    /// Configuration options for MongoDB log processing
    /// </summary>
    public class MongoLogOptions
    {
        /// <summary>
        /// Directory where log files are stored.
        /// Default: {AppDomain.CurrentDomain.BaseDirectory}/logs
        /// </summary>
        public string LogDirectory { get; set; } = "logs";

        /// <summary>
        /// File pattern for log files to process.
        /// Default: *LOGMONGO*.*
        /// </summary>
        public string FilePattern { get; set; } = "*LOGMONGO*.*";

        /// <summary>
        /// Maximum number of files to process per job execution.
        /// Default: 5
        /// </summary>
        public int MaxFilesPerBatch { get; set; } = 5;

        /// <summary>
        /// MongoDB collection name for storing logs.
        /// Read from configuration: Logging:LogCollection
        /// </summary>
        public string LogCollectionName { get; set; } = "FwkLog";

        /// <summary>
        /// Cron expression for QueueManager job scheduling.
        /// Read from configuration: Scheduler:QueueManager
        /// </summary>
        public string CronExpression { get; set; } = "0 0/5 * * * ?"; // every 5 minutes

        /// <summary>
        /// Prefix for error files containing lines that failed to process.
        /// Default: ERRORLINESMONGOLOG
        /// </summary>
        public string ErrorFilePrefix { get; set; } = "ERRORLINESMONGOLOG";
    }
}
