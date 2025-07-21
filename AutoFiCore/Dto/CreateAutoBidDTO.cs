using AutoFiCore.Enums;

public class CreateAutoBidDTO
{
    public int AuctionId { get; set; }
    public decimal MaxBidAmount { get; set; }
    public int UserId { get; set; }
    public BidStrategyType BidStrategyType { get; set; } = BidStrategyType.Conservative;
    public bool IsActive { get; set; } = true;
    public int? BidDelaySeconds { get; set; } = 5;
    public int? MaxBidsPerMinute { get; set; } = 10;
    public PreferredBidTiming PreferredBidTiming { get; set; } = PreferredBidTiming.Immediate;
    public DateTime UpdatedAt { get; set; }

}