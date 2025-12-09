using System.Threading.Tasks;

namespace MIT.Fwk.Scheduler.Services
{
    /// <summary>
    /// Facade service for processing log files and saving to MongoDB
    /// </summary>
    public interface ILogFileProcessor
    {
        /// <summary>
        /// Processes log files from the configured directory
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        Task ProcessLogsAsync();
    }
}
