using AutoFiCore.Enums;

namespace AutoFiCore.Dto
{
    public class UserAutoBidSettings
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AuctionId { get; set; }
        public decimal MaxBidAmount { get; set; }
        public BidStrategyType BidStrategyType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? BidDelaySeconds { get; set; }
        public int? MaxBidsPerMinute { get; set; }
        public int? MaxSpreadBids { get; set; }
        public PreferredBidTiming PreferredBidTiming { get; set; }
    }
}
