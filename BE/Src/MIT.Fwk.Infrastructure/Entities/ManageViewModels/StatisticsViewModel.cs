using System;
using System.ComponentModel.DataAnnotations;

namespace MIT.Fwk.Infrastructure.Entities.ManageViewModels
{
    public class StatisticsViewModel
    {
        public string TenantId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        [Required]
        public int RowPageNumber { get; set; }
        [Required]
        public int PageNumber { get; set; }

        public int FirstAccessDone { get; set; }

        public string SortingColumn { get; set; }

        public string SortingDirection { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public int TotalRows { get; set; }



    }


}
