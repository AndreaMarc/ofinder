using MIT.Fwk.Core.Domain.Interfaces;

namespace MIT.Fwk.Core.Models
{
    public class DocumentFile : IDocument
    {
        public long Id { get; set; }

        public int TenantId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string FileName { get; set; }
        public string Extension { get; set; }

        public string Meta { get; set; }

        public string FileGuid { get; set; }
        public string SmallFormat { get; set; }
        public string MediumFormat { get; set; }

        public string BigFormat { get; set; }
        public string FileBase64 { get; set; }
        public byte[] BinaryData { get; set; }
    }
}
