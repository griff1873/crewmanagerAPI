using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CrewManagerData.Models;

[Table("schedules")]
public class Schedule : ModelBase
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    // Foreign key to Boat
    [Required]
    public int BoatId { get; set; }

    // Navigation property to Boat
    [ForeignKey("BoatId")]
    public virtual Boat Boat { get; set; } = null!;

    // Navigation property - one schedule can have many events
    [JsonIgnore]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}