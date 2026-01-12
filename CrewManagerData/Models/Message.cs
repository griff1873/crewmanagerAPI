using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CrewManagerData.Models
{
    [Table("Message")]
    public class Message : ModelBase
    {
        [Required]
        public int SenderProfileId { get; set; }

        [ForeignKey("SenderProfileId")]
        public virtual Profile Sender { get; set; } = null!;

        [MaxLength(255)]
        public string? Subject { get; set; }

        [Required]
        public string Body { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Channel { get; set; } = "InApp"; // Email, SMS, InApp

        public int? TargetEventId { get; set; } // Optional: Link message to an Event

        // Threading
        public int? ParentMessageId { get; set; }
        [ForeignKey("ParentMessageId")]
        public virtual Message? ParentMessage { get; set; }

        public int? RootMessageId { get; set; }
        [ForeignKey("RootMessageId")]
        public virtual Message? RootMessage { get; set; }

        // Navigation for replies
        [JsonIgnore]
        public virtual ICollection<Message> Replies { get; set; } = new List<Message>();

        // Navigation for recipients
        public virtual ICollection<MessageRecipient> Recipients { get; set; } = new List<MessageRecipient>();
    }
}
