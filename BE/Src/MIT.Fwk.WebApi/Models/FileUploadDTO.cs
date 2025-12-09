namespace MIT.Fwk.WebApi.Models
{
    public class FileUploadDTO
    {
        public string base64String { get; set; } // data:image/png;base64,contenuto
        public string base64Name { get; set; } // filename con estensione
        public string album { get; set; }
        public string alt { get; set; }
        public string category { get; set; }
        public string tag { get; set; }
        public string tenantId { get; set; }
        public string typologyArea { get; set; }
        public string uploadDate { get; set; }
        public string userGuid { get; set; }
        public string global { get; set; }
    }
}
