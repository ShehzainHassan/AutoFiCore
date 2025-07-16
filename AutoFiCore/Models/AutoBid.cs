using AutoFiCore.Models;
using System.ComponentModel.DataAnnotations;
using AutoFiCore.Enums;
public class AutoBid
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AuctionId { get; set; }
    public User User { get; set; } = null!;
    public Auction Auction { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal MaxBidAmount { get; set; }
    public decimal CurrentBidAmount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public BidStrategyType BidStrategyType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }
}
