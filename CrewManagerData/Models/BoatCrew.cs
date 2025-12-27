using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CrewManagerData.Models;

[Table("boat_crew")]
public class BoatCrew : ModelBase
{
    // Foreign key to Profile.Id
    [Required]
    public int ProfileId { get; set; }

    // Foreign key to Boat.Id
    [Required]
    public int BoatId { get; set; }

    // Admin flag for this crew member on this boat
    public bool IsAdmin { get; set; } = false;
    [Required]
    [MaxLength(1)]
    public string Status { get; set; } = "P";

    // Status: "P"ENDING, "A"CCEPTED, "R"EJECTED
    [MaxLength(1)]
    public string Status { get; set; } = "P";

    // Navigation properties
    [ForeignKey("ProfileId")]
    public virtual Profile Profile { get; set; } = null!;

    [ForeignKey("BoatId")]
    public virtual Boat Boat { get; set; } = null!;
}