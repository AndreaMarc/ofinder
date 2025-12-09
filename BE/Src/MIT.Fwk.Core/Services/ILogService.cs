namespace MIT.Fwk.Core.Services
{
    /// <summary>
    /// Logging service abstraction.
    /// Provides structured logging with context support.
    /// Replaces static LogHelper with dependency injection pattern.
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="logContext">Optional context name (default: "MIT")</param>
        void Info(string message, string logContext = null);

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="logContext">Optional context name (default: "MIT")</param>
        void Debug(string message, string logContext = null);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="logContext">Optional context name (default: "MIT")</param>
        void Warn(string message, string logContext = null);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="logContext">Optional context name (default: "MIT")</param>
        void Error(string message, string logContext = null);

        /// <summary>
        /// Logs a fatal error message.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="logContext">Optional context name (default: "MIT")</param>
        void Fatal(string message, string logContext = null);

        /// <summary>
        /// Logs to MongoDB-specific context.
        /// Used for log aggregation/bus operations.
        /// </summary>
        /// <param name="message">Message to log</param>
        void ForMongo(string message);

        /// <summary>
        /// Writes message to console output.
        /// </summary>
        /// <param name="message">Message to write</param>
        void ToConsole(string message);
    }
}
