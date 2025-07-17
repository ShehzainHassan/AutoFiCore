using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    public class BidStrategy
    {
        public int Id { get; set; }
        public BidStrategyType Type { get; set; } = BidStrategyType.Conservative;
        public int BidDelaySeconds { get; set; } = 5;
        public int MaxBidsPerMinute { get; set; } = 10;
        public PreferredBidTiming PreferredBidTiming { get; set; } = PreferredBidTiming.Immediate;
        public int SuccessfulBids { get; set; }
        public int FailedBids { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
