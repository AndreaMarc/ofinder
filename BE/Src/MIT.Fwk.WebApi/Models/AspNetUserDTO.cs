using System;

namespace MIT.Fwk.WebApi.Models
{
    /// <summary>
    /// DTO for AspNetUser entity - Simple POCO without legacy BaseDTO pattern.
    /// AutoMapper configuration now in AllMappingProfile.cs
    /// </summary>
    public class AspNetUserDTO
    {

        public String Id { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public DateTime? LastAccess { get; set; }

        public DateTime PasswordLastChange { get; set; }

    }
}
