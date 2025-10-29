using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CrewManagerData.Models
{
    [Table("profile")]
    public class Profile : ModelBase
    {
        public string LoginId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        // Navigation property - one profile can have many boats (as owner)
        public virtual ICollection<Boat> Boats { get; set; } = new List<Boat>();

        // Navigation property - one profile can be crew on many boats
        public virtual ICollection<BoatCrew> BoatCrews { get; set; } = new List<BoatCrew>();
    }
}