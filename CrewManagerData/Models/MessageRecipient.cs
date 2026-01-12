using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CrewManagerData.Models
{
    [Table("MessageRecipient")]
    public class MessageRecipient : ModelBase
    {
        [Required]
        public int MessageId { get; set; }

        [ForeignKey("MessageId")]
        [JsonIgnore] // Prevent cycles when serializing from Message side
        public virtual Message Message { get; set; } = null!;

        [Required]
        public int RecipientProfileId { get; set; }

        [ForeignKey("RecipientProfileId")]
        public virtual Profile Recipient { get; set; } = null!;

        public bool IsRead { get; set; } = false;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Sent, Delivered, Failed

        [MaxLength(100)]
        public string? ExternalReferenceId { get; set; } // ID from Email/SMS provider

        public string? FailureReason { get; set; }
    }
}
