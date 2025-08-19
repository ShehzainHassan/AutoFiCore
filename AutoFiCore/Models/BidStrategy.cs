using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    /// <summary>
    /// Defines the bidding strategy configuration for a user in a specific auction.
    /// </summary>
    public class BidStrategy
    {
        /// <summary>
        /// ID of the auction this strategy applies to.
        /// </summary>
        public int AuctionId { get; set; }

        /// <summary>
        /// ID of the user using this strategy.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The type of bidding strategy (e.g., Conservative, Aggressive).
        /// </summary>
        public BidStrategyType Type { get; set; } = BidStrategyType.Conservative;

        /// <summary>
        /// Delay in seconds before placing a bid.
        /// </summary>
        public int? BidDelaySeconds { get; set; } = 5;

        /// <summary>
        /// Maximum number of bids allowed per minute.
        /// </summary>
        public int? MaxBidsPerMinute { get; set; } = 10;

        /// <summary>
        /// Maximum number of spread bids allowed across time intervals.
        /// </summary>
        public int? MaxSpreadBids { get; set; }

        /// <summary>
        /// Preferred timing for placing bids (e.g., Immediate, LastSecond).
        /// </summary>
        public PreferredBidTiming PreferredBidTiming { get; set; } = PreferredBidTiming.Immediate;

        /// <summary>
        /// Count of successful bids placed using this strategy.
        /// </summary>
        public int SuccessfulBids { get; set; } = 0;

        /// <summary>
        /// Count of failed bids due to strategy constraints or auction rules.
        /// </summary>
        public int FailedBids { get; set; } = 0;

        /// <summary>
        /// UTC timestamp when the strategy was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UTC timestamp when the strategy was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for the associated auction.
        /// </summary>
        public Auction Auction { get; set; } = null!;

        /// <summary>
        /// Navigation property for the user applying the strategy.
        /// </summary>
        public User User { get; set; } = null!;
    }
}