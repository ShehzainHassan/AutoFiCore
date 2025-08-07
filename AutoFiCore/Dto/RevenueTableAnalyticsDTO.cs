namespace AutoFiCore.Dto
{
    public class RevenueTableAnalyticsDTO
    {
        public int AuctionId { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public string Vehicle { get; set; } = string.Empty;
        public string Buyer { get; set; } = "---";
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
    }
}
