using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrewManagerData.Models
{
    [Table("auth0_identity")]
    public class Auth0Identity : ModelBase
    {
        [Required]
        [MaxLength(100)]
        public string Provider { get; set; } = string.Empty;

        public bool IsSocial { get; set; }

        [Required]
        [MaxLength(200)]
        public string Connection { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        // Foreign key to Auth0ProfileData
        public int? ProfileDataId { get; set; }

        // Navigation property
        [ForeignKey("ProfileDataId")]
        public virtual Auth0ProfileData? ProfileData { get; set; }

        // Foreign key to link with main user/profile if needed
        [MaxLength(100)]
        public string? Auth0UserId { get; set; }
    }
}