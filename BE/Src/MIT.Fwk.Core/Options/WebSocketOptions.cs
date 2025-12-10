namespace MIT.Fwk.Core.Options
{
    /// <summary>
    /// WebSocket configuration options.
    /// Maps to "WebSocket" section in customsettings.json
    /// </summary>
    public class WebSocketOptions
    {
        public string LaunchUrl { get; set; } = "http://*:7003/";
    }
}
