using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrewManagerData.Models
{
    [Table("profile")]
    public class Profile : ModelBase
    {
        [Required]
        [MaxLength(100)]
        public string LoginId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // Navigation property - one profile can have many boats (as owner)
        public virtual ICollection<Boat> Boats { get; set; } = new List<Boat>();

        // Navigation property - one profile can be crew on many boats
        public virtual ICollection<BoatCrew> BoatCrews { get; set; } = new List<BoatCrew>();
    }
}