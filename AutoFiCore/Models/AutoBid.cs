using AutoFiCore.Models;
using System.ComponentModel.DataAnnotations;
using AutoFiCore.Enums;

/// <summary>
/// Represents an automated bidding configuration for a user in a specific auction.
/// </summary>
public class AutoBid
{
    /// <summary>
    /// Unique identifier for the AutoBid entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The ID of the user who configured the auto-bid.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The ID of the auction this auto-bid is associated with.
    /// </summary>
    public int AuctionId { get; set; }

    /// <summary>
    /// The maximum amount the user is willing to bid automatically.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "MaxBidAmount must be non-negative.")]
    public decimal MaxBidAmount { get; set; }

    /// <summary>
    /// The current bid amount placed by the auto-bid logic.
    /// </summary>
    public decimal CurrentBidAmount { get; set; } = 0;

    /// <summary>
    /// Indicates whether the auto-bid is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The bidding strategy used to determine how bids are placed.
    /// </summary>
    public BidStrategyType BidStrategyType { get; set; } = BidStrategyType.Conservative;

    /// <summary>
    /// Timestamp when the auto-bid was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the auto-bid was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the auto-bid was executed, if applicable.
    /// </summary>
    public DateTime? ExecutedAt { get; set; }

    /// <summary>
    /// Navigation property for the associated user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Navigation property for the associated auction.
    /// </summary>
    public Auction Auction { get; set; } = null!;
}