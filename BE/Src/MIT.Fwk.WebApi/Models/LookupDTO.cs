using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MIT.Fwk.WebApi.Models
{
    /// <summary>
    /// DTO for lookup/query results - Simple POCO without legacy BaseDTO pattern.
    /// </summary>
    [DataContract(Namespace = "")]
    public class LookupDTO
    {
        public LookupDTO() { }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string[] Resources { get; set; }
        [DataMember]
        public int PageNumber { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public List<LookupDataDTO> Data { get; set; }
        [DataMember]
        public List<LookupMetaDTO> Meta { get; set; }
        [DataMember]
        public long TotalRecords { get; set; }
    }

    [CollectionDataContract]
    public class LookupDataDTO : List<object>
    {
        public LookupDataDTO()
        {
        }

        public LookupDataDTO(IEnumerable<object> collection)
        {
            foreach (object item in collection)
            {
                this.Add(item);
            }
        }
    }

    [DataContract(Namespace = "")]
    public class LookupMetaDTO
    {
        public LookupMetaDTO() { }

        [DataMember]
        public string Alias { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool Hidden { get; set; }
        [DataMember]
        public bool IsKey { get; set; }
        [DataMember]
        public string TypeName { get; set; }

        public override string ToString()
        {
            return this.ToString();
        }
    }

}
