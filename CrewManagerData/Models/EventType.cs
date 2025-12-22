using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrewManagerData.Models
{
    [Table("event_type")]
    public class EventType : ModelBase
    {
        public int? ProfileId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
    }
}