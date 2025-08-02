namespace AutoFiCore.Dto
{
    public class CategoryPerformance
    {
        public string CategoryName { get; set; } = string.Empty;
        public int AuctionCount { get; set; }
        public double SuccessRate { get; set; }
        public decimal AveragePrice { get; set; }
    }
}
