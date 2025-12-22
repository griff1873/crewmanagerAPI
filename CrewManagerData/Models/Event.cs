using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrewManagerData.Models;

[Table("events")]
public class Event : ModelBase
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    [MaxLength(300)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public int MinCrew { get; set; }

    public int MaxCrew { get; set; }

    public int DesiredCrew { get; set; }

    // Required foreign key to Boat
    [Required]
    public int BoatId { get; set; }

    // Required foreign key to EventType
    [Required]
    public int EventTypeId { get; set; }

    // Navigation properties
    [ForeignKey("BoatId")]
    public virtual Boat Boat { get; set; } = null!;

    [ForeignKey("EventTypeId")]
    public virtual EventType EventType { get; set; } = null!;
}