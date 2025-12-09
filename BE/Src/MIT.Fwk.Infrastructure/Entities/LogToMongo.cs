namespace MIT.Fwk.Infrastructure.Entities
{
    public class LogToMongo
    {
        public string Model { get; set; }
        public MITApplicationUser CurrentUser { get; set; }
        public string RoutePath { get; set; }
        public string Headers { get; set; }
        public string PayLoad { get; set; }
        public string RequestMethod { get; set; }
        public string LogType { get; set; }
    }
}
