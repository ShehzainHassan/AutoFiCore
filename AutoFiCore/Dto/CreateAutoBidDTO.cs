using AutoFiCore.Enums;

public class CreateAutoBidDTO
{
    public int AuctionId { get; set; }
    public decimal MaxBidAmount { get; set; }
    public BidStrategyType BidStrategyType { get; set; }
    public bool IsActive { get; set; } = true;
}
