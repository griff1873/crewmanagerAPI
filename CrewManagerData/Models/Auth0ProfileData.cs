using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrewManagerData.Models
{
    [Table("auth0_profile_data")]
    public class Auth0ProfileData : ModelBase
    {
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        public bool EmailVerified { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(100)]
        public string GivenName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FamilyName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public bool PhoneVerified { get; set; }

        // Navigation property - one profile data can be referenced by multiple identities
        public virtual ICollection<Auth0Identity> Identities { get; set; } = new List<Auth0Identity>();
    }
}