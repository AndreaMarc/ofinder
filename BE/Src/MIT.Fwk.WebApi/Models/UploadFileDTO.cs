using System;

namespace MIT.Fwk.WebApi.Models
{
    /// <summary>
    /// DTO for UploadFile entity - Simple POCO without legacy BaseDTO pattern.
    /// AutoMapper configuration now in AllMappingProfile.cs
    /// </summary>
    public class UploadFileDTO
    {
        public UploadFileDTO()
        { }

        public Guid? UploadId { get; set; }

        public string UploadUid { get; set; }

        public string UploadType { get; set; }

        public string UploadSrc { get; set; }

        public string UploadFileName { get; set; }

        public short UploadActive { get; set; }

        public DateTime? UploadCreationDate { get; set; }

        public string UploadAlbum { get; set; }

        public string UploadExtension { get; set; }

        public string UploadFileSize { get; set; }

    }
}

