using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    public class AnalyticsEvent
    {
        public int Id { get; set; }
        public AnalyticsEventType EventType { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public int? AuctionId { get; set; }
        public Auction? Auction { get; set; }

        public string? EventData { get; set; }
        public AnalyticsSource? Source { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
