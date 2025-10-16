using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrewManagerData.Models
{
    [Table("auth0_user")]
    public class Auth0User : ModelBase
    {
        [Required]
        [MaxLength(100)]
        public string Auth0UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        public bool EmailVerified { get; set; }

        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public bool PhoneVerified { get; set; }

        public DateTime Auth0CreatedAt { get; set; }

        public DateTime Auth0UpdatedAt { get; set; }

        [Column(TypeName = "jsonb")]
        public string AppMetadata { get; set; } = "{}";

        [Column(TypeName = "jsonb")]
        public string UserMetadata { get; set; } = "{}";

        [MaxLength(500)]
        public string Picture { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Nickname { get; set; } = string.Empty;

        [Column(TypeName = "jsonb")]
        public string Multifactor { get; set; } = "[]";

        [MaxLength(45)]
        public string LastIp { get; set; } = string.Empty;

        public DateTime? LastLogin { get; set; }

        public int LoginsCount { get; set; }

        public bool Blocked { get; set; }

        [MaxLength(100)]
        public string GivenName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FamilyName { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Auth0Identity> Identities { get; set; } = new List<Auth0Identity>();

        // Optional: Link to existing Profile if you want to connect Auth0 users to your existing user system
        public int? ProfileId { get; set; }

        [ForeignKey("ProfileId")]
        public virtual Profile? Profile { get; set; }
    }
}