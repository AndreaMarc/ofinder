using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    /// <summary>
    /// User's favorite performers list
    /// UNIQUE constraint on (UserId, PerformerId) - one favorite per user per performer
    /// </summary>
    [Resource]
    [Table("UserFavorites")]
    public class UserFavorite : Identifiable<string>
    {
        [Attr]
        [Required]
        public string UserId { get; set; }

        [Attr]
        [Required]
        public string PerformerId { get; set; }

        [Attr]
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        [HasOne]
        public virtual User User { get; set; }

        [HasOne]
        public virtual Performer Performer { get; set; }
    }
}
