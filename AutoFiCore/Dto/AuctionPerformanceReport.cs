namespace AutoFiCore.Dto
{
    public class AuctionPerformanceReport
    {
        public int TotalAuctions { get; set; }
        public double SuccessRate { get; set; }
        public decimal AverageViews { get; set; }
        public decimal AverageBids { get; set; }
        public decimal AverageFinalPrice { get; set; }

    }
}

