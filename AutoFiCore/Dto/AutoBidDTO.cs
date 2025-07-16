using AutoFiCore.Enums;
public class AutoBidDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AuctionId { get; set; }
    public decimal MaxBidAmount { get; set; }
    public bool IsActive { get; set; }
    public BidStrategyType BidStrategyType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

}
