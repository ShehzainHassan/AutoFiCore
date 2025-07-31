using AutoFiCore.Enums;

namespace AutoFiCore.Dto
{
    public class NotificationDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? AuctionId { get; set; }

    }
}
