namespace MIT.Fwk.Core.Options
{
    /// <summary>
    /// File upload configuration options.
    /// Maps to UploadsPath and related settings in customsettings.json
    /// </summary>
    public class FileUploadOptions
    {
        public string UploadsPath { get; set; } = "./uploads";
        public long MaxFileSize { get; set; } = 10485760;  // 10MB default
        public string AllowedExtensions { get; set; } = ".jpg,.jpeg,.png,.pdf,.doc,.docx,.xls,.xlsx";
        public string ImageExtensions { get; set; } = ".jpg,.jpeg,.png,.gif,.bmp";
    }
}
