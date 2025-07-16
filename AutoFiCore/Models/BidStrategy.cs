using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    public class BidStrategy
    {
        public int Id { get; set; }
        public BidStrategyType Type { get; set; }
        public int BidDelaySeconds { get; set; }
        public int MaxBidsPerMinute { get; set; }
        public PreferredBidTiming PreferredBidTiming { get; set; }
        public int SuccessfulBids { get; set; }
        public int FailedBids { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
