namespace AutoFiCore.Models
{
    public class AuctionAnalytics
    {
        public int Id { get; set; }

        public int AuctionId { get; set; }
        public virtual Auction Auction { get; set; } = null!;

        public int TotalViews { get; set; } = 0;
        public int UniqueBidders { get; set; } = 0;
        public int TotalBids { get; set; } = 0;
        public bool? CompletionStatus { get; set; }

        public decimal? ViewToBidRatio { get; set; }

        public decimal? StartPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public TimeSpan? Duration { get; set; }

        public double? SuccessRate { get; set; }  
        public double? EngagementScore { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
