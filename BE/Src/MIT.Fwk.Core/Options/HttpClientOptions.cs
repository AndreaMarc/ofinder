namespace MIT.Fwk.Core.Options
{
    /// <summary>
    /// HTTP client configuration options.
    /// For configuring HttpClient timeout, base URLs, etc.
    /// </summary>
    public class HttpClientOptions
    {
        public int TimeoutSeconds { get; set; } = 30;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
