using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CrewManagerData.Models;

[Table("boats")]
public class Boat : ModelBase
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(3)]
    public string ShortName { get; set; }

    public string CalendarColor { get; set; }


    // Foreign key to Profile.Id
    [Required]
    public int ProfileId { get; set; }

    public string? Image { get; set; }

    // Navigation property to Profile
    [ForeignKey("ProfileId")]
    public virtual Profile Profile { get; set; } = null!;



    // Navigation property - one boat can have many events
    [JsonIgnore]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    // Navigation property - one boat can have many crew members
    [JsonIgnore]
    public virtual ICollection<BoatCrew> BoatCrews { get; set; } = new List<BoatCrew>();
}