using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrewManagerData.Models;

[Table("crew_events")]
public class CrewEvent : ModelBase
{
    // Foreign key to Event.Id
    [Required]
    public int EventId { get; set; }

    // Foreign key to Profile.Id
    [Required]
    public int ProfileId { get; set; }

    [Required]
    [MaxLength(20)]
    // Status: "Pending", "In", "Out", "Maybe"
    public string Status { get; set; } = "Pending";

    // Navigation properties
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("ProfileId")]
    public virtual Profile Profile { get; set; } = null!;
}
