using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrewManagerData.Models;

[Table("boats")]
public class Boat : ModelBase
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    // Foreign key to Profile.LoginId
    [Required]
    [MaxLength(100)]
    public string ProfileLoginId { get; set; } = string.Empty;

    // Navigation property to Profile
    [ForeignKey("ProfileLoginId")]
    public virtual Profile Profile { get; set; } = null!;

    // Navigation property - one boat can have many schedules
    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    // Navigation property - one boat can have many crew members
    public virtual ICollection<BoatCrew> BoatCrews { get; set; } = new List<BoatCrew>();
}