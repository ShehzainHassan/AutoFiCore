using AutoFiCore.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoFiCore.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        [ForeignKey("Auction")]
        public int? AuctionId { get; set; }  
        public Auction? Auction { get; set; }
        public NotificationType NotificationType { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? EmailSentAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
