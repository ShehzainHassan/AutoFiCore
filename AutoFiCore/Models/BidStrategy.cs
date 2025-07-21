using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    public class BidStrategy
    {
        public int AuctionId { get; set; }
        public int UserId { get; set; }
        public BidStrategyType Type { get; set; } = BidStrategyType.Conservative;
        public int? BidDelaySeconds { get; set; } = 5;
        public int? MaxBidsPerMinute { get; set; } = 10;
        public PreferredBidTiming PreferredBidTiming { get; set; } = PreferredBidTiming.Immediate;
        public int SuccessfulBids { get; set; } = 0;
        public int FailedBids { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Auction Auction { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
