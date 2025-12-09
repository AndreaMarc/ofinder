namespace MIT.Fwk.Core.Options
{
    /// <summary>
    /// Logging configuration options.
    /// Maps to "Logging" section in customsettings.json
    /// </summary>
    public class LogOptions
    {
        public bool LogWithBus { get; set; } = true;
        public string LogCollection { get; set; } = "FwkLog";
        public int LogLevel { get; set; } = 10;
    }
}
