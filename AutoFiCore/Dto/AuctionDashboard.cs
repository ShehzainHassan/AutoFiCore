namespace AutoFiCore.Dto
{
    public class AuctionDashboard
    {
        public double CompletionRate { get; set; }
        public double AverageBidCount { get; set; }
        public List<string> TopItems { get; set; } = new();
    }
}
