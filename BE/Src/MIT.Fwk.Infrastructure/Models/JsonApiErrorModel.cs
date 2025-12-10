using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Models
{
    public class JsonApiErrorModel
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }

    }
}
