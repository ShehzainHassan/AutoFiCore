namespace AutoFiCore.Models
{
    /// <summary>
    /// Represents analytical metrics and performance indicators for a specific auction.
    /// </summary>
    public class AuctionAnalytics
    {
        /// <summary>
        /// The unique identifier for the analytics record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The ID of the auction associated with these analytics.
        /// </summary>
        public int AuctionId { get; set; }

        /// <summary>
        /// The auction entity linked to this analytics record.
        /// </summary>
        public virtual Auction Auction { get; set; } = null!;

        /// <summary>
        /// The total number of views the auction received.
        /// </summary>
        public int TotalViews { get; set; } = 0;

        /// <summary>
        /// The number of unique users who placed bids.
        /// </summary>
        public int UniqueBidders { get; set; } = 0;

        /// <summary>
        /// The total number of bids placed during the auction.
        /// </summary>
        public int TotalBids { get; set; } = 0;

        /// <summary>
        /// Indicates whether the auction was completed successfully.
        /// </summary>
        public bool? CompletionStatus { get; set; }

        /// <summary>
        /// The ratio of views to bids, used to measure conversion.
        /// </summary>
        public decimal? ViewToBidRatio { get; set; }

        /// <summary>
        /// The starting price of the auctioned item.
        /// </summary>
        public decimal? StartPrice { get; set; }

        /// <summary>
        /// The final price at which the auction closed.
        /// </summary>
        public decimal? FinalPrice { get; set; }

        /// <summary>
        /// The total duration of the auction.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// A calculated metric representing the success rate of the auction.
        /// </summary>
        public double? SuccessRate { get; set; }

        /// <summary>
        /// A calculated metric representing user engagement during the auction.
        /// </summary>
        public double? EngagementScore { get; set; }

        /// <summary>
        /// The UTC timestamp when the analytics record was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}