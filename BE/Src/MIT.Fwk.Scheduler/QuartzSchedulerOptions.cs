namespace MIT.Fwk.Scheduler
{
    /// <summary>
    /// Configuration options for Quartz.NET scheduler
    /// </summary>
    public class QuartzSchedulerOptions
    {
        /// <summary>
        /// Comma-separated list of job names to start.
        /// Example: "MongoLogBusManager,SyncManager"
        /// </summary>
        public string ToStart { get; set; } = string.Empty;

        /// <summary>
        /// Serializer type for Quartz (default: json for .NET Core compatibility)
        /// </summary>
        public string SerializerType { get; set; } = "json";

        /// <summary>
        /// Timeout in milliseconds for graceful shutdown (default: 30000ms = 30 seconds)
        /// </summary>
        public int ShutdownTimeoutMs { get; set; } = 30000;
    }
}
