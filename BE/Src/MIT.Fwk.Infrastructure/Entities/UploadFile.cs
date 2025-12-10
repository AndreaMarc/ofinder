using System;

namespace MIT.Fwk.Infrastructure.Entities
{
    public partial class UploadFile
    {
        public UploadFile()
        {
        }

        public UploadFile(Guid? uploadId)
        {
            UploadId = uploadId;
        }

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
